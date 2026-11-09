using System.Globalization;
using Prometheus;
using Vtex.Api.Services;

namespace WishList.Exporting
{
    public sealed class PrometheusExportTelemetry : IExportTelemetry
    {
        private static readonly string[] LabelNames = { "operation", "result" };

        private static readonly Counter Operations = Metrics.CreateCounter(
            "wishlist_export_operations_total",
            "Wish list export operations.",
            new CounterConfiguration { LabelNames = LabelNames });

        private static readonly Histogram RecordsRead = Metrics.CreateHistogram(
            "wishlist_export_records_read",
            "Records read from Master Data per export.",
            new HistogramConfiguration
            {
                LabelNames = LabelNames,
                Buckets = new double[] { 200, 1000, 5000, 10000, 50000, 100000, 500000 }
            });

        private static readonly Histogram BytesRead = Metrics.CreateHistogram(
            "wishlist_export_bytes_read",
            "Bytes read from Master Data per export.",
            new HistogramConfiguration
            {
                LabelNames = LabelNames,
                Buckets = new double[]
                {
                    1024 * 1024,
                    10 * 1024 * 1024,
                    50 * 1024 * 1024,
                    100 * 1024 * 1024,
                    250 * 1024 * 1024,
                    500 * 1024 * 1024,
                    1024d * 1024 * 1024
                }
            });

        private static readonly Histogram RecordsReturned = Metrics.CreateHistogram(
            "wishlist_export_records_returned",
            "Records returned per export.",
            new HistogramConfiguration
            {
                LabelNames = LabelNames,
                Buckets = new double[] { 0, 200, 1000, 5000, 10000, 50000, 100000, 500000 }
            });

        private static readonly Histogram PagesRead = Metrics.CreateHistogram(
            "wishlist_export_pages_read",
            "Master Data pages read per export.",
            new HistogramConfiguration
            {
                LabelNames = LabelNames,
                Buckets = new double[] { 1, 5, 25, 50, 250, 500, 2500 }
            });

        private static readonly Histogram Duration = Metrics.CreateHistogram(
            "wishlist_export_duration_seconds",
            "Wish list export duration in seconds.",
            new HistogramConfiguration
            {
                LabelNames = LabelNames,
                Buckets = new double[] { 0.1, 1, 5, 10, 20, 40, 60 }
            });

        private readonly IIOLogger _logger;

        public PrometheusExportTelemetry(IIOLogger logger)
        {
            _logger = logger;
        }

        public void Record(ExportObservation observation)
        {
            var labels = new[] { observation.Operation, observation.Result };
            Operations.WithLabels(labels).Inc();
            RecordsRead.WithLabels(labels).Observe(observation.RecordsRead);
            RecordsReturned.WithLabels(labels).Observe(observation.RecordsReturned);
            BytesRead.WithLabels(labels).Observe(observation.BytesRead);
            PagesRead.WithLabels(labels).Observe(observation.PagesRead);
            Duration.WithLabels(labels).Observe(observation.DurationSeconds);

            var workflowType = observation.Result == ExportResultCode.Busy
                ? "WishListExportRejected"
                : "WishListExportCompleted";

            _logger.Info(workflowType, null, "Wish list export finished.", new[]
            {
                ("operation", observation.Operation),
                ("result", observation.Result),
                ("pageList", observation.PageList?.ToString(CultureInfo.InvariantCulture) ?? ""),
                ("pagesRead", observation.PagesRead.ToString(CultureInfo.InvariantCulture)),
                ("recordsRead", observation.RecordsRead.ToString(CultureInfo.InvariantCulture)),
                ("recordsReturned", observation.RecordsReturned.ToString(CultureInfo.InvariantCulture)),
                ("bytesRead", observation.BytesRead.ToString(CultureInfo.InvariantCulture)),
                ("durationMs", (observation.DurationSeconds * 1000).ToString(CultureInfo.InvariantCulture))
            });
        }
    }
}
