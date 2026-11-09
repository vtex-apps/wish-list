using System;
using System.Threading;

namespace WishList.Exporting
{
    public sealed class ExportConcurrencyGate : IExportConcurrencyGate
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public bool TryAcquire(out IDisposable lease)
        {
            if (!_semaphore.Wait(0))
            {
                lease = null;
                return false;
            }

            lease = new Lease(_semaphore);
            return true;
        }

        private sealed class Lease : IDisposable
        {
            private SemaphoreSlim _semaphore;

            public Lease(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                var semaphore = Interlocked.Exchange(ref _semaphore, null);
                semaphore?.Release();
            }
        }
    }
}
