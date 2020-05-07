namespace WishList.Data
{
    using WishList.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWishListRepository
    {
        Task<bool> SaveWishList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic);
        Task<WishListWrapper> GetWishList(string shopperId);
    }
}
