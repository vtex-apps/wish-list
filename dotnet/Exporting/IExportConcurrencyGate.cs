using System;

namespace WishList.Exporting
{
    public interface IExportConcurrencyGate
    {
        bool TryAcquire(out IDisposable lease);
    }
}
