using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Vtex.Api.Context;
using Vtex.Api.Services;
using WishList.Data;
using WishList.Models;
using WishList.Services;
using Xunit;

namespace WishList.Tests
{
    public class WishListServiceAuthTests
    {
        private const string Account = "testaccount";
        private const string AdminToken = "admin-token";
        private const string StoreToken = "store-token";
        private const string ShopperEmail = "shopper@test.com";
        private const string ShopperId = "shopper-guid-1";

        private sealed class Scenario
        {
            // token -> validate response
            public Dictionary<string, ValidatedUser> Tokens = new Dictionary<string, ValidatedUser>();
            // logins granted in License Manager; any other login gets "false"
            public HashSet<string> GrantedLogins = new HashSet<string> { "admin-login" };
            public string GrantedBody = "true";
            public HttpRequestMessage LastGrantedRequest;
            public HttpStatusCode GrantedStatus = HttpStatusCode.OK;
        }

        private static WishListService CreateService(Scenario scenario, string adminToken, string storeToken, string headerToken = null)
        {
            var handler = new TestHttpMessageHandler(req =>
            {
                string url = req.RequestUri?.ToString() ?? string.Empty;
                if (url.Contains("credential/validate", StringComparison.Ordinal))
                {
                    string body = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var token = JsonConvert.DeserializeObject<ValidateToken>(body)?.Token;
                    if (token != null && scenario.Tokens.TryGetValue(token, out var user))
                    {
                        return TestHttpMessageHandler.Ok(JsonConvert.SerializeObject(user));
                    }

                    return TestHttpMessageHandler.Ok(JsonConvert.SerializeObject(new ValidatedUser { AuthStatus = "InvalidToken" }));
                }

                if (url.Contains("/logins/", StringComparison.Ordinal) && url.Contains("/granted", StringComparison.Ordinal))
                {
                    scenario.LastGrantedRequest = req;
                    bool isGrantedLogin = scenario.GrantedLogins.Any(login => url.Contains($"/logins/{login}/", StringComparison.Ordinal));
                    return TestHttpMessageHandler.Status(scenario.GrantedStatus, isGrantedLogin ? scenario.GrantedBody : "false");
                }

                return TestHttpMessageHandler.Status(HttpStatusCode.NotFound);
            });

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME] = Account;
            httpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL] = "test-credential";
            if (headerToken != null)
            {
                httpContext.Request.Headers[WishListConstants.VtexIdCookie] = headerToken;
            }

            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);

            var logger = new Mock<IIOLogger>(MockBehavior.Loose);
            var vtex = new Mock<IIOContext>();
            vtex.SetupGet(v => v.Logger).Returns(logger.Object);
            vtex.SetupGet(v => v.AdminUserAuthToken).Returns(adminToken);
            vtex.SetupGet(v => v.StoreUserAuthToken).Returns(storeToken);

            var io = new Mock<IIOServiceContext>();
            io.SetupGet(c => c.Vtex).Returns(vtex.Object);

            return new WishListService(new Mock<IWishListRepository>().Object, accessor.Object, clientFactory.Object, io.Object);
        }

        private static ValidatedUser Valid(string id, string user) =>
            new ValidatedUser { AuthStatus = "Success", Id = id, User = user };

        private static Scenario AdminScenario()
        {
            var s = new Scenario();
            s.Tokens[AdminToken] = Valid("admin-login", "admin@test.com");
            return s;
        }

        private static Scenario ShopperScenario()
        {
            var s = new Scenario();
            s.Tokens[StoreToken] = Valid(ShopperId, ShopperEmail);
            return s;
        }

        [Fact]
        public async Task Admin_WithLicenseManagerGrant_CanAccessAnyShopper()
        {
            var service = CreateService(AdminScenario(), AdminToken, null);

            Assert.Equal(HttpStatusCode.OK, await service.EnsureCanAccessShopper("someone@else.com"));
            Assert.Equal(HttpStatusCode.OK, await service.EnsureAdmin());
        }

        [Fact]
        public async Task GrantedCheck_SendsAppCredentialAsProxyAuthorizationAndUserTokenAsCookie()
        {
            var scenario = AdminScenario();
            var service = CreateService(scenario, AdminToken, null);

            await service.EnsureAdmin();

            HttpRequestMessage granted = scenario.LastGrantedRequest;
            Assert.NotNull(granted);
            Assert.Equal($"http://{Account}.vtexcommercestable.com.br/api/pvt/accounts/{Account}/logins/admin-login/granted", granted.RequestUri.ToString());
            Assert.Equal("test-credential", granted.Headers.GetValues(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME).Single());
            Assert.Equal(AdminToken, granted.Headers.GetValues(WishListConstants.VtexIdCookie).Single());
            Assert.Equal("true", granted.Headers.GetValues(WishListConstants.USE_HTTPS_HEADER_NAME).Single());
            Assert.False(granted.Headers.Contains(WishListConstants.AUTHORIZATION_HEADER_NAME));
        }

        [Fact]
        public async Task Admin_WhenLicenseManagerReturnsFalse_IsDenied()
        {
            var scenario = AdminScenario();
            scenario.GrantedBody = "false";
            var service = CreateService(scenario, AdminToken, null);

            var requester = await service.GetRequesterAsync();

            Assert.False(requester.IsAdmin);
            Assert.NotEqual(HttpStatusCode.OK, await service.EnsureCanAccessShopper("someone@else.com"));
            Assert.NotEqual(HttpStatusCode.OK, await service.EnsureAdmin());
        }

        [Fact]
        public async Task Admin_WhenLicenseManagerFails_IsDenied()
        {
            var scenario = AdminScenario();
            scenario.GrantedStatus = HttpStatusCode.InternalServerError;
            var service = CreateService(scenario, AdminToken, null);

            Assert.False((await service.GetRequesterAsync()).IsAdmin);
        }

        [Fact]
        public async Task Admin_WhenLicenseManagerBodyUnexpected_IsDenied()
        {
            var scenario = AdminScenario();
            scenario.GrantedBody = "<html>nope</html>";
            var service = CreateService(scenario, AdminToken, null);

            Assert.False((await service.GetRequesterAsync()).IsAdmin);
        }

        [Fact]
        public async Task Admin_WithTokenInVtexIdclientAutCookieHeader_IsAdmin()
        {
            // REST integrations (e.g. /_v/wishlist/export-lists) send the token in the header
            var service = CreateService(AdminScenario(), null, null, AdminToken);

            Assert.True((await service.GetRequesterAsync()).IsAdmin);
            Assert.Equal(HttpStatusCode.OK, await service.EnsureAdmin());
        }

        [Fact]
        public async Task ShopperTokenInVtexIdclientAutCookieHeader_IsNotAdminAndCannotAccessOthers()
        {
            var service = CreateService(ShopperScenario(), null, null, StoreToken);

            Assert.False((await service.GetRequesterAsync()).IsAdmin);
            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureCanAccessShopper("victim@test.com"));
            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureAdmin());
        }

        [Fact]
        public async Task Shopper_WhenValidateReturnsNoIdentity_IsDenied()
        {
            var scenario = new Scenario();
            scenario.Tokens[StoreToken] = new ValidatedUser { AuthStatus = "Success" };
            var service = CreateService(scenario, null, StoreToken);

            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureCanAccessShopper(ShopperEmail));
        }

        [Fact]
        public async Task Shopper_CanAccessOwnListByEmail()
        {
            var service = CreateService(ShopperScenario(), null, StoreToken);

            Assert.Equal(HttpStatusCode.OK, await service.EnsureCanAccessShopper(ShopperEmail));
            Assert.Equal(HttpStatusCode.OK, await service.EnsureCanAccessShopper("  SHOPPER@test.com "));
        }

        [Fact]
        public async Task Shopper_CanAccessOwnListByUserId()
        {
            var service = CreateService(ShopperScenario(), null, StoreToken);

            Assert.Equal(HttpStatusCode.OK, await service.EnsureCanAccessShopper(ShopperId));
        }

        [Fact]
        public async Task Shopper_CannotAccessAnotherShopper()
        {
            var service = CreateService(ShopperScenario(), null, StoreToken);

            Assert.Equal(HttpStatusCode.Forbidden, await service.EnsureCanAccessShopper("victim@test.com"));
            Assert.Equal(HttpStatusCode.Forbidden, await service.EnsureCanAccessShopper("other-guid"));
            Assert.Equal(HttpStatusCode.Forbidden, await service.EnsureCanAccessShopper(""));
        }

        [Fact]
        public async Task Shopper_IsNeverAdmin_AndCannotReachAdminOperations()
        {
            var service = CreateService(ShopperScenario(), null, StoreToken);

            Assert.Equal(HttpStatusCode.Forbidden, await service.EnsureAdmin());
        }

        [Fact]
        public async Task NoTokens_ReturnsUnauthorized()
        {
            var service = CreateService(new Scenario(), null, null);

            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureCanAccessShopper(ShopperEmail));
            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureAdmin());
        }

        [Fact]
        public async Task InvalidTokens_ReturnUnauthorized()
        {
            var service = CreateService(new Scenario(), "bad-admin", "bad-store");

            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureCanAccessShopper(ShopperEmail));
        }

        [Fact]
        public async Task ShopperTokenInAdminSlot_NotGrantedInLicenseManager_IsNotAdminAndCannotAccessOthers()
        {
            // e.g. shopper token sent through the generic VtexIdclientAutCookie populates AdminUserAuthToken
            var scenario = new Scenario();
            scenario.Tokens["shopper-cookie"] = Valid(ShopperId, ShopperEmail);
            var service = CreateService(scenario, "shopper-cookie", null);

            var requester = await service.GetRequesterAsync();

            Assert.False(requester.IsAdmin);
            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureCanAccessShopper("victim@test.com"));
            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureCanAccessShopper(ShopperEmail));
            Assert.Equal(HttpStatusCode.Unauthorized, await service.EnsureAdmin());
        }
    }
}
