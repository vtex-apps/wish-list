using System;

namespace WishList.Exporting
{
    public static class ExportResultCode
    {
        public const string Success = "success";
        public const string Busy = "busy";
        public const string Cancelled = "cancelled";
        public const string MasterDataError = "masterdata_error";
        public const string InvalidResponse = "invalid_response";
        public const string NoProgress = "no_progress";
    }

    public class WishListExportException : Exception
    {
        public WishListExportException(
            string resultCode,
            string message,
            Exception innerException = null,
            long receivedPages = 0,
            long receivedBytes = 0)
            : base(message, innerException)
        {
            ResultCode = resultCode;
            ReceivedPages = receivedPages;
            ReceivedBytes = receivedBytes;
        }

        public string ResultCode { get; }

        /// <summary>Pages received from Master Data that never reached the scroll session (e.g. unparseable).</summary>
        public long ReceivedPages { get; }

        /// <summary>Raw bytes of those pages.</summary>
        public long ReceivedBytes { get; }
    }

    public sealed class ScrollNoProgressException : WishListExportException
    {
        public ScrollNoProgressException()
            : base(ExportResultCode.NoProgress, "Master Data scroll returned the same non-terminal page twice.")
        {
        }
    }

    public sealed class ExportBusyException : WishListExportException
    {
        public ExportBusyException()
            : base(ExportResultCode.Busy, "Another export is already running on this instance.")
        {
        }
    }
}
