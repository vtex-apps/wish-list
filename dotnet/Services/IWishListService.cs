using System.Collections.Generic;
using System.Threading.Tasks;
using WishList.Models;
using System.Net;

namespace WishList.Services
{
    public interface IWishListService
    {
        Task<WishListWrapper> GetList(string shopperId, string listName);
        Task<ResponseListWrapper> GetLists(string shopperId);
        Task<bool> SaveList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId);
        Task<int?> SaveItem(ListItem listItem, string shopperId, string listName, bool? isPublic);
        Task<bool> RemoveItem(int itemId, string shopperId, string listName);
        Task<IList<ListItem>> LimitList(IList<ListItem> listItems, int from, int to);
        Task<HttpStatusCode> IsValidAuthUser();
        Task<ValidatedUser> ValidateUserToken(string token);
    }
}