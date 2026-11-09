using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WishList.Exporting;
using WishList.Models;
using WishList.Tests.TestDoubles;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class WishListExportCoordinatorTests
    {
        private sealed class TrackingLease : IDisposable
        {
            public int Disposed { get; private set; }
            public void Dispose() { Disposed++; }
        }

        private readonly Mock<IWishListExportCollector> _collector = new Mock<IWishListExportCollector>();
        private readonly Mock<IExportConcurrencyGate> _gate = new Mock<IExportConcurrencyGate>();
        private readonly Mock<IExportTelemetry> _telemetry = new Mock<IExportTelemetry>();
        private readonly List<ExportObservation> _observations = new List<ExportObservation>();
        private readonly TrackingLease _lease = new TrackingLease();

        public WishListExportCoordinatorTests()
        {
            _telemetry.Setup(value => value.Record(It.IsAny<ExportObservation>()))
                .Callback<ExportObservation>(o => _observations.Add(o));
        }

        private WishListExportCoordinator Create(bool acquire)
        {
            IDisposable lease = acquire ? _lease : null;
            _gate.Setup(value => value.TryAcquire(out lease)).Returns(acquire);
            return new WishListExportCoordinator(_collector.Object, _gate.Object, _telemetry.Object);
        }

        private static ExportResult SampleResult()
        {
            var session = new ScrollSession("t");
            session.Accept(new ScrollPage(
                new List<WishListWrapper> { new WishListWrapper { Id = "1" }, new WishListWrapper { Id = "2" } },
                500, true, "sig"));
            return new ExportResult(
                new WishListsWrapper { WishLists = new List<WishListWrapper> { new WishListWrapper { Id = "1" } } },
                session);
        }

        [Fact]
        public async Task BusyGateRecordsRejectionWithoutCallingCollector()
        {
            var coordinator = Create(acquire: false);

            await Assert.ThrowsAsync<ExportBusyException>(
                () => coordinator.ExportAllAsync(null, CancellationToken.None));

            _collector.Verify(
                value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _telemetry.Verify(value => value.Record(It.Is<ExportObservation>(
                observation => observation.Operation == "full"
                    && observation.Result == ExportResultCode.Busy)), Times.Once);
        }

        [Fact]
        public async Task SuccessReturnsCollectorValueAndRecordsCounters()
        {
            var expected = SampleResult();
            _collector.Setup(value => value.ExportPageAsync(3, It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
            var coordinator = Create(acquire: true);

            var value2 = await coordinator.ExportPageAsync(3, null, CancellationToken.None);

            Assert.Same(expected.Value, value2);
            var observation = Assert.Single(_observations);
            Assert.Equal("paged", observation.Operation);
            Assert.Equal(ExportResultCode.Success, observation.Result);
            Assert.Equal(3, observation.PageList);
            Assert.Equal(1, observation.PagesRead);
            Assert.Equal(2, observation.RecordsRead);
            Assert.Equal(1, observation.RecordsReturned);
            Assert.Equal(500, observation.BytesRead);
            Assert.Equal(1, _lease.Disposed);
        }

        [Fact]
        public async Task CancellationRecordsCancelledAndRethrows()
        {
            _collector.Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
            var coordinator = Create(acquire: true);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => coordinator.ExportAllAsync(null, cts.Token));

            Assert.Equal(ExportResultCode.Cancelled, Assert.Single(_observations).Result);
            Assert.Equal(1, _lease.Disposed);
        }

        public static IEnumerable<object[]> TimeoutExceptions()
        {
            yield return new object[] { new OperationCanceledException("timeout") };
            yield return new object[] { new TaskCanceledException("HttpClient timeout") };
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task CancellationExceptionWithoutCancelledTokenIsMasterDataError(OperationCanceledException timeout)
        {
            _collector.Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(timeout);
            var coordinator = Create(acquire: true);

            var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => coordinator.ExportAllAsync(null, CancellationToken.None));

            Assert.Same(timeout, thrown);
            Assert.Equal(ExportResultCode.MasterDataError, Assert.Single(_observations).Result);
            Assert.Equal(1, _lease.Disposed);
        }

        [Fact]
        public async Task StableFailureRecordsItsResultCode()
        {
            _collector.Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ScrollNoProgressException());
            var coordinator = Create(acquire: true);

            await Assert.ThrowsAsync<ScrollNoProgressException>(
                () => coordinator.ExportAllAsync(null, CancellationToken.None));

            Assert.Equal(ExportResultCode.NoProgress, Assert.Single(_observations).Result);
            Assert.Equal(1, _lease.Disposed);
        }

        [Fact]
        public async Task UnexpectedFailureMapsToMasterDataError()
        {
            _collector.Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));
            var coordinator = Create(acquire: true);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => coordinator.ExportAllAsync(null, CancellationToken.None));

            Assert.Equal(ExportResultCode.MasterDataError, Assert.Single(_observations).Result);
            Assert.Equal(1, _lease.Disposed);
        }
    
        [Fact]
        public async Task TelemetryFailureDoesNotFailASuccessfulExport()
        {
            var expected = SampleResult();
            _collector.Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
            _telemetry.Setup(value => value.Record(It.IsAny<ExportObservation>()))
                .Throws(new InvalidOperationException("logger down"));
            var coordinator = Create(acquire: true);

            var value2 = await coordinator.ExportAllAsync(null, CancellationToken.None);

            Assert.Same(expected.Value, value2);
            Assert.Equal(1, _lease.Disposed);
        }

        [Fact]
        public async Task TelemetryFailureDoesNotMaskOriginalException()
        {
            _collector.Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ScrollNoProgressException());
            _telemetry.Setup(value => value.Record(It.IsAny<ExportObservation>()))
                .Throws(new InvalidOperationException("logger down"));
            var coordinator = Create(acquire: true);

            await Assert.ThrowsAsync<ScrollNoProgressException>(
                () => coordinator.ExportAllAsync(null, CancellationToken.None));
        }
    
        // ---- partial progress is preserved on failure ----

        private WishListExportCoordinator CreateWithRealCollector(IMasterDataScrollReader reader)
        {
            return new WishListExportCoordinator(
                new WishListExportCollector(reader),
                new ExportConcurrencyGate(),
                _telemetry.Object);
        }

        private void AssertPartial(string result, long pages, long records, long bytes)
        {
            var observation = Assert.Single(_observations);
            Assert.Equal(result, observation.Result);
            Assert.Equal(pages, observation.PagesRead);
            Assert.Equal(records, observation.RecordsRead);
            Assert.Equal(bytes, observation.BytesRead);
            Assert.Equal(0, observation.RecordsReturned);
        }

        [Fact]
        public async Task CancellationKeepsProgressAccumulatedBeforeIt()
        {
            using var cts = new CancellationTokenSource();
            var reader = new ScriptedScrollReader(
                ScriptedScrollReader.FullPage("p0"),
                () => ScriptedScrollReader.FullPage("p1", 200),
                () =>
                {
                    cts.Cancel();
                    throw new OperationCanceledException(cts.Token);
                });

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => CreateWithRealCollector(reader).ExportAllAsync(null, cts.Token));

            AssertPartial(ExportResultCode.Cancelled, pages: 2, records: 400, bytes: 2000);
        }

        [Fact]
        public async Task NoProgressCountsTheRepeatedBatchThatCausedIt()
        {
            var reader = new ScriptedScrollReader(
                ScriptedScrollReader.FullPage("same"),
                () => ScriptedScrollReader.FullPage("same"));

            await Assert.ThrowsAsync<ScrollNoProgressException>(
                () => CreateWithRealCollector(reader).ExportAllAsync(null, CancellationToken.None));

            AssertPartial(ExportResultCode.NoProgress, pages: 2, records: 400, bytes: 2000);
        }

        [Fact]
        public async Task MasterDataErrorKeepsProgressAccumulatedBeforeIt()
        {
            var reader = new ScriptedScrollReader(
                ScriptedScrollReader.FullPage("p0"),
                () => throw new WishListExportException(ExportResultCode.MasterDataError, "502"));

            await Assert.ThrowsAsync<WishListExportException>(
                () => CreateWithRealCollector(reader).ExportPageAsync(2, null, CancellationToken.None));

            AssertPartial(ExportResultCode.MasterDataError, pages: 1, records: 200, bytes: 1000);
        }

        [Fact]
        public async Task HttpErrorAddsItsBytesButNoPageToPreviousProgress()
        {
            var items = new List<string>();
            for (var i = 0; i < 200; i++)
            {
                items.Add("{\"id\":\"" + i + "\"}");
            }

            var firstBody = "[" + string.Join(",", items) + "]";
            var handler = new SequenceHttpMessageHandler(
                MdResponse(firstBody, "token-a"),
                new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadGateway)
                {
                    Content = new System.Net.Http.StringContent("upstream failure")
                });
            var client = new System.Net.Http.HttpClient(handler);
            var factory = new Mock<System.Net.Http.IHttpClientFactory>();
            factory.Setup(value => value.CreateClient(It.IsAny<string>())).Returns(client);
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            httpContext.Request.Headers["X-Vtex-Account"] = "store";
            var reader = new MasterDataScrollReader(
                factory.Object,
                new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = httpContext });

            await Assert.ThrowsAsync<WishListExportException>(
                () => CreateWithRealCollector(reader).ExportAllAsync(null, CancellationToken.None));

            AssertPartial(
                ExportResultCode.MasterDataError,
                pages: 1,
                records: 200,
                bytes: System.Text.Encoding.UTF8.GetByteCount(firstBody) + 16);
        }

        [Fact]
        public async Task InvalidJsonCountsTheInvalidPageOnTopOfPreviousProgress()
        {
            var items = new List<string>();
            for (var i = 0; i < 200; i++)
            {
                items.Add("{\"id\":\"" + i + "\"}");
            }

            var firstBody = "[" + string.Join(",", items) + "]";
            var handler = new SequenceHttpMessageHandler(
                MdResponse(firstBody, "token-a"),
                MdResponse("{"));
            var client = new System.Net.Http.HttpClient(handler);
            var factory = new Mock<System.Net.Http.IHttpClientFactory>();
            factory.Setup(value => value.CreateClient(It.IsAny<string>())).Returns(client);
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            httpContext.Request.Headers["X-Vtex-Account"] = "store";
            var reader = new MasterDataScrollReader(
                factory.Object,
                new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = httpContext });

            await Assert.ThrowsAsync<WishListExportException>(
                () => CreateWithRealCollector(reader).ExportAllAsync(null, CancellationToken.None));

            AssertPartial(
                ExportResultCode.InvalidResponse,
                pages: 2,
                records: 200,
                bytes: System.Text.Encoding.UTF8.GetByteCount(firstBody) + 1);
        }

        private static System.Net.Http.HttpResponseMessage MdResponse(string json, string token = null)
        {
            var response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
            if (token != null)
            {
                response.Headers.Add("X-VTEX-MD-TOKEN", token);
            }

            return response;
        }

        [Fact]
        public async Task FailureBeforeAnyResponseRecordsZeroProgress()
        {
            var reader = new ScriptedScrollReader(null);

            await Assert.ThrowsAsync<WishListExportException>(
                () => CreateWithRealCollector(reader).ExportAllAsync(null, CancellationToken.None));

            AssertPartial(ExportResultCode.MasterDataError, pages: 0, records: 0, bytes: 0);
        }

        [Fact]
        public async Task InvalidFirstPageCountsItsBytesAndPage()
        {
            var handler = new SequenceHttpMessageHandler(MdResponse("{", "token-a"));
            var client = new System.Net.Http.HttpClient(handler);
            var factory = new Mock<System.Net.Http.IHttpClientFactory>();
            factory.Setup(value => value.CreateClient(It.IsAny<string>())).Returns(client);
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            httpContext.Request.Headers["X-Vtex-Account"] = "store";
            var reader = new MasterDataScrollReader(
                factory.Object,
                new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = httpContext });

            await Assert.ThrowsAsync<WishListExportException>(
                () => CreateWithRealCollector(reader).ExportAllAsync(null, CancellationToken.None));

            AssertPartial(ExportResultCode.InvalidResponse, pages: 1, records: 0, bytes: 1);
        }

        [Fact]
        public async Task SuccessfulExportCountsEachPageOnlyOnce()
        {
            var reader = QueueScrollReader.WithNumberedPages(pageCount: 3, pageSize: 200, lastPageSize: 50);

            await CreateWithRealCollector(reader).ExportAllAsync(null, CancellationToken.None);

            var observation = Assert.Single(_observations);
            Assert.Equal(ExportResultCode.Success, observation.Result);
            Assert.Equal(3, observation.PagesRead);
            Assert.Equal(450, observation.RecordsRead);
            Assert.Equal(45000, observation.BytesRead);
        }

        // ---- preflight runs inside the gate ----

        [Fact]
        public async Task PrepareRunsAfterGateAcquiredAndBeforeCollector()
        {
            var order = new List<string>();
            IDisposable lease = _lease;
            _gate.Setup(value => value.TryAcquire(out lease))
                .Callback(new TryAcquireCallback((out IDisposable l) => { order.Add("gate"); l = _lease; }))
                .Returns(true);
            _collector.Setup(value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()))
                .Callback(() => order.Add("collector"))
                .ReturnsAsync(SampleResult());
            var coordinator = new WishListExportCoordinator(_collector.Object, _gate.Object, _telemetry.Object);
            using var cts = new CancellationTokenSource();
            CancellationToken seen = default;

            await coordinator.ExportAllAsync(
                token => { order.Add("prepare"); seen = token; return Task.CompletedTask; },
                cts.Token);

            Assert.Equal(new[] { "gate", "prepare", "collector" }, order);
            Assert.Equal(cts.Token, seen);
        }

        private delegate void TryAcquireCallback(out IDisposable lease);

        [Fact]
        public async Task PrepareIsNotCalledWhenGateIsBusy()
        {
            var coordinator = Create(acquire: false);
            var called = false;

            await Assert.ThrowsAsync<ExportBusyException>(() => coordinator.ExportAllAsync(
                token => { called = true; return Task.CompletedTask; },
                CancellationToken.None));

            Assert.False(called);
            _collector.Verify(
                value => value.ExportAllAsync(It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelledPrepareRecordsCancelledSkipsCollectorAndReleasesLease()
        {
            var coordinator = Create(acquire: true);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => coordinator.ExportPageAsync(
                1,
                token => throw new OperationCanceledException(token),
                cts.Token));

            Assert.Equal(ExportResultCode.Cancelled, Assert.Single(_observations).Result);
            Assert.Equal(1, _lease.Disposed);
            _collector.Verify(
                value => value.ExportPageAsync(It.IsAny<int>(), It.IsAny<ExportProgress>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
