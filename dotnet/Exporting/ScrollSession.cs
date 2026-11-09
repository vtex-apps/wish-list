using System;

namespace WishList.Exporting
{
    public sealed class ScrollSession
    {
        public ScrollSession(string token)
        {
            Token = string.IsNullOrWhiteSpace(token)
                ? throw new ArgumentException("A scroll token is required.", nameof(token))
                : token;
        }

        public string Token { get; }
        public long PagesRead { get; private set; }
        public long RecordsRead { get; private set; }
        public long BytesRead { get; private set; }
        public string PreviousSignature { get; private set; }

        public void Accept(ScrollPage page)
        {
            var repeated = !page.IsLast && PreviousSignature == page.Signature;

            // The page was received from Master Data even when it proves there is no progress.
            PagesRead++;
            RecordsRead += page.Items.Count;
            BytesRead += page.RawBytes;
            PreviousSignature = page.Signature;

            if (repeated)
            {
                throw new ScrollNoProgressException();
            }
        }
    }
}
