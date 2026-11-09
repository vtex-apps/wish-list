using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WishList.Models;

namespace WishList.Exporting
{
    public sealed class WishListExportCollector : IWishListExportCollector
    {
        private const int PublicPageSize = 5000;

        private readonly IMasterDataScrollReader _reader;

        public WishListExportCollector(IMasterDataScrollReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<ExportResult> ExportAllAsync(ExportProgress progress, CancellationToken cancellationToken)
        {
            var output = new List<WishListWrapper>();
            var started = await StartAsync(progress, cancellationToken);
            var session = started.Session;
            progress?.Attach(session);
            var page = started.FirstPage;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                session.Accept(page);
                output.AddRange(page.Items);

                if (page.IsLast)
                {
                    return Result(output, session);
                }

                page = await ReadNextAsync(session, progress, cancellationToken);
            }
        }

        public async Task<ExportResult> ExportPageAsync(
            int pageList,
            ExportProgress progress,
            CancellationToken cancellationToken)
        {
            if (pageList < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageList));
            }

            var startIndex = checked(((long)pageList - 1) * PublicPageSize);
            var endExclusive = checked(startIndex + PublicPageSize);
            var output = new List<WishListWrapper>(PublicPageSize);
            var started = await StartAsync(progress, cancellationToken);
            var session = started.Session;
            progress?.Attach(session);
            var page = started.FirstPage;
            long absoluteIndex = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                session.Accept(page);

                foreach (var item in page.Items)
                {
                    if (absoluteIndex >= startIndex && absoluteIndex < endExclusive)
                    {
                        output.Add(item);
                    }

                    absoluteIndex++;
                    if (absoluteIndex >= endExclusive)
                    {
                        return Result(output, session);
                    }
                }

                if (page.IsLast)
                {
                    return Result(output, session);
                }

                page = await ReadNextAsync(session, progress, cancellationToken);
            }
        }

        private async Task<ScrollStart> StartAsync(ExportProgress progress, CancellationToken cancellationToken)
        {
            try
            {
                return await _reader.StartAsync(cancellationToken);
            }
            catch (WishListExportException exception) when (Record(progress, exception))
            {
                throw;
            }
        }

        private async Task<ScrollPage> ReadNextAsync(
            ScrollSession session,
            ExportProgress progress,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _reader.ReadNextAsync(session, cancellationToken);
            }
            catch (WishListExportException exception) when (Record(progress, exception))
            {
                throw;
            }
        }

        // Always returns false so the exception filter only observes the exception and never catches it.
        private static bool Record(ExportProgress progress, WishListExportException exception)
        {
            progress?.AddUnacceptedPage(exception.ReceivedPages, exception.ReceivedBytes);
            return false;
        }

        private static ExportResult Result(List<WishListWrapper> output, ScrollSession session)
        {
            return new ExportResult(new WishListsWrapper { WishLists = output }, session);
        }
    }
}
