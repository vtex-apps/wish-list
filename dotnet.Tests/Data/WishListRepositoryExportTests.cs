using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Vtex.Api.Context;
using WishList.Data;
using WishList.Exporting;
using WishList.Models;
using WishList.Services;
using WishList.Tests.TestDoubles;
using Xunit;

namespace WishList.Tests.Data
{
    public class WishListRepositoryExportTests
    {
        private readonly Mock<IWishListExportCollector> _collector = new Mock<IWishListExportCollector>();
        private readonly ExportConcurrencyGate _gate = new ExportConcurrencyGate();
        private SequenceHttpMessageHandler _handler;

        private WishListRepository Create(params HttpResponseMessage[] responses)
        {
            _handler = new SequenceHttpMessageHandler(responses);
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(value => value.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_handler));

            var httpContext = new DefaultHttpContext();
            // Unique account: schema verification results are cached per account in a static map.
            httpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME] = "acc" + Guid.NewGuid().ToString("N");

            var environment = new Mock<IVtexEnvironmentVariableProvider>();
            var context = new Mock<IIOServiceContext> { DefaultValue = DefaultValue.Mock };
            var coordinator = new WishListExportCoordinator(
                _collector.Object,
                _gate,
                Mock.Of<IExportTelemetry>());

            return new WishListRepository(
                environment.Object,
                new HttpContextAccessor { HttpContext = httpContext },
                factory.Object,
                context.Object,
                coordinator);
        }

        [Fact]
        public async Task BusyFullExportIsRejectedBeforeSchemaOrMasterDataCalls()
        {
            var repository = Create();
            Assert.True(_gate.TryAcquire(out var held));

            using (held)
            {
                await Assert.ThrowsAsync<ExportBusyException>(
                    () => repository.GetAllLists(CancellationToken.None));
            }

            Assert.Empty(_handler.RequestedUris);
            _collector.Verify(
                value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task BusyPagedExportIsRejectedBeforeSchemaOrMasterDataCalls()
        {
            var repository = Create();
            Assert.True(_gate.TryAcquire(out var held));

            using (held)
            {
                await Assert.ThrowsAsync<ExportBusyException>(
                    () => repository.GetAllListsPaged(1, CancellationToken.None));
            }

            Assert.Empty(_handler.RequestedUris);
            _collector.Verify(
                value => value.ExportPageAsync(It.IsAny<int>(), It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SchemaIsVerifiedInsideTheGateBeforeTheCollectorRuns()
        {
            var order = new System.Collections.Generic.List<string>();
            var repository = Create(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("down")
            });
            _collector
                .Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    order.Add("collector:" + _handler.RequestedUris.Count);
                    order.Add("gate-held:" + !_gate.TryAcquire(out _));
                })
                .ReturnsAsync(new ExportResult(
                    new WishListsWrapper { WishLists = new System.Collections.Generic.List<WishListWrapper>() },
                    new ScrollSession("t")));

            await repository.GetAllLists(CancellationToken.None);

            Assert.Equal(new[] { "collector:1", "gate-held:True" }, order);
            Assert.Contains("/schemas/", _handler.RequestedUris[0]);
        }

        [Fact]
        public async Task CancellationReachesSchemaVerificationAndStopsTheExport()
        {
            var repository = Create(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => repository.GetAllLists(cts.Token));

            _collector.Verify(
                value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task VerifySchemaWithoutTokenStillSwallowsUpstreamFailures()
        {
            var repository = Create(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("down")
            });

            await repository.VerifySchema();

            Assert.Single(_handler.RequestedUris);
        }
    }
}
