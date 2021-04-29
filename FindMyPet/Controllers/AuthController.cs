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

namespace FindMyPet.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;
        private readonly FacebookAuthSettings _facebookAuthSettings;

        public AuthController(INotificator notificator, UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, ITokenUser user, IOptions<JwtSettings> jwtSettings, IOptions<FacebookAuthSettings> facebookAuthSettings) : base(notificator, user)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
            _facebookAuthSettings = facebookAuthSettings.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await _userManager.FindByEmailAsync(userLogin.Email);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, userLogin.Password, false, true);

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
        public async Task<IActionResult> Facebook(UserFacebookLoginDto userFacebookLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            // 1.generate an app access token
            var appAccessTokenResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_facebookAuthSettings.AppId}&client_secret={_facebookAuthSettings.AppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            // 2. validate the user access token
            var userAccessTokenValidationResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/debug_token?input_token={userFacebookLogin.AccessToken}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                NotificateError("Token inválido.");
                return CustomResponse();
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await new HttpClient().GetStringAsync($"https://graph.facebook.com/v10.0/me?fields=id,email,name,picture&access_token={userFacebookLogin.AccessToken}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            // 4. ready to create the local user account (if necessary) and jwt
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = userInfo.Name,
                    Email = userInfo.Email,
                    AvatarUrl = userInfo.Picture.Data.Url
                };

                var result = await _userManager.CreateAsync(user, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));

                if (result.Succeeded)
                {
                    user = await _userManager.FindByEmailAsync(user.Email);
                    await _signInManager.SignInAsync(user, false);

                    return CustomResponse(await GerarJwt(user));
                }
                foreach (var error in result.Errors)
                {
                    NotificateError(error.Description);
                }

                return CustomResponse();
            }

            await _signInManager.SignInAsync(user, false);

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

            var result = await _userManager.CreateAsync(user, userRegisterDto.Password);

            if (result.Succeeded)
            {
                user = await _userManager.FindByEmailAsync(user.Email);
                await _signInManager.SignInAsync(user, false);

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
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

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
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);

            var response = new UserLoginResponseDto
            {
                Token = encodedToken,
                User = _mapper.Map<UserDto>(user)
            };

            return response;
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}