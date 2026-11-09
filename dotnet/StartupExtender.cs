using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vtex.Api.Context;
using WishList.Data;
using WishList.Exporting;
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
            services.AddTransient<IMasterDataScrollReader, MasterDataScrollReader>();
            services.AddTransient<IWishListExportCollector, WishListExportCollector>();
            services.AddSingleton<IExportConcurrencyGate, ExportConcurrencyGate>();
            services.AddTransient<IExportTelemetry>(provider =>
                new PrometheusExportTelemetry(
                    provider.GetRequiredService<IIOServiceContext>().Vtex.Logger));
            services.AddTransient<IWishListExportCoordinator, WishListExportCoordinator>();
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