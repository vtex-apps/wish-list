using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Vtex;
using WishList.Data;
using Xunit;

namespace WishList.Tests
{
    public class StartupExtenderTests
    {
        [Fact]
        public void AuthHttpClient_HasShortTimeout()
        {
            var services = new ServiceCollection();
            new StartupExtender().ExtendConfigureServices(services);
            IHttpClientFactory factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            Assert.Equal(TimeSpan.FromSeconds(6), factory.CreateClient(WishListConstants.AUTH_HTTP_CLIENT).Timeout);
            // Below the 60 s route timeout in service.json
            Assert.True(WishListConstants.AuthHttpClientTimeout < TimeSpan.FromSeconds(60));
        }
    }
}
