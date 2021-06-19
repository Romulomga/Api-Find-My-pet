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
using FindMyPet.Dto;
using FindMyPet.Controllers.Base;
using FindMyPet.Business.Interfaces;
using FindMyPet.Models;
using FindMyPet.Helpers;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Newtonsoft.Json;
using FindMyPet.Models.Facebbok;
using FindMyPet.Dto.Login;
using AppleAuth;
using FindMyPet.Dto.Login.Responses;

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

        public AuthController(INotificator Notificator, UserManager<User> UserManager, SignInManager<User> SignInManager, IMapper Mapper, ITokenUser User, IOptions<JwtSettings> JwtSettings, IOptions<FacebookAuthSettings> FacebookAuthSettings) : base(Notificator, User)
        {
            this.UserManager = UserManager;
            this.SignInManager = SignInManager;
            this.Mapper = Mapper;
            this.JwtSettings = JwtSettings.Value;
            this.FacebookAuthSettings = FacebookAuthSettings.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto UserLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var User = await UserManager.FindByEmailAsync(UserLogin.Email);

            if (User != null)
            {
                var Result = await SignInManager.PasswordSignInAsync(User, UserLogin.Password, false, true);

                if (Result.Succeeded)
                {
                    return CustomResponse(await GerarJwt(User));
                }
                if (Result.IsLockedOut)
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
        public async Task<IActionResult> Facebook(SocialLoginDto Facebook)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            // 1.generate an app access token
            var AppAccessTokenResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={FacebookAuthSettings.AppId}&client_secret={FacebookAuthSettings.AppSecret}&grant_type=client_credentials");
            var AppAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(AppAccessTokenResponse);
            // 2. validate the user access token
            var UserAccessTokenValidationResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/debug_token?input_token={Facebook.AccessToken}&access_token={AppAccessToken.AccessToken}");
            var UserAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(UserAccessTokenValidationResponse);

            if (!UserAccessTokenValidation.Data.IsValid)
            {
                NotificateError("Token inválido.");
                return CustomResponse();
            }

            // 3. we've got a valid token so we can request user data from fb
            var UserInfoResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/v10.0/me?fields=id,email,name,picture&access_token={Facebook.AccessToken}");
            var UserInfo = JsonConvert.DeserializeObject<FacebookUserData>(UserInfoResponse);

            // 4. ready to create the local user account (if necessary) and jwt
            var User = await UserManager.FindByEmailAsync(UserInfo.Email);

            if (User == null)
            {
                User = new User
                {
                    UserName = UserInfo.Name,
                    Email = UserInfo.Email,
                    AvatarUrl = UserInfo.Picture.Data.Url
                };

                var Result = await UserManager.CreateAsync(User, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));

                if (Result.Succeeded)
                {
                    User = await UserManager.FindByEmailAsync(User.Email);
                    await SignInManager.SignInAsync(User, false);

                    return CustomResponse(await GerarJwt(User));
                }

                foreach (var error in Result.Errors)
                {
                    NotificateError(error.Description);
                }

                return CustomResponse();
            }

            await SignInManager.SignInAsync(User, false);

            return CustomResponse(await GerarJwt(User));
        }

        [HttpPost("login/apple")]
        [AllowAnonymous]
        public async Task<IActionResult> Apple(SocialLoginDto Apple)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var PrivateKey = System.IO.File.ReadAllText("path/to/file.p8");
            var Provider = new AppleAuthProvider("MyClientID", "MyTeamID", "MyKeyID", "https://myredirecturl.com/HandleResponseFromApple", "SomeState");

            var RefreshToken = await Provider.GetRefreshToken(Apple.AccessToken, PrivateKey);

            var User = await UserManager.FindByEmailAsync(RefreshToken.UserInformation.Email);

            if (User == null)
            {
                User = new User
                {
                    UserName = Apple.UserName,
                    Email = Apple.Email
                };

                var Result = await UserManager.CreateAsync(User, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));

                if (Result.Succeeded)
                {
                    User = await UserManager.FindByEmailAsync(User.Email);
                    await SignInManager.SignInAsync(User, false);

                    return CustomResponse(await GerarJwt(User));
                }

                foreach (var Error in Result.Errors)
                {
                    NotificateError(Error.Description);
                }

                return CustomResponse();
            }

            await SignInManager.SignInAsync(User, false);

            return CustomResponse(await GerarJwt(User));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterDto UserRegisterDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var User = new User
            {
                UserName = UserRegisterDto.UserName,
                Email = UserRegisterDto.Email
            };

            var Result = await UserManager.CreateAsync(User, UserRegisterDto.Password);

            if (Result.Succeeded)
            {
                User = await UserManager.FindByEmailAsync(User.Email);
                await SignInManager.SignInAsync(User, false);

                return CustomResponse(await GerarJwt(User));
            }
            foreach (var Error in Result.Errors)
            {
                NotificateError(Error.Description);
            }

            return CustomResponse();
        }

        private async Task<UserLoginResponseDto> GerarJwt(User User)
        {
            var Claims = await UserManager.GetClaimsAsync(User);
            var UserRoles = await UserManager.GetRolesAsync(User);

            Claims.Add(new Claim(JwtRegisteredClaimNames.Sub, User.Id.ToString()));
            Claims.Add(new Claim(JwtRegisteredClaimNames.Email, User.Email));
            Claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            Claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            Claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            foreach (var userRole in UserRoles)
            {
                Claims.Add(new Claim("role", userRole));
            }

            var IdentityClaims = new ClaimsIdentity();
            IdentityClaims.AddClaims(Claims);

            var TokenHandler = new JwtSecurityTokenHandler();
            var Key = Encoding.ASCII.GetBytes(JwtSettings.Secret);
            var Token = TokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = JwtSettings.Issuer,
                Audience = JwtSettings.Audience,
                Subject = IdentityClaims,
                Expires = DateTime.UtcNow.AddHours(JwtSettings.ExpirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Key), SecurityAlgorithms.HmacSha256Signature)
            });

            var EncodedToken = TokenHandler.WriteToken(Token);

            var Response = new UserLoginResponseDto
            {
                Token = EncodedToken,
                User = Mapper.Map<UserDto>(User)
            };

            return Response;
        }

        private static long ToUnixEpochDate(DateTime Date) => (long)Math.Round((Date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}