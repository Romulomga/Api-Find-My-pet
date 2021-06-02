using FindMyPet.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FindMyPet.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfig(this IServiceCollection Services)
        {
            Services.AddControllers();

            Services.AddApiVersioning(Options =>
            {
                Options.AssumeDefaultVersionWhenUnspecified = true;
                Options.DefaultApiVersion = new ApiVersion(1, 0);
                Options.ReportApiVersions = true;
            });

            Services.AddVersionedApiExplorer(Options =>
            {
                Options.GroupNameFormat = "'v'VVV";
                Options.SubstituteApiVersionInUrl = true;
            });

            Services.Configure<ApiBehaviorOptions>(Options =>
            {
                Options.SuppressModelStateInvalidFilter = true;
            });

            Services.AddCors(Options =>
            {
                Options.AddPolicy("Development", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
                Options.AddPolicy("Production", builder => builder.WithMethods("GET").WithOrigins("http://desenvolvedor.io").SetIsOriginAllowedToAllowWildcardSubdomains().AllowAnyHeader());
            });

            return Services;
        }

        public static IApplicationBuilder UseApiConfig(this IApplicationBuilder App, IWebHostEnvironment Env)
        {
            if (Env.IsDevelopment())
            {
                App.UseCors("Development");
                App.UseDeveloperExceptionPage();
            }
            else
            {
                App.UseCors("Development"); // Usar apenas nas demos => Configuração Ideal: Production
                App.UseHsts();
            }

            App.UseMiddleware<ExceptionMiddleware>();
            App.UseHttpsRedirection();
            App.UseRouting();
            App.UseAuthentication();
            App.UseAuthorization();

            App.UseStaticFiles();

            App.UseEndpoints(Endpoints =>
            {
                Endpoints.MapControllers();
            });

            return App;
        }
    }
}
