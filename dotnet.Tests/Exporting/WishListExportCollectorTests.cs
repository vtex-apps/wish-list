using System;
using System.Threading;
using System.Threading.Tasks;
using WishList.Exporting;
using WishList.Tests.TestDoubles;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class WishListExportCollectorTests
    {
        [Fact]
        public async Task FirstPublicPageStopsAfterExactlyTwentyFiveFullBatches()
        {
            var reader = QueueScrollReader.WithNumberedPages(pageCount: 26, pageSize: 200);
            var collector = new WishListExportCollector(reader);

            var result = await collector.ExportPageAsync(1, new ExportProgress(), CancellationToken.None);

            Assert.Equal(5000, result.Value.WishLists.Count);
            Assert.Equal("0", result.Value.WishLists[0].Id);
            Assert.Equal("4999", result.Value.WishLists[4999].Id);
            Assert.Equal(25, result.PagesRead);
            Assert.Equal(25, reader.ReadCount);
        }

        [Fact]
        public async Task SecondPublicPageDiscardsFirstFiveThousandRecords()
        {
            var reader = QueueScrollReader.WithNumberedPages(pageCount: 51, pageSize: 200);
            var collector = new WishListExportCollector(reader);

            var result = await collector.ExportPageAsync(2, new ExportProgress(), CancellationToken.None);

            Assert.Equal(5000, result.Value.WishLists.Count);
            Assert.Equal("5000", result.Value.WishLists[0].Id);
            Assert.Equal("9999", result.Value.WishLists[4999].Id);
            Assert.Equal(10000, result.RecordsRead);
            Assert.Equal(50, reader.ReadCount);
        }

        [Fact]
        public async Task PagePastEndReturnsEmptyList()
        {
            var reader = QueueScrollReader.WithNumberedPages(
                pageCount: 2,
                pageSize: 200,
                lastPageSize: 50);
            var collector = new WishListExportCollector(reader);

            var result = await collector.ExportPageAsync(2, new ExportProgress(), CancellationToken.None);

            Assert.Empty(result.Value.WishLists);
        }

        [Fact]
        public async Task FullExportPreservesOrderWithoutMergedJArray()
        {
            var reader = QueueScrollReader.WithNumberedPages(
                pageCount: 3,
                pageSize: 200,
                lastPageSize: 50);
            var collector = new WishListExportCollector(reader);

            var result = await collector.ExportAllAsync(new ExportProgress(), CancellationToken.None);

            Assert.Equal(450, result.Value.WishLists.Count);
            Assert.Equal("0", result.Value.WishLists[0].Id);
            Assert.Equal("449", result.Value.WishLists[449].Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task InvalidPageIsRejected(int pageList)
        {
            var collector = new WishListExportCollector(QueueScrollReader.Empty());
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => collector.ExportPageAsync(pageList, new ExportProgress(), CancellationToken.None));
        }
    }
}
