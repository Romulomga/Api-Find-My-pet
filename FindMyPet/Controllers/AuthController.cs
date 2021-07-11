using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FindMyPet.Controllers.Base;
using FindMyPet.Business.Interfaces;
using FindMyPet.Business.Models;
using FindMyPet.Helpers;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Newtonsoft.Json;
using AppleAuth;
using FindMyPet.Dto.Requests;
using FindMyPet.Dto.Responses;
using FindMyPet.Services;

namespace FindMyPet.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly FacebookAuthSettings FacebookAuthSettings;
        private readonly AuthenticationService AuthenticationService;

        public AuthController(INotificator notificator, IJwtUser user, IOptions<FacebookAuthSettings> facebookAuthSettings, AuthenticationService authenticationService) : base(notificator, user)
        {
            this.FacebookAuthSettings = facebookAuthSettings.Value;
            this.AuthenticationService = authenticationService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new User
            {
                UserName = userRegisterDto.UserName,
                Email = userRegisterDto.Email
            };

            var result = await AuthenticationService.UserManager.CreateAsync(user, userRegisterDto.Password);

            if (result.Succeeded)
            {
                user = await AuthenticationService.UserManager.FindByEmailAsync(user.Email);
                await AuthenticationService.SignInManager.SignInAsync(user, false);

                var response = await AuthenticationService.GetUserLoginResponse(user);

                return CustomResponse(response);
            }
            foreach (var error in result.Errors)
            {
                NotificateError(error.Description);
            }

            return CustomResponse();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await AuthenticationService.UserManager.FindByEmailAsync(userLogin.Email);

            if (user != null)
            {
                var result = await AuthenticationService.SignInManager.PasswordSignInAsync(user, userLogin.Password, false, true);

                if (result.Succeeded)
                {
                    var response = await AuthenticationService.GetUserLoginResponse(user);

                    return CustomResponse(response);
                }
                if (result.IsLockedOut)
                {
                    NotificateError("Usuário temporariamente bloqueado por tentativas inválidas");
                    return CustomResponse();
                }
            }
            
            NotificateError("Usuário ou Senha incorretos");
            return CustomResponse();
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                NotificateError("Refresh Token inválido");
                return CustomResponse();
            }

            var token = await AuthenticationService.CheckRefreshToken(Guid.Parse(refreshToken));

            if (token != null)
            {
                var user = await AuthenticationService.UserManager.FindByEmailAsync(token.Email);

                if (user != null)
                {
                    return CustomResponse(await AuthenticationService.GetUserLoginResponse(user));
                }
            }

            NotificateError("Refresh Token expirado");

            return CustomResponse();
        }

        [HttpPost("login/facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> Facebook(SocialLoginDto facebook)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            // 1.generate an app access token
            var appAccessTokenResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={FacebookAuthSettings.AppId}&client_secret={FacebookAuthSettings.AppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            // 2. validate the user access token
            var userAccessTokenValidationResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/debug_token?input_token={facebook.AccessToken}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                NotificateError("Token inválido.");
                return CustomResponse();
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/v10.0/me?fields=id,email,name,picture&access_token={facebook.AccessToken}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            // 4. ready to create the local user account (if necessary) and jwt
            var user = await AuthenticationService.UserManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = userInfo.Name,
                    Email = userInfo.Email,
                    AvatarUrl = userInfo.Picture.Data.Url
                };

                var result = await AuthenticationService.UserManager.CreateAsync(user, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));

                if (result.Succeeded)
                {
                    user = await AuthenticationService.UserManager.FindByEmailAsync(user.Email);
                    await AuthenticationService.SignInManager.SignInAsync(user, false);

                    var response = await AuthenticationService.GetUserLoginResponse(user);

                    return CustomResponse(response);
                }

                foreach (var error in result.Errors)
                {
                    NotificateError(error.Description);
                }

                return CustomResponse();
            } 
            else 
            {
                await AuthenticationService.SignInManager.SignInAsync(user, false);
                var response = await AuthenticationService.GetUserLoginResponse(user);

                return CustomResponse(response);
            }
        }

        [HttpPost("login/apple")]
        [AllowAnonymous]
        public async Task<IActionResult> Apple(SocialLoginDto apple)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var privateKey = System.IO.File.ReadAllText("path/to/file.p8");
            var provider = new AppleAuthProvider("MyClientID", "MyTeamID", "MyKeyID", "https://myredirecturl.com/HandleResponseFromApple", "SomeState");

            var appleRefreshToken = await provider.GetRefreshToken(apple.AccessToken, privateKey);

            var user = await AuthenticationService.UserManager.FindByEmailAsync(appleRefreshToken.UserInformation.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = apple.UserName,
                    Email = apple.Email
                };

                var result = await AuthenticationService.UserManager.CreateAsync(user, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));

                if (result.Succeeded)
                {
                    user = await AuthenticationService.UserManager.FindByEmailAsync(user.Email);
                    await AuthenticationService.SignInManager.SignInAsync(user, false);

                    var response = await AuthenticationService.GetUserLoginResponse(user);

                    return CustomResponse(response);
                }

                foreach (var error in result.Errors)
                {
                    NotificateError(error.Description);
                }

                return CustomResponse();
            } 
            else
            {
                await AuthenticationService.SignInManager.SignInAsync(user, false);
                var response = await AuthenticationService.GetUserLoginResponse(user);

                return CustomResponse(response);
            }
        }
    }
}