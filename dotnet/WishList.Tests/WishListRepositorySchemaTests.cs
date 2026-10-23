using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using Vtex.Api.Context;
using Vtex.Api.Services;
using WishList.Data;
using WishList.Services;
using Xunit;

namespace WishList.Tests
{
    public class WishListRepositorySchemaTests : IDisposable
    {
        // Legacy schema from v1.19.1, with public fields
        private const string LegacySchema = "{\"name\":\"wishlist\",\"properties\":{\"email\":{\"type\":\"string\",\"title\":\"Shopper ID\",\"description\":\"\"},\"ListItemsWrapper\":{\"type\":\"array\",\"title\":\"The ListItemsWrapper schema\",\"description\":\"An explanation about the purpose of this instance.\"}},\"v-indexed\":[\"email\"],\"v-default-fields\":[\"email\",\"ListItemsWrapper\"],\"v-cache\":false,\"v-security\":{\"allowGetAll\":false,\"publicRead\":[\"email\",\"ListItemsWrapper\",\"id\"],\"publicWrite\":[\"email\",\"ListItemsWrapper\"],\"publicFilter\":[\"email\",\"ListItemsWrapper\"]},\"v-immediate-indexing\":true}";

        // Shape returned by Master Data after the PII campaign: indented, with only `email` made private
        private const string CampaignSchema = @"{
    ""name"": ""wishlist"",
    ""properties"": {
        ""email"": { ""type"": ""string"", ""title"": ""Shopper ID"", ""description"": """" },
        ""ListItemsWrapper"": { ""type"": ""array"", ""title"": ""The ListItemsWrapper schema"", ""description"": ""An explanation about the purpose of this instance."" }
    },
    ""v-indexed"": [ ""email"" ],
    ""v-default-fields"": [ ""email"", ""ListItemsWrapper"" ],
    ""v-cache"": false,
    ""v-security"": {
        ""allowGetAll"": false,
        ""publicRead"": [ ""ListItemsWrapper"", ""id"" ],
        ""publicWrite"": [ ""ListItemsWrapper"" ],
        ""publicFilter"": [ ""ListItemsWrapper"" ]
    },
    ""v-immediate-indexing"": true
}";

        private readonly List<HttpMethod> _calls = new List<HttpMethod>();
        private readonly List<string> _putBodies = new List<string>();

        public WishListRepositorySchemaTests()
        {
            WishListRepository.ResetSchemaCache();
            WishListRepository.SchemaRetryDelays = new[] { TimeSpan.Zero, TimeSpan.Zero };
        }

        public void Dispose()
        {
            WishListRepository.ResetSchemaCache();
        }

        private WishListRepository CreateRepository(Func<HttpMethod, int, HttpResponseMessage> responder, string account = "testaccount")
        {
            var handler = new TestHttpMessageHandler(req =>
            {
                _calls.Add(req.Method);
                if (req.Method == HttpMethod.Put)
                {
                    _putBodies.Add(req.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                }

                return responder(req.Method, _calls.Count(m => m == req.Method));
            });

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME] = account;
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);

            var env = new Mock<IVtexEnvironmentVariableProvider>();
            env.Setup(e => e.ApplicationVendor).Returns("vtex");
            env.Setup(e => e.ApplicationName).Returns("wish-list");

            var logger = new Mock<IIOLogger>(MockBehavior.Loose);
            var vtex = new Mock<IIOContext>();
            vtex.SetupGet(v => v.Logger).Returns(logger.Object);
            vtex.SetupGet(v => v.AuthToken).Returns("app-token");

            var io = new Mock<IIOServiceContext>();
            io.SetupGet(c => c.Vtex).Returns(vtex.Object);

            return new WishListRepository(env.Object, accessor.Object, clientFactory.Object, io.Object);
        }

        private static string ReformattedCurrentSchema()
        {
            // Same content as SCHEMA_JSON, different formatting, plus a field Master Data could add
            JObject schema = JObject.Parse(WishListConstants.SCHEMA_JSON);
            schema["v-default-fields"] = new JArray("email", "ListItemsWrapper", "id");
            schema["createdIn"] = "2026-10-23T00:00:00Z";
            return schema.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        [Fact]
        public void IsSchemaUpToDate_ComparesSecurityAndIndexOnly()
        {
            Assert.True(WishListRepository.IsSchemaUpToDate(WishListConstants.SCHEMA_JSON));
            Assert.True(WishListRepository.IsSchemaUpToDate(ReformattedCurrentSchema()));
            Assert.False(WishListRepository.IsSchemaUpToDate(LegacySchema));
            Assert.False(WishListRepository.IsSchemaUpToDate(CampaignSchema));
            Assert.False(WishListRepository.IsSchemaUpToDate("not json"));
            Assert.False(WishListRepository.IsSchemaUpToDate("[]"));
        }

        [Fact]
        public async Task UpToDateSchema_DoesNotPut()
        {
            var repository = CreateRepository((method, _) => TestHttpMessageHandler.Ok(ReformattedCurrentSchema()));

            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get }, _calls);
        }

        [Fact]
        public async Task VerifiedSchema_IsCached()
        {
            var repository = CreateRepository((method, _) => TestHttpMessageHandler.Ok(WishListConstants.SCHEMA_JSON));

            await repository.VerifySchema();
            await repository.VerifySchema();
            await CreateRepository((method, _) => TestHttpMessageHandler.Ok(WishListConstants.SCHEMA_JSON)).VerifySchema();

            Assert.Single(_calls);
        }

        [Fact]
        public async Task LegacySchema_IsReplacedAndConfirmed()
        {
            var repository = CreateRepository((method, count) =>
            {
                if (method == HttpMethod.Put)
                {
                    return TestHttpMessageHandler.Ok();
                }

                return TestHttpMessageHandler.Ok(count == 1 ? LegacySchema : WishListConstants.SCHEMA_JSON);
            });

            await repository.VerifySchema();
            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Get }, _calls);
            Assert.Equal(WishListConstants.SCHEMA_JSON, _putBodies.Single());
        }

        [Fact]
        public async Task CampaignSchema_IsReplacedWithPrivateSchema()
        {
            var repository = CreateRepository((method, count) =>
            {
                if (method == HttpMethod.Put)
                {
                    return TestHttpMessageHandler.Ok();
                }

                return TestHttpMessageHandler.Ok(count == 1 ? CampaignSchema : WishListConstants.SCHEMA_JSON);
            });

            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Get }, _calls);
            Assert.Equal(WishListConstants.SCHEMA_JSON, _putBodies.Single());
        }

        [Fact]
        public async Task MissingSchema_IsCreated()
        {
            var repository = CreateRepository((method, count) =>
            {
                if (method == HttpMethod.Put)
                {
                    return TestHttpMessageHandler.Ok();
                }

                return count == 1
                    ? TestHttpMessageHandler.Status(HttpStatusCode.NotFound)
                    : TestHttpMessageHandler.Ok(WishListConstants.SCHEMA_JSON);
            });

            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Get }, _calls);
        }

        [Theory]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Locked)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task RetryableStatus_IsRetried(HttpStatusCode retryableStatus)
        {
            var repository = CreateRepository((method, count) =>
            {
                if (method == HttpMethod.Put)
                {
                    return count < 3 ? TestHttpMessageHandler.Status(retryableStatus) : TestHttpMessageHandler.Ok();
                }

                return TestHttpMessageHandler.Ok(count == 1 ? LegacySchema : WishListConstants.SCHEMA_JSON);
            });

            await repository.VerifySchema();
            await repository.VerifySchema();

            Assert.Equal(3, _calls.Count(m => m == HttpMethod.Put));
            Assert.Equal(2, _calls.Count(m => m == HttpMethod.Get));
        }

        [Fact]
        public async Task PersistentFailure_IsNotCachedAsVerified_AndIsThrottled()
        {
            var repository = CreateRepository((method, _) => method == HttpMethod.Put
                ? TestHttpMessageHandler.Status(HttpStatusCode.TooManyRequests)
                : TestHttpMessageHandler.Ok(LegacySchema));

            await repository.VerifySchema();
            int callsAfterFirstCheck = _calls.Count;
            await repository.VerifySchema();

            Assert.Equal(3, _calls.Count(m => m == HttpMethod.Put));
            Assert.Equal(callsAfterFirstCheck, _calls.Count);
        }

        [Fact]
        public async Task NonRetryableFailure_IsNotRetried()
        {
            var repository = CreateRepository((method, _) => method == HttpMethod.Put
                ? TestHttpMessageHandler.Status(HttpStatusCode.Forbidden)
                : TestHttpMessageHandler.Ok(LegacySchema));

            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get, HttpMethod.Put }, _calls);
        }

        [Fact]
        public async Task NotModified_IsTreatedAsVerified()
        {
            var repository = CreateRepository((method, _) => method == HttpMethod.Put
                ? TestHttpMessageHandler.Status(HttpStatusCode.NotModified)
                : TestHttpMessageHandler.Ok(LegacySchema));

            await repository.VerifySchema();
            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get, HttpMethod.Put }, _calls);
        }

        [Fact]
        public async Task GetFailure_DoesNotPut()
        {
            var repository = CreateRepository((method, _) => TestHttpMessageHandler.Status(HttpStatusCode.InternalServerError));

            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get }, _calls);
        }

        [Fact]
        public async Task AppliedButNotConfirmed_IsNotCachedAsVerified()
        {
            var repository = CreateRepository((method, _) => method == HttpMethod.Put
                ? TestHttpMessageHandler.Ok()
                : TestHttpMessageHandler.Ok(LegacySchema));

            await repository.VerifySchema();

            Assert.Equal(new[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Get }, _calls);
        }

        [Fact]
        public async Task Cache_IsPerAccount()
        {
            await CreateRepository((method, _) => TestHttpMessageHandler.Ok(WishListConstants.SCHEMA_JSON), "account-a").VerifySchema();
            await CreateRepository((method, _) => TestHttpMessageHandler.Ok(WishListConstants.SCHEMA_JSON), "account-b").VerifySchema();

            Assert.Equal(2, _calls.Count);
        }
    }
}
