using AutoMapper;
using FindMyPet.Configuration;
using FindMyPet.Helpers;
using FindMyPet.Repository.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FindMyPet
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostEnvironment HostEnvironment)
        {
            var Builder = new ConfigurationBuilder()
                .SetBasePath(HostEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{HostEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            if (HostEnvironment.IsProduction())
            {
                Builder.AddUserSecrets<Startup>();
            }

            Configuration = Builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection Services)
        {
            Services.AddDbContext<MyDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            Services.AddIdentityConfig(Configuration);
            Services.AddApiConfig();
            Services.AddSwaggerConfig();
            Services.ResolveDependencies();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutomapperConfig());
            });

            IMapper Mapper = mappingConfig.CreateMapper();
            Services.AddSingleton(Mapper);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder App, IWebHostEnvironment Env, IApiVersionDescriptionProvider Provider)
        {
            App.UseApiConfig(Env);
            App.UseSwaggerConfig(Provider);
        }
    }
}
