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

namespace FindMyPet.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly UserManager<User> UserManager;
        private readonly SignInManager<User> SignInManager;
        private readonly IMapper Mapper;
        private readonly JwtSettings JwtSettings;
        private readonly FacebookAuthSettings FacebookAuthSettings;

        public AuthController(INotificator notificator, UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, IJwtUser user, IOptions<JwtSettings> jwtSettings, IOptions<FacebookAuthSettings> facebookAuthSettings) : base(notificator, user)
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
            this.Mapper = mapper;
            this.JwtSettings = jwtSettings.Value;
            this.FacebookAuthSettings = facebookAuthSettings.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await UserManager.FindByEmailAsync(userLogin.Email);

            if (User != null)
            {
                var result = await SignInManager.PasswordSignInAsync(user, userLogin.Password, false, true);

                if (result.Succeeded)
                {
                    return CustomResponse(await GerarJwt(user));
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

                    return CustomResponse(await GerarJwt(user));
                }

                foreach (var error in result.Errors)
                {
                    NotificateError(error.Description);
                }

                return CustomResponse();
            }

            await SignInManager.SignInAsync(user, false);

            return CustomResponse(await GerarJwt(user));
        }

        [HttpPost("login/apple")]
        [AllowAnonymous]
        public async Task<IActionResult> Apple(SocialLoginDto apple)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var privateKey = System.IO.File.ReadAllText("path/to/file.p8");
            var provider = new AppleAuthProvider("MyClientID", "MyTeamID", "MyKeyID", "https://myredirecturl.com/HandleResponseFromApple", "SomeState");

            var refreshToken = await provider.GetRefreshToken(apple.AccessToken, privateKey);

            var user = await UserManager.FindByEmailAsync(refreshToken.UserInformation.Email);

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

                    return CustomResponse(await GerarJwt(user));
                }

                foreach (var error in result.Errors)
                {
                    NotificateError(error.Description);
                }

                return CustomResponse();
            }

            await SignInManager.SignInAsync(user, false);

            return CustomResponse(await GerarJwt(user));
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

                return CustomResponse(await GerarJwt(user));
            }
            foreach (var error in result.Errors)
            {
                NotificateError(error.Description);
            }

            return CustomResponse();
        }

        private async Task<UserLoginResponseDto> GerarJwt(User user)
        {
            var claims = await UserManager.GetClaimsAsync(user);
            var userRoles = await UserManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = JwtSettings.Issuer,
                Audience = JwtSettings.Audience,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(JwtSettings.ExpirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);

            var response = new UserLoginResponseDto
            {
                Token = encodedToken,
                User = Mapper.Map<UserDto>(User)
            };

            return response;
        }

        private static long ToUnixEpochDate(DateTime Date) => (long)Math.Round((Date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}