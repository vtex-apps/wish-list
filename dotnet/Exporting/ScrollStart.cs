using System;

namespace WishList.Exporting
{
    public sealed class ScrollStart
    {
        public ScrollStart(ScrollSession session, ScrollPage firstPage)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            FirstPage = firstPage ?? throw new ArgumentNullException(nameof(firstPage));
        }

        public ScrollSession Session { get; }
        public ScrollPage FirstPage { get; }
    }
}
