using WishList.Models;

namespace WishList.Exporting
{
    public sealed class ExportResult
    {
        public ExportResult(WishListsWrapper value, ScrollSession session)
        {
            Value = value;
            PagesRead = session.PagesRead;
            RecordsRead = session.RecordsRead;
            BytesRead = session.BytesRead;
        }

        public WishListsWrapper Value { get; }
        public long PagesRead { get; }
        public long RecordsRead { get; }
        public long RecordsReturned => Value.WishLists.Count;
        public long BytesRead { get; }
    }
}
