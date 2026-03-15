using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WishList.Models;

namespace WishList.Services
{
    public interface IWishListService
    {
        Task<WishListWrapper> GetList(string shopperId, string listName, string organizationId = null, string costCenterId = null);
        Task<ResponseListWrapper> GetLists(string shopperId, string organizationId = null, string costCenterId = null);
        Task<bool> SaveList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId, string organizationId = null, string costCenterId = null);
        Task<int?> SaveItem(ListItem listItem, string shopperId, string listName, bool? isPublic, string organizationId = null, string costCenterId = null);
        Task<bool> RemoveItem(int itemId, string shopperId, string listName, string organizationId = null, string costCenterId = null);
        Task<IList<ListItem>> LimitList(IList<ListItem> listItems, int from, int to);
        Task<SessionContext> GetSessionContext();
        Task<HttpStatusCode> IsValidAdminAuthUser();
        Task<string> GetScopeMode();
        Task<int> GetListSizeBase(string email = null, string organizationId = null, string costCenterId = null);
        Task<WishListsWrapper> ExportAllWishLists(string email = null, string organizationId = null, string costCenterId = null);
        Task<WishListsWrapper> ExportAllWishListsPaged(int pageList, string email = null, string organizationId = null, string costCenterId = null);
    }
}
