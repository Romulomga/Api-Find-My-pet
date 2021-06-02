using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMyPet.Configuration
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection Services)
        {
            Services.AddSwaggerGen(C =>
            {
                C.OperationFilter<SwaggerDefaultValues>();

                C.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT desta maneira: Bearer {seu token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                C.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            return Services;
        }

        public static IApplicationBuilder UseSwaggerConfig(this IApplicationBuilder App, IApiVersionDescriptionProvider Provider)
        {
            //app.UseMiddleware<SwaggerAuthorizedMiddleware>();
            App.UseSwagger();
            App.UseSwaggerUI(
                Options =>
                {
                    foreach (var Description in Provider.ApiVersionDescriptions)
                    {
                        Options.SwaggerEndpoint($"/swagger/{Description.GroupName}/swagger.json", Description.GroupName.ToUpperInvariant());
                    }
                });

            return App;
        }
    }

    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider Provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider Provider) => this.Provider = Provider;

        public void Configure(SwaggerGenOptions Options)
        {
            foreach (var Description in Provider.ApiVersionDescriptions)
            {
                Options.SwaggerDoc(Description.GroupName, CreateInfoForApiVersion(Description));
            }
        }

        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription Description)
        {
            var Info = new OpenApiInfo()
            {
                Title = "API - Find My Pets",
                Version = Description.ApiVersion.ToString(),
                Description = "Esta API foi feita como achados e perdidos e animais.",
                Contact = new OpenApiContact() { Name = "Rômulo Monteiro", Email = "rm.abrahao@gmail.com" },
                License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
            };

            if (Description.IsDeprecated)
            {
                Info.Description += " Esta versão está obsoleta!";
            }

            return Info;
        }
    }

    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation Operation, OperationFilterContext Context)
        {
            if (Operation.Parameters == null)
            {
                return;
            }

            foreach (var Parameter in Operation.Parameters)
            {
                var Description = Context.ApiDescription
                    .ParameterDescriptions
                    .First(p => p.Name == Parameter.Name);

                var RouteInfo = Description.RouteInfo;

                Operation.Deprecated = OpenApiOperation.DeprecatedDefault;

                if (Parameter.Description == null)
                {
                    Parameter.Description = Description.ModelMetadata?.Description;
                }

                if (RouteInfo == null)
                {
                    continue;
                }

                if (Parameter.In != ParameterLocation.Path && Parameter.Schema.Default == null)
                {
                    Parameter.Schema.Default = new OpenApiString(RouteInfo.DefaultValue.ToString());
                }

                Parameter.Required |= !RouteInfo.IsOptional;
            }
        }
    }

    public class SwaggerAuthorizedMiddleware
    {
        private readonly RequestDelegate Next;

        public SwaggerAuthorizedMiddleware(RequestDelegate Next)
        {
            this.Next = Next;
        }

        public async Task Invoke(HttpContext Context)
        {
            if (Context.Request.Path.StartsWithSegments("/swagger")
                && !Context.User.Identity.IsAuthenticated)
            {
                Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await Next.Invoke(Context);
        }
    }
}
