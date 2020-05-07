using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WishList.Data;
using WishList.Models;

namespace WishList.Services
{
    public class WishListService : IWishListService
    {
        private readonly IWishListRepository _wishListRepository;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const int MaximumReturnedRecords = 999;

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
            ListItemsWrapper listItemsWrapper = new ListItemsWrapper();
            WishListWrapper wishListWrapper = await _wishListRepository.GetWishList(shopperId);
            if (wishListWrapper != null)
            {
                Console.WriteLine($"wishListWrapper Count = {wishListWrapper.ListItemsWrapper.Count}");
                listItemsWrapper = wishListWrapper.ListItemsWrapper.Where(n => n.Name.Equals(listName)).FirstOrDefault();
                Console.WriteLine($"listItemsWrapper Count = {listItemsWrapper.ListItems.Count}");
            }
            else
            {
                Console.WriteLine("GetList Null/Empty");
            }

            return listItemsWrapper;
        }

        public async Task<WishListWrapper> GetLists(string shopperId)
        {
            return await _wishListRepository.GetWishList(shopperId);
        }

        public async Task<bool> SaveList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic)
        {
            IList<ListItem> listItemsToSave = null;

            ListItemsWrapper listItemsWrapper = await this.GetList(shopperId, listName);
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                listItemsToSave = listItemsWrapper.ListItems;
                foreach (ListItem listItem in listItems)
                {
                    listItemsToSave.Add(listItem);
                }
            }
            else
            {
                listItemsToSave = listItems;
            }

            return await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic);
        }

        public async Task<bool> SaveItem(ListItem listItem, string shopperId, string listName, bool? isPublic)
        {
            IList<ListItem> listItemsToSave = null;

            ListItemsWrapper listItemsWrapper = await this.GetList(shopperId, listName);
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                listItemsToSave = listItemsWrapper.ListItems;
                if(listItem.Id == null)
                {
                    int maxId = listItemsToSave.Max(t => t.Id ?? 0);
                    listItem.Id = ++maxId;
                }
                else
                {
                    // If an Id has been specified, remove existing item
                    ListItem itemToRemove = listItemsToSave.Where(r => r.Id == listItem.Id).FirstOrDefault();
                    if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                    {
                        listItemsToSave.Remove(itemToRemove);
                    }
                }

                listItemsToSave.Add(listItem);
            }
            else
            {
                listItem.Id = listItem.Id ?? 0;
                listItemsToSave = new List<ListItem> { listItem };
            }

            return await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic);
        }

        public async Task<bool> RemoveItem(int itemId, string shopperId, string listName)
        {
            bool wasRemoved = false;
            IList<ListItem> listItemsToSave = null;
            ListItemsWrapper listItemsWrapper = await this.GetList(shopperId, listName);
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                listItemsToSave = listItemsWrapper.ListItems;
                ListItem itemToRemove = listItemsToSave.Where(r => r.Id == itemId).FirstOrDefault();
                if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                {
                    wasRemoved = await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, listItemsWrapper.IsPublic);
                }
            }

            return wasRemoved;
        }

        public async Task<IList<ListItem>> LimitList(IList<ListItem> listItems, int from, int to)
        {
            int take = MaximumReturnedRecords;
            if (to > 0)
            {
                take = Math.Min((to - from) + 1, MaximumReturnedRecords);
                //Console.WriteLine($"    >>>>>>>>>>>>>>>>>  Take {take} reviews {from}-{to}");
            }

            listItems = listItems.Skip(from - 1).Take(take).ToList();

            return listItems;
        }
    }
}
