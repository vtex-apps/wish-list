using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WishList.Exporting;
using WishList.Models;

namespace WishList.Tests.TestDoubles
{
    /// <summary>Returns a first page, then runs scripted steps (each returns a page or throws).</summary>
    public sealed class ScriptedScrollReader : IMasterDataScrollReader
    {
        private readonly ScrollPage _first;
        private readonly Queue<Func<ScrollPage>> _steps;

        public ScriptedScrollReader(ScrollPage first, params Func<ScrollPage>[] steps)
        {
            _first = first;
            _steps = new Queue<Func<ScrollPage>>(steps);
        }

        public static ScrollPage FullPage(string signature, int firstId = 0, int size = 200)
        {
            var items = new List<WishListWrapper>();
            for (var i = 0; i < size; i++)
            {
                items.Add(new WishListWrapper { Id = (firstId + i).ToString() });
            }

            return new ScrollPage(items, 1000, false, signature);
        }

        public Task<ScrollStart> StartAsync(CancellationToken cancellationToken)
        {
            if (_first == null)
            {
                throw new WishListExportException(ExportResultCode.MasterDataError, "start failed");
            }

            return Task.FromResult(new ScrollStart(new ScrollSession("test-token"), _first));
        }

        public Task<ScrollPage> ReadNextAsync(ScrollSession session, CancellationToken cancellationToken)
        {
            return Task.FromResult(_steps.Dequeue()());
        }
    }
}
