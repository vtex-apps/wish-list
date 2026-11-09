using System.Threading;
using System.Threading.Tasks;

namespace WishList.Exporting
{
    public interface IWishListExportCollector
    {
        Task<ExportResult> ExportAllAsync(ExportProgress progress, CancellationToken cancellationToken);
        Task<ExportResult> ExportPageAsync(int pageList, ExportProgress progress, CancellationToken cancellationToken);
    }
}
