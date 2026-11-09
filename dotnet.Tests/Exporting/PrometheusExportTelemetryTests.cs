using System;
using System.Linq;
using Moq;
using Prometheus;
using Vtex.Api.Services;
using WishList.Exporting;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class PrometheusExportTelemetryTests
    {
        private static readonly string[] Forbidden =
            { "account", "token", "email", "shopper", "payload", "items" };

        private static (string Workflow, string Message, (string, string)[] Fields) Capture(ExportObservation observation)
        {
            var logger = new Mock<IIOLogger>();
            (string, string)[] captured = null;
            string workflow = null;
            string message = null;
            logger
                .Setup(value => value.Info(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<(string, string)[]>()))
                .Callback<string, string, string, (string, string)[]>(
                    (type, instance, text, fields) =>
                    {
                        workflow = type;
                        message = text;
                        captured = fields;
                    });

            new PrometheusExportTelemetry(logger.Object).Record(observation);
            return (workflow, message, captured);
        }

        [Fact]
        public void BusyObservationUsesRejectedWorkflowAndSafeFields()
        {
            var (workflow, _, captured) = Capture(ExportObservation.Busy("full", null));

            Assert.Equal("WishListExportRejected", workflow);
            Assert.Contains(captured, field => field.Item1 == "operation" && field.Item2 == "full");
            Assert.Contains(captured, field => field.Item1 == "result" && field.Item2 == "busy");
            Assert.DoesNotContain(captured, field =>
                Forbidden.Contains(field.Item1, StringComparer.OrdinalIgnoreCase));
        }

        [Fact]
        public void SuccessObservationUsesCompletedWorkflowAndAllCounters()
        {
            var (workflow, message, captured) = Capture(
                new ExportObservation("paged", ExportResultCode.Success, 2, 25, 5000, 4000, 1024, 1.5));

            Assert.Equal("WishListExportCompleted", workflow);
            var names = captured.Select(field => field.Item1).ToArray();
            foreach (var expected in new[]
            {
                "operation", "result", "pageList", "pagesRead",
                "recordsRead", "recordsReturned", "bytesRead", "durationMs"
            })
            {
                Assert.Contains(expected, names);
            }

            Assert.Equal("1500", captured.Single(field => field.Item1 == "durationMs").Item2);
            Assert.DoesNotContain(names, name =>
                Forbidden.Contains(name, StringComparer.OrdinalIgnoreCase));
            Assert.Equal("Wish list export finished.", message);
        }

        [Fact]
        public void RecordPublishesSixFamiliesWithOnlyOperationAndResultLabels()
        {
            Capture(new ExportObservation("full", ExportResultCode.Success, null, 1, 2, 2, 100, 0.2));

            var registry = Metrics.DefaultRegistry;
            var text = new System.IO.MemoryStream();
            registry.CollectAndExportAsTextAsync(text, default).GetAwaiter().GetResult();
            var output = System.Text.Encoding.UTF8.GetString(text.ToArray());

            foreach (var family in new[]
            {
                "wishlist_export_operations_total", "wishlist_export_records_read",
                "wishlist_export_records_returned", "wishlist_export_bytes_read",
                "wishlist_export_pages_read", "wishlist_export_duration_seconds"
            })
            {
                Assert.Contains("# TYPE " + family, output);
            }

            var series = output.Split('\n').Where(line => line.StartsWith("wishlist_export_") && line.Contains("{"));
            foreach (var line in series)
            {
                var labels = line.Substring(line.IndexOf('{') + 1, line.IndexOf('}') - line.IndexOf('{') - 1);
                var names = labels.Split(',').Select(pair => pair.Split('=')[0]).Where(name => name != "le");
                Assert.Equal(new[] { "operation", "result" }, names.OrderBy(name => name).ToArray());
            }
        }
    }
}
