namespace WishList.Exporting
{
    public sealed class ExportObservation
    {
        public ExportObservation(
            string operation,
            string result,
            int? pageList,
            long pagesRead,
            long recordsRead,
            long recordsReturned,
            long bytesRead,
            double durationSeconds)
        {
            Operation = operation;
            Result = result;
            PageList = pageList;
            PagesRead = pagesRead;
            RecordsRead = recordsRead;
            RecordsReturned = recordsReturned;
            BytesRead = bytesRead;
            DurationSeconds = durationSeconds;
        }

        public string Operation { get; }
        public string Result { get; }
        public int? PageList { get; }
        public long PagesRead { get; }
        public long RecordsRead { get; }
        public long RecordsReturned { get; }
        public long BytesRead { get; }
        public double DurationSeconds { get; }

        public static ExportObservation Busy(string operation, int? pageList)
        {
            return new ExportObservation(
                operation, ExportResultCode.Busy, pageList, 0, 0, 0, 0, 0);
        }

        public static ExportObservation From(
            string operation,
            string result,
            int? pageList,
            ExportResult exportResult,
            ExportProgress progress,
            double durationSeconds)
        {
            return new ExportObservation(
                operation,
                result,
                pageList,
                exportResult?.PagesRead ?? progress?.PagesRead ?? 0,
                exportResult?.RecordsRead ?? progress?.RecordsRead ?? 0,
                exportResult?.RecordsReturned ?? 0,
                exportResult?.BytesRead ?? progress?.BytesRead ?? 0,
                durationSeconds);
        }
    }
}
