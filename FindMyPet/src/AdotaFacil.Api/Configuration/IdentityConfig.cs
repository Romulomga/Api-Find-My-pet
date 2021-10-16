using AdotaFacil.Api.Extensions;
using AdotaFacil.Api.Settings;
using AdotaFacil.Business.Models;
using AdotaFacil.Repository.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace AdotaFacil.Api.Configuration
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = string.Empty;

                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;

                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;

            }).AddEntityFrameworkStores<MyDbContext>()
            .AddErrorDescriber<IdentityMessagesPortuguese>()
            .AddDefaultTokenProviders();

            // JWT
            var jwtSettingsSection = configuration.GetSection(nameof(JwtSettings));

            // Refresh Token
            var refreshTokenSettingsSection = configuration.GetSection(nameof(RefreshTokenSettings));

            // Facebook
            var facebookAuthSettingsSection = configuration.GetSection(nameof(FacebookAuthSettings));

            services.Configure<JwtSettings>(jwtSettingsSection);
            services.Configure<RefreshTokenSettings>(refreshTokenSettingsSection);
            services.Configure<FacebookAuthSettings>(facebookAuthSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
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

            return services;
        }
    }
}
