namespace WishList.Data
{
    using WishList.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWishListRepository
    {
        Task<bool> SaveWishList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId);
        Task<ResponseListWrapper> GetWishList(string shopperId);
        Task<bool> DeleteWishList(string documentId);
        Task VerifySchema();

        Task <WishListsWrapper> GetAllLists();
    }
}
