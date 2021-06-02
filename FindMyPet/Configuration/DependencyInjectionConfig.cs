using FindMyPet.Business.Interfaces;
using FindMyPet.Business.Notifications;
using FindMyPet.Repository.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMyPet.Configuration
{
    public static class DependencyInjectionConfig {

        public static IServiceCollection ResolveDependencies(this IServiceCollection Services)
        {
            Services.AddScoped<MyDbContext>();

            Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            Services.AddScoped<ITokenUser, TokenUser>();

            Services.AddScoped<INotificator, Notificator>();
            Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return Services;
        }
    }
}
