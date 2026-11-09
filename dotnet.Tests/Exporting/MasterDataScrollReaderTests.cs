using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using WishList.Data;
using WishList.Exporting;
using WishList.Tests.TestDoubles;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class MasterDataScrollReaderTests
    {
        private SequenceHttpMessageHandler _handler;

        private MasterDataScrollReader CreateReader(params HttpResponseMessage[] responses)
        {
            _handler = new SequenceHttpMessageHandler(responses);
            var client = new HttpClient(_handler);
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(value => value.CreateClient(It.IsAny<string>())).Returns(client);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME] = "store";
            httpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL] = "credential";

            return new MasterDataScrollReader(
                factory.Object,
                new HttpContextAccessor { HttpContext = httpContext });
        }

        private static HttpResponseMessage Response(string json, string token = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            if (token != null)
            {
                response.Headers.Add("X-VTEX-MD-TOKEN", token);
            }

            return response;
        }

        [Fact]
        public async Task InterleavedSessionsUseTheirOwnTokens()
        {
            var reader = CreateReader(
                Response("[{\"id\":\"a\"}]", "token-a"),
                Response("[{\"id\":\"b\"}]", "token-b"),
                Response("[]"),
                Response("[]"));

            var first = await reader.StartAsync(CancellationToken.None);
            var second = await reader.StartAsync(CancellationToken.None);
            await reader.ReadNextAsync(first.Session, CancellationToken.None);
            await reader.ReadNextAsync(second.Session, CancellationToken.None);

            Assert.EndsWith("_token=token-a", _handler.RequestedUris[2]);
            Assert.EndsWith("_token=token-b", _handler.RequestedUris[3]);
        }

        [Fact]
        public async Task StartRejectsMissingTokenCountingTheReceivedPage()
        {
            var reader = CreateReader(Response("[{\"id\":\"a\"}]"));

            var error = await Assert.ThrowsAsync<WishListExportException>(
                () => reader.StartAsync(CancellationToken.None));

            Assert.Equal(ExportResultCode.InvalidResponse, error.ResultCode);
            Assert.Equal(1, error.ReceivedPages);
            Assert.Equal(12, error.ReceivedBytes);
        }

        [Fact]
        public async Task StartRejectsEmptyTokenCountingTheReceivedPage()
        {
            var reader = CreateReader(Response("[{\"id\":\"a\"}]", ""));

            var error = await Assert.ThrowsAsync<WishListExportException>(
                () => reader.StartAsync(CancellationToken.None));

            Assert.Equal(ExportResultCode.InvalidResponse, error.ResultCode);
            Assert.Equal(1, error.ReceivedPages);
            Assert.Equal(12, error.ReceivedBytes);
        }

        [Fact]
        public async Task ReaderPropagatesCancellation()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var reader = CreateReader(Response("[]", "token-a"));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => reader.StartAsync(cancellation.Token));
        }

        [Fact]
        public async Task StartRejectsMalformedJson()
        {
            var reader = CreateReader(Response("{", "token-a"));
            var error = await Assert.ThrowsAsync<WishListExportException>(
                () => reader.StartAsync(CancellationToken.None));
            Assert.Equal(ExportResultCode.InvalidResponse, error.ResultCode);
            Assert.Equal(1, error.ReceivedPages);
            Assert.Equal(1, error.ReceivedBytes);
        }

        [Fact]
        public async Task StartRejectsNullBodyCountingTheReceivedPage()
        {
            var reader = CreateReader(Response("null", "token-a"));
            var error = await Assert.ThrowsAsync<WishListExportException>(
                () => reader.StartAsync(CancellationToken.None));
            Assert.Equal(ExportResultCode.InvalidResponse, error.ResultCode);
            Assert.Equal(1, error.ReceivedPages);
            Assert.Equal(4, error.ReceivedBytes);
        }

        [Fact]
        public async Task ReadNextMalformedJsonReportsTheReceivedBytes()
        {
            var reader = CreateReader(Response("[]", "token-a"), Response("{\"x\":"));
            var started = await reader.StartAsync(CancellationToken.None);
            var error = await Assert.ThrowsAsync<WishListExportException>(
                () => reader.ReadNextAsync(started.Session, CancellationToken.None));
            Assert.Equal(1, error.ReceivedPages);
            Assert.Equal(5, error.ReceivedBytes);
        }

        [Fact]
        public async Task StartMapsNonSuccessStatusCountingOnlyTheReceivedBytes()
        {
            var reader = CreateReader(new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = new StringContent("upstream failure")
            });

            var error = await Assert.ThrowsAsync<WishListExportException>(
                () => reader.StartAsync(CancellationToken.None));
            Assert.Equal(ExportResultCode.MasterDataError, error.ResultCode);
            Assert.Equal(0, error.ReceivedPages);
            Assert.Equal(16, error.ReceivedBytes);
        }

        [Fact]
        public async Task ReadNextMapsNonSuccessStatusCountingOnlyTheReceivedBytes()
        {
            var reader = CreateReader(
                Response("[]", "token-a"),
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("server error")
                });
            var started = await reader.StartAsync(CancellationToken.None);

            var error = await Assert.ThrowsAsync<WishListExportException>(
                () => reader.ReadNextAsync(started.Session, CancellationToken.None));
            Assert.Equal(ExportResultCode.MasterDataError, error.ResultCode);
            Assert.Equal(0, error.ReceivedPages);
            Assert.Equal(12, error.ReceivedBytes);
        }

        [Fact]
        public async Task PartialBatchIsTerminal()
        {
            var reader = CreateReader(Response("[{\"id\":\"a\"}]", "token-a"));
            var started = await reader.StartAsync(CancellationToken.None);
            Assert.True(started.FirstPage.IsLast);
        }
    
        [Fact]
        public async Task ScrollTokenIsTreatedAsOpaqueAndEscaped()
        {
            const string token = "a+b&c#d/e=f";
            var reader = CreateReader(
                Response("[]", token),
                Response("[]"));

            var started = await reader.StartAsync(CancellationToken.None);
            await reader.ReadNextAsync(started.Session, CancellationToken.None);

            Assert.EndsWith("_token=" + Uri.EscapeDataString(token), _handler.RequestedUris[1]);
            Assert.DoesNotContain("#", _handler.RequestedUris[1]);
        }
    
        private static string FullBatch(int firstId)
        {
            var items = new System.Collections.Generic.List<string>();
            for (var i = 0; i < 200; i++)
            {
                items.Add("{\"id\":\"" + (firstId + i) + "\"}");
            }

            return "[" + string.Join(",", items) + "]";
        }

        [Fact]
        public async Task RepeatedFullBatchIsDetectedAsNoProgress()
        {
            var reader = CreateReader(
                Response(FullBatch(0), "token-a"),
                Response(FullBatch(0)));

            var started = await reader.StartAsync(CancellationToken.None);
            Assert.False(started.FirstPage.IsLast);
            started.Session.Accept(started.FirstPage);
            var repeated = await reader.ReadNextAsync(started.Session, CancellationToken.None);

            var error = Assert.Throws<ScrollNoProgressException>(() => started.Session.Accept(repeated));
            Assert.Equal(ExportResultCode.NoProgress, error.ResultCode);
        }

        [Fact]
        public async Task DistinctFullBatchesAreAccepted()
        {
            var reader = CreateReader(
                Response(FullBatch(0), "token-a"),
                Response(FullBatch(200)));

            var started = await reader.StartAsync(CancellationToken.None);
            started.Session.Accept(started.FirstPage);
            var next = await reader.ReadNextAsync(started.Session, CancellationToken.None);

            started.Session.Accept(next);
            Assert.Equal(2, started.Session.PagesRead);
        }
    }
}
