namespace WishList.Data
{
    using WishList.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWishListRepository
    {
        Task<bool> SaveWishList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic);
        Task<ListItemsWrapper> GetWishList(string shopperId, string listName);
    }
}
