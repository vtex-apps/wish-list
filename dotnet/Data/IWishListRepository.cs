namespace WishList.Data
{
    using WishList.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWishListRepository
    {
        Task<bool> SaveWishList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId, string scopeMode, string organizationId = null, string costCenterId = null);
        Task<ResponseListWrapper> GetWishList(string shopperId, string scopeMode, string organizationId = null, string costCenterId = null);
        Task<bool> DeleteWishList(string documentId);
        Task VerifySchema(string schemaJson);
        Task<int> GetListsSize(string scopeMode, string email = null, string organizationId = null, string costCenterId = null);
        Task<WishListsWrapper> GetAllLists(string scopeMode, string email = null, string organizationId = null, string costCenterId = null);
        Task<WishListsWrapper> GetAllListsPaged(int pageSize, string scopeMode, string email = null, string organizationId = null, string costCenterId = null);
    }
}
