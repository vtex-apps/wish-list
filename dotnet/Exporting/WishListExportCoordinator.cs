using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WishList.Models;

namespace WishList.Exporting
{
    public sealed class WishListExportCoordinator : IWishListExportCoordinator
    {
        private readonly IWishListExportCollector _collector;
        private readonly IExportConcurrencyGate _gate;
        private readonly IExportTelemetry _telemetry;

        public WishListExportCoordinator(
            IWishListExportCollector collector,
            IExportConcurrencyGate gate,
            IExportTelemetry telemetry)
        {
            _collector = collector ?? throw new ArgumentNullException(nameof(collector));
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        public Task<WishListsWrapper> ExportAllAsync(
            Func<CancellationToken, Task> prepare,
            CancellationToken cancellationToken)
        {
            return ExecuteAsync(
                "full",
                null,
                prepare,
                cancellationToken,
                progress => _collector.ExportAllAsync(progress, cancellationToken));
        }

        public Task<WishListsWrapper> ExportPageAsync(
            int pageList,
            Func<CancellationToken, Task> prepare,
            CancellationToken cancellationToken)
        {
            return ExecuteAsync(
                "paged",
                pageList,
                prepare,
                cancellationToken,
                progress => _collector.ExportPageAsync(pageList, progress, cancellationToken));
        }

        private void TryRecord(ExportObservation observation)
        {
            try
            {
                _telemetry.Record(observation);
            }
            catch
            {
                // Observability must never change the outcome of an export.
            }
        }

        private async Task<WishListsWrapper> ExecuteAsync(
            string operation,
            int? pageList,
            Func<CancellationToken, Task> prepare,
            CancellationToken cancellationToken,
            Func<ExportProgress, Task<ExportResult>> action)
        {
            if (!_gate.TryAcquire(out var lease))
            {
                TryRecord(ExportObservation.Busy(operation, pageList));
                throw new ExportBusyException();
            }

            var stopwatch = Stopwatch.StartNew();
            ExportResult exportResult = null;
            var progress = new ExportProgress();
            var resultCode = ExportResultCode.Success;

            using (lease)
            {
                try
                {
                    if (prepare != null)
                    {
                        await prepare(cancellationToken);
                    }

                    exportResult = await action(progress);
                    return exportResult.Value;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Only a cancelled request token is a cancellation; HttpClient timeouts also
                    // surface as OperationCanceledException/TaskCanceledException and fall through
                    // to the generic failure below.
                    resultCode = ExportResultCode.Cancelled;
                    throw;
                }
                catch (WishListExportException exception)
                {
                    resultCode = exception.ResultCode;
                    throw;
                }
                catch
                {
                    resultCode = ExportResultCode.MasterDataError;
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                    TryRecord(ExportObservation.From(
                        operation,
                        resultCode,
                        pageList,
                        exportResult,
                        progress,
                        stopwatch.Elapsed.TotalSeconds));
                }
            }
        }
    }
}
