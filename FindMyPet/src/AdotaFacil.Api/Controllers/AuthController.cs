using AppleAuth;
using AdotaFacil.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using AdotaFacil.Api.Settings;
using AdotaFacil.Api.Services;
using AdotaFacil.Api.Dto.Requests;
using AdotaFacil.Api.Controllers.Base;
using AdotaFacil.Business.Models;

namespace AdotaFacil.Api.Controllers
{
    [Authorize]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly FacebookAuthSettings FacebookAuthSettings;
        private readonly AuthenticationService AuthenticationService;
        private readonly SignInManager<User> SignInManager;
        private readonly UserManager<User> UserManager;

        public AuthController(INotificator notificator, IJwtUser user, IOptions<FacebookAuthSettings> facebookAuthSettings, AuthenticationService authenticationService, UserManager<User> userManager, SignInManager<User> signInManager) : base(notificator, user)
        {
            this.FacebookAuthSettings = facebookAuthSettings.Value;
            this.AuthenticationService = authenticationService;
            this.UserManager = userManager;
            this.SignInManager = signInManager;
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

            var result = await UserManager.CreateAsync(user, userRegisterDto.Password);

            if (result.Succeeded)
            {
                user = await UserManager.FindByEmailAsync(user.Email);
                await SignInManager.SignInAsync(user, false);

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

            var user = await UserManager.FindByEmailAsync(userLogin.Email);

            if (user != null)
            {
                var result = await SignInManager.PasswordSignInAsync(user, userLogin.Password, false, true);

                if (result.Succeeded)
                {
                    var response = await AuthenticationService.GetUserLoginResponse(user);

                    return CustomResponse(response);
                }
                if (result.IsLockedOut)
                {
                    NotificateError("Usu�rio temporariamente bloqueado por tentativas inv�lidas");
                    return CustomResponse();
                }
            }

            NotificateError("Usu�rio ou Senha incorretos");
            return CustomResponse();
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                NotificateError("Refresh Token inv�lido");
                return CustomResponse();
            }

            var token = await AuthenticationService.CheckRefreshToken(Guid.Parse(refreshToken));

            if (token != null)
            {
                var user = await UserManager.FindByEmailAsync(token.Email);

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

            // 1. generate an app access token
            var appAccessTokenResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={FacebookAuthSettings.AppId}&client_secret={FacebookAuthSettings.AppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            // 2. validate the user access token
            var userAccessTokenValidationResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/debug_token?input_token={facebook.AccessToken}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                NotificateError("Token inv�lido.");
                return CustomResponse();
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/v10.0/me?fields=id,email,name,picture&access_token={facebook.AccessToken}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            // 4. ready to create the local user account (if necessary) and jwt
            var user = await UserManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = userInfo.Name,
                    Email = userInfo.Email,
                    AvatarUrl = userInfo.Picture.Data.Url
                };

                var result = await UserManager.CreateAsync(user, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));

                if (result.Succeeded)
                {
                    user = await UserManager.FindByEmailAsync(user.Email);
                    await SignInManager.SignInAsync(user, false);

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
                await SignInManager.SignInAsync(user, false);
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

            var user = await UserManager.FindByEmailAsync(appleRefreshToken.UserInformation.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = apple.UserName,
                    Email = apple.Email
                };

                var result = await UserManager.CreateAsync(user, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));

                if (result.Succeeded)
                {
                    user = await UserManager.FindByEmailAsync(user.Email);
                    await SignInManager.SignInAsync(user, false);

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
                await SignInManager.SignInAsync(user, false);
                var response = await AuthenticationService.GetUserLoginResponse(user);

                return CustomResponse(response);
            }
        }
    }
}