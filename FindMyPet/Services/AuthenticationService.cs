using AutoMapper;
using FindMyPet.Business.Interfaces;
using FindMyPet.Business.Models;
using FindMyPet.Data;
using FindMyPet.Dto.Responses;
using FindMyPet.Helpers;
using FindMyPet.Repository.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FindMyPet.Services
{
    public class AuthenticationService
    {
        public readonly SignInManager<User> SignInManager;
        public readonly UserManager<User> UserManager;
        private readonly JwtSettings JwtSettings;
        private readonly FacebookAuthSettings FacebookAuthSettings;
        private readonly RefreshTokenSettings RefreshTokenSettings;
        private readonly IdentityDbContext Context;
        private readonly IMapper Mapper;
        private readonly IJwtUser User;

        public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager, IJwtUser user, IOptions<JwtSettings> jwtSettings, IOptions<FacebookAuthSettings> facebookAuthSettings, IOptions<RefreshTokenSettings> refreshTokenSettings, IdentityDbContext context, IMapper mapper)
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
            this.JwtSettings = jwtSettings.Value;
            this.FacebookAuthSettings = facebookAuthSettings.Value;
            this.RefreshTokenSettings = refreshTokenSettings.Value;
            this.Context = context;
            this.Mapper = mapper;
            this.User = user;
        }

        public async Task<UserLoginResponseDto> GetUserLoginResponse(User user)
        {
            var claims = await UserManager.GetClaimsAsync(user);
            var identityClaims = await GetUserClaims(claims, user);
            var jwt = GenerateJwt(identityClaims);
            var refreshToken = await GenerateRefreshToken(user);

            return GenerateUserLoginResponse(jwt, refreshToken, user);
        }

        private async Task<ClaimsIdentity> GetUserClaims(ICollection<Claim> claims, User user)
        {
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

            return identityClaims;
        }
        private string GenerateJwt(ClaimsIdentity identityClaims)
        {
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

            return tokenHandler.WriteToken(token);
        }
        private async Task<string> GenerateRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                Email = user.Email,
                ExpirationDate = DateTime.UtcNow.AddHours(RefreshTokenSettings.ExpirationHours)
            };

            Context.RefreshTokens.RemoveRange(Context.RefreshTokens.Where(u => u.Email == user.Email));

            await Context.RefreshTokens.AddAsync(refreshToken);
            await Context.SaveChangesAsync();

            return refreshToken.Token.ToString();
        }
        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        private UserLoginResponseDto GenerateUserLoginResponse(string jwt, string refreshToken, User user)
        {
            var response = new UserLoginResponseDto
            {
                Jwt = jwt,
                RefreshToken = refreshToken,
                User = Mapper.Map<UserDto>(user)
            };

            return response;
        }
        public async Task<RefreshToken> CheckRefreshToken(Guid refreshToken)
        {
            var token = await Context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(u => u.Token == refreshToken);

            return token != null && token.ExpirationDate.ToUniversalTime() > DateTime.UtcNow ? token : null;
        }
    }
}
