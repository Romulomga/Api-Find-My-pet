using FindMyPet.Business.Interfaces;
using FindMyPet.Business.Models;
using FindMyPet.Interfaces;
using FindMyPet.Models;
using FindMyPet.Repository.Context;
using FindMyPet.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FindMyPet.Configuration
{
    public static class DependencyInjectionConfig {

        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            services.AddScoped<MyDbContext>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IJwtUser, JwtUser>();
            services.AddScoped<AuthenticationService>();

            services.AddScoped<INotificator, Notificator>();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;
        }
    }
}
