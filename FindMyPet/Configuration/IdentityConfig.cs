using FindMyPet.Data;
using FindMyPet.Extensions;
using FindMyPet.Helpers;
using FindMyPet.Business.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FindMyPet.Configuration
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfig(this IServiceCollection Services, IConfiguration Configuration)
        {
            Services.AddDbContext<IdentityDbContext>(Options => Options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            Services.AddIdentity<User, IdentityRole<long>>(Options =>
            {
                Options.User.RequireUniqueEmail = true;
                Options.User.AllowedUserNameCharacters = string.Empty;

                Options.Password.RequireDigit = false;
                Options.Password.RequireNonAlphanumeric = false;
                Options.Password.RequireLowercase = false;
                Options.Password.RequireUppercase = false;
                Options.Password.RequiredLength = 4;

                Options.Lockout.MaxFailedAccessAttempts = 3;
                Options.Lockout.AllowedForNewUsers = true;

            }).AddEntityFrameworkStores<IdentityDbContext>()
            .AddErrorDescriber<IdentityMessagesPortuguese>()
            .AddDefaultTokenProviders();

            // JWT
            var JwtSettingsSection = Configuration.GetSection(nameof(JwtSettings));
            var FacebookAuthSettingsSection = Configuration.GetSection(nameof(FacebookAuthSettings));

            Services.Configure<JwtSettings>(JwtSettingsSection);
            Services.Configure<FacebookAuthSettings>(FacebookAuthSettingsSection);

            var jwtSettings = JwtSettingsSection.Get<JwtSettings>();

            Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(Options =>
            {
                Options.RequireHttpsMetadata = true;
                Options.SaveToken = true;
                Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                };
            });

            return Services;
        }
    }
}
