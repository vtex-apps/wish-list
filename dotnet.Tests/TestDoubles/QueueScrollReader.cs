using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WishList.Exporting;
using WishList.Models;

namespace WishList.Tests.TestDoubles
{
    public sealed class QueueScrollReader : IMasterDataScrollReader
    {
        private readonly Queue<ScrollPage> _pages;

        private QueueScrollReader(IEnumerable<ScrollPage> pages)
        {
            _pages = new Queue<ScrollPage>(pages);
        }

        public int ReadCount { get; private set; }

        public static QueueScrollReader Empty()
        {
            return new QueueScrollReader(new[]
            {
                new ScrollPage(new List<WishListWrapper>(), 2, true, "empty")
            });
        }

        public static QueueScrollReader WithNumberedPages(
            int pageCount,
            int pageSize,
            int? lastPageSize = null)
        {
            var nextId = 0;
            var pages = Enumerable.Range(0, pageCount).Select(index =>
            {
                var count = index == pageCount - 1 && lastPageSize.HasValue
                    ? lastPageSize.Value
                    : pageSize;
                var items = Enumerable.Range(nextId, count)
                    .Select(id => new WishListWrapper { Id = id.ToString() })
                    .ToList();
                nextId += count;
                return new ScrollPage(
                    items,
                    count * 100,
                    count < pageSize,
                    $"page-{index}");
            }).ToList();
            return new QueueScrollReader(pages);
        }

        public Task<ScrollStart> StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCount++;
            return Task.FromResult(new ScrollStart(
                new ScrollSession("test-token"),
                _pages.Dequeue()));
        }

        public Task<ScrollPage> ReadNextAsync(
            ScrollSession session,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCount++;
            if (_pages.Count == 0)
            {
                throw new InvalidOperationException("Collector read past the configured pages.");
            }

            return Task.FromResult(_pages.Dequeue());
        }
    }
}
