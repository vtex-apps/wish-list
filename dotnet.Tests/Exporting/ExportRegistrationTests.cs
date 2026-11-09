using Microsoft.Extensions.DependencyInjection;
using Moq;
using Vtex;
using Vtex.Api.Context;
using WishList.Exporting;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class ExportRegistrationTests
    {
        [Fact]
        public void CoordinatorResolvesFromScopeWhenServiceContextIsScoped()
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => new Mock<IIOServiceContext> { DefaultValue = DefaultValue.Mock }.Object);
            new StartupExtender().ExtendConfigureServices(services);

            using var provider = services.BuildServiceProvider(
                new ServiceProviderOptions { ValidateScopes = true });
            using var scope = provider.CreateScope();

            var telemetry = scope.ServiceProvider.GetService<IExportTelemetry>();
            Assert.NotNull(telemetry);
        }
    }
}
