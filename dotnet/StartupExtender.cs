using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WishList.Data;
using WishList.Services;

namespace Vtex
{
    public class StartupExtender
    {
        public void ExtendConstructor(IConfiguration config, IWebHostEnvironment env)
        {

        }

        public void ExtendConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IVtexEnvironmentVariableProvider, VtexEnvironmentVariableProvider>();
            services.AddTransient<IWishListRepository, WishListRepository>();
            services.AddTransient<IWishListService, WishListService>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
        }

        // This method is called inside Startup.Configure() before calling app.UseRouting()
        public void ExtendConfigureBeforeRouting(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        // This method is called inside Startup.Configure() before calling app.UseEndpoint()
        public void ExtendConfigureBeforeEndpoint(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        // This method is called inside Startup.Configure() after calling app.UseEndpoint()
        public void ExtendConfigureAfterEndpoint(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}