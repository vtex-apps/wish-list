using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;
using WishList.Data;
using WishList.Models;
using System.Net;

namespace WishList.Services
{
    public class WishListService : IWishListService
    {
        private readonly IWishListRepository _wishListRepository;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOServiceContext _context;

        private const int MaximumReturnedRecords = 999;

        public WishListService(IWishListRepository wishListRepository, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context)
        {
            this._wishListRepository = wishListRepository ??
                                            throw new ArgumentNullException(nameof(wishListRepository));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                  throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));
        }

        public async Task<WishListWrapper> GetList(string shopperId, string listName)
        {
            ListItemsWrapper listItemsWrapper = new ListItemsWrapper();
            WishListWrapper wishListWrapper = await _wishListRepository.GetWishList(shopperId);
            if (wishListWrapper != null && wishListWrapper.ListItemsWrapper != null)
            {
                listItemsWrapper = wishListWrapper.ListItemsWrapper.Where(n => n.Name.Equals(listName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (listItemsWrapper == null)
                {
                    listItemsWrapper = new ListItemsWrapper();
                }
            }
            else
            {
                _context.Vtex.Logger.Debug("GetList", null, $"Retrying... '{shopperId}' '{listName}'");
                wishListWrapper = await _wishListRepository.GetWishList(shopperId);
                if (wishListWrapper != null && wishListWrapper.ListItemsWrapper != null)
                {
                    listItemsWrapper = wishListWrapper.ListItemsWrapper.Where(n => n.Name.Equals(listName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (listItemsWrapper == null)
                    {
                        listItemsWrapper = new ListItemsWrapper();
                    }
                }
            }

            wishListWrapper.ListItemsWrapper = new List<ListItemsWrapper> { listItemsWrapper };

            return wishListWrapper;
        }

        public async Task<ResponseListWrapper> GetLists(string shopperId)
        {
            return await _wishListRepository.GetWishList(shopperId);
        }

        public async Task<bool> SaveList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId)
        {
            IList<ListItem> listItemsToSave = null;

            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                _context.Vtex.Logger.Debug("SaveList", null, $"Saving '{shopperId}' '{listName}' {listItems.Count} new items {listItemsWrapper.ListItems.Count} existing items.");
                listItemsToSave = listItemsWrapper.ListItems;
                foreach (ListItem listItem in listItems)
                {
                    listItemsToSave.Add(listItem);
                }
            }
            else
            {
                _context.Vtex.Logger.Debug("SaveList", null, $"Saving '{shopperId}' '{listName}' {listItems.Count} new items.");
                listItemsToSave = listItems;
            }

            return await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic, documentId);
        }

        public async Task<int?> SaveItem(ListItem listItem, string shopperId, string listName, bool? isPublic)
        {
            IList<ListItem> listItemsToSave = null;

            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' {listItemsWrapper.ListItems.Count} existing items.");
                listItemsToSave = listItemsWrapper.ListItems;
                foreach (ListItem item in listItemsToSave)
                {
                    if (listItem.ProductId ==  item.ProductId)
                    {
                        listItem.Id = item.Id;
                    }
                }
                if(listItem.Id == null)
                {
                    int maxId = 0;
                    if (listItemsToSave.Count > 0)
                    {
                        maxId = listItemsToSave.Max(t => t.Id ?? 0);
                    }

                    listItem.Id = ++maxId;
                    _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' Setting Id: {listItem.Id}");
                }
                else
                {
                    // If an Id has been specified, remove existing item
                    ListItem itemToRemove = listItemsToSave.Where(r => r.Id == listItem.Id).FirstOrDefault();
                    if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                    {
                        _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' Removing {listItem.Id}");
                        listItemsToSave.Remove(itemToRemove);
                    }
                }

                listItemsToSave.Add(listItem);
            }
            else
            {
                listItem.Id = listItem.Id ?? 0;
                listItemsToSave = new List<ListItem> { listItem };
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' First Item: {listItem.Id}");
            }

            if(await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic, wishListWrapper.Id))
            {
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' Saved: {listItem.Id}");
            }
            else
            {
                _context.Vtex.Logger.Warn("SaveItem", null, $"Saving '{shopperId}' '{listName}' Failed to save: {listItem.Id}");
            }

            return listItem.Id;
        }

        public async Task<bool> RemoveItem(int itemId, string shopperId, string listName)
        {
            bool wasRemoved = false;
            IList<ListItem> listItemsToSave = null;
            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                listItemsToSave = listItemsWrapper.ListItems;
                ListItem itemToRemove = listItemsToSave.FirstOrDefault(r => r.Id == itemId);
                if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                {
                    wasRemoved = await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, listItemsWrapper.IsPublic, wishListWrapper.Id);
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
            }

            listItems = listItems.Skip(from - 1).Take(take).ToList();

            return listItems;
        }

        public async Task<ValidatedUser> ValidateUserToken(string token)
        {
            ValidatedUser validatedUser = null;
            ValidateToken validateToken = new ValidateToken
            {
                Token = token
            };

            var jsonSerializedToken = JsonConvert.SerializeObject(validateToken);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/vtexid/credential/validate"),
                Content = new StringContent(jsonSerializedToken, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];

            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();

            try
            {
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    validatedUser = JsonConvert.DeserializeObject<ValidatedUser>(responseContent);
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("ValidateUserToken", null, $"Error validating user token", ex);
            }

            return validatedUser;
        }

        public async Task<HttpStatusCode> IsValidAuthUser()
        {
            if (string.IsNullOrEmpty(_context.Vtex.AdminUserAuthToken))
            {
                return HttpStatusCode.Unauthorized;
            }

            ValidatedUser validatedUser = null;

            try {
                validatedUser = await ValidateUserToken(_context.Vtex.AdminUserAuthToken);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("IsValidAuthUser", null, "Error fetching user", ex);

                return HttpStatusCode.BadRequest;
            }

            bool hasPermission = validatedUser != null && validatedUser.AuthStatus.Equals("Success");

            if (!hasPermission)
            {
                _context.Vtex.Logger.Warn("IsValidAuthUser", null, "User Does Not Have Permission");

                return HttpStatusCode.Forbidden;
            }

            return HttpStatusCode.OK;
        }

        public async Task<WishListsWrapper> ExportAllWishLists()
        {
            WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllLists();
            return wishListsWrapper;
        }
    }
}
