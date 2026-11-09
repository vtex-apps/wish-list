using System;
using System.Collections.Generic;
using WishList.Models;

namespace WishList.Exporting
{
    public sealed class ScrollPage
    {
        public ScrollPage(IReadOnlyList<WishListWrapper> items, long rawBytes, bool isLast, string signature)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            RawBytes = rawBytes;
            IsLast = isLast;
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        public IReadOnlyList<WishListWrapper> Items { get; }
        public long RawBytes { get; }
        public bool IsLast { get; }
        public string Signature { get; }
    }
}
