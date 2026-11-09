using System.Threading;
using System.Threading.Tasks;

namespace WishList.Exporting
{
    public interface IMasterDataScrollReader
    {
        Task<ScrollStart> StartAsync(CancellationToken cancellationToken);
        Task<ScrollPage> ReadNextAsync(ScrollSession session, CancellationToken cancellationToken);
    }
}
