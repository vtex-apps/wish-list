using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WishList.Data;
using WishList.Models;

namespace WishList.Services
{
    public class WishListService
    {
        private readonly IWishListRepository _wishListRepository;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WishListService(IWishListRepository wishListRepository, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            this._wishListRepository = wishListRepository ??
                                            throw new ArgumentNullException(nameof(wishListRepository));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                  throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<ListItemsWrapper> GetList(string shopperId, string listName)
        {
            return await _wishListRepository.GetWishList(shopperId, listName);
        }

        public async Task<bool> SaveList(IList<ListItem> listItems, string shopperId, string listName, bool isPublic)
        {
            bool success = false;

            ListItemsWrapper listItemsWrapper = await _wishListRepository.GetWishList(shopperId, listName);
            if(listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                foreach (ListItem listItem in listItems)
                {
                    listItemsWrapper.ListItems.Add(listItem);
                }
            }
            else
            {
                listItemsWrapper = new ListItemsWrapper { ListItems = listItems, Id = shopperId, IsPublic = isPublic, Name = listName };
            }

            return success;
        }
    }
}
