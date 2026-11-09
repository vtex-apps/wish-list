namespace WishList.Exporting
{
    /// <summary>
    /// Per-operation view of the scroll counters. The collector attaches the request's
    /// <see cref="ScrollSession"/> as soon as it exists, so the coordinator can still report
    /// what was read when the export fails or is cancelled before an <see cref="ExportResult"/> exists.
    /// </summary>
    public sealed class ExportProgress
    {
        private ScrollSession _session;

        public void Attach(ScrollSession session)
        {
            _session = session;
        }

        private long _unacceptedPages;
        private long _unacceptedBytes;

        /// <summary>
        /// Counts a page that Master Data returned but that could not be turned into a
        /// <see cref="ScrollPage"/> (for example invalid JSON), so it never reached the session.
        /// </summary>
        public void AddUnacceptedPage(long pages, long bytes)
        {
            _unacceptedPages += pages;
            _unacceptedBytes += bytes;
        }

        public long PagesRead => (_session?.PagesRead ?? 0) + _unacceptedPages;
        public long RecordsRead => _session?.RecordsRead ?? 0;
        public long BytesRead => (_session?.BytesRead ?? 0) + _unacceptedBytes;
    }
}
