using System;
using System.Threading;
using System.Threading.Tasks;
using WishList.Models;

namespace WishList.Exporting
{
    public interface IWishListExportCoordinator
    {
        Task<WishListsWrapper> ExportAllAsync(Func<CancellationToken, Task> prepare, CancellationToken cancellationToken);
        Task<WishListsWrapper> ExportPageAsync(int pageList, Func<CancellationToken, Task> prepare, CancellationToken cancellationToken);
    }
}
