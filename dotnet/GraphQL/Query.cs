using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WishList.GraphQL.Types;
using WishList.Models;
using WishList.Services;

namespace WishList.GraphQL
{
    [GraphQLMetadata("Query")]
    public class Query : ObjectGraphType<object>
    {
        public Query(IWishListService wishListService)
        {
            Name = "Query";

            FieldAsync<ListResponseType>(
                "viewList",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId", Description = "Shopper Id"},
                    new QueryArgument<StringGraphType> { Name = "name", Description = "List Name" },
                    new QueryArgument<IntGraphType> { Name = "from", Description = "From" },
                    new QueryArgument<IntGraphType> { Name = "to", Description = "To" }
                ),
                resolve: async context =>
                {
                    SessionContext sessionContext = await wishListService.GetSessionContext();
                    if (!sessionContext.IsAuthenticated)
                    {
                        context.Errors.Add(new ExecutionError("Unauthorized") { Code = "Unauthorized" });
                        return null;
                    }

                    string shopperId = context.GetArgument<string>("shopperId");
                    string name = context.GetArgument<string>("name");
                    int from = context.GetArgument<int>("from");
                    int to = context.GetArgument<int>("to");
                    IList<ListItem> resultList = new List<ListItem>();
                    int totalCount = 0;
                    var resultListsWrapper = await wishListService.GetList(shopperId, name, sessionContext.OrganizationId, sessionContext.CostCenterId);
                    var resultListWrapper = resultListsWrapper.ListItemsWrapper.FirstOrDefault();
                    if (resultListWrapper != null)
                    {
                        if (resultListWrapper.ListItems != null)
                        {
                            resultList = resultListWrapper.ListItems;
                            totalCount = resultList.Count;

                            if (from > 0 && to > 0)
                            {
                                resultList = await wishListService.LimitList(resultList, from, to);
                            }

                            foreach (ListItem listItem in resultList)
                            {
                                if (string.IsNullOrWhiteSpace(listItem.Title))
                                {
                                    listItem.Title = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            resultList = new List<ListItem>();
                        }
                    }

                    ListResponse listResponse = new ListResponse
                    {
                        Data = new DataElement { data = resultList },
                        Range = new ResultRange { From = from, To = to, Total = totalCount },
                        Public = resultListWrapper.IsPublic ?? false,
                        Name = resultListWrapper.Name
                    };

                    return listResponse;
                }
            );

            FieldAsync<ListGraphType<ListResponseType>>(
                "viewLists",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId", Description = "Shopper Id" },
                    new QueryArgument<IntGraphType> { Name = "from", Description = "From" },
                    new QueryArgument<IntGraphType> { Name = "to", Description = "To" }
                ),
                resolve: async context =>
                {
                    SessionContext sessionContext = await wishListService.GetSessionContext();
                    if (!sessionContext.IsAuthenticated)
                    {
                        context.Errors.Add(new ExecutionError("Unauthorized") { Code = "Unauthorized" });
                        return null;
                    }

                    string shopperId = context.GetArgument<string>("shopperId");
                    int from = context.GetArgument<int>("from");
                    int to = context.GetArgument<int>("to");
                    IList<ListItem> resultList = new List<ListItem>();
                    IList<ListResponse> resultLists = new List<ListResponse>();
                    int totalCount = 0;
                    var resultListsWrapper = await wishListService.GetLists(shopperId, sessionContext.OrganizationId, sessionContext.CostCenterId);
                    if (resultListsWrapper != null && resultListsWrapper.ListItemsWrapper != null)
                    {
                        foreach (ListItemsWrapper listItemsWrapper in resultListsWrapper.ListItemsWrapper)
                        {
                            if (listItemsWrapper != null)
                            {
                                if (listItemsWrapper.ListItems != null)
                                {
                                    resultList = listItemsWrapper.ListItems;
                                    totalCount = resultList.Count;

                                    if (from > 0 && to > 0)
                                    {
                                        resultList = await wishListService.LimitList(resultList, from, to);
                                    }

                                    foreach (ListItem listItem in resultList)
                                    {
                                        if (string.IsNullOrWhiteSpace(listItem.Title))
                                        {
                                            listItem.Title = string.Empty;
                                        }
                                    }
                                }
                                else
                                {
                                    resultList = new List<ListItem>();
                                }
                            }

                            ListResponse listResponse = new ListResponse
                            {
                                Data = new DataElement { data = resultList },
                                Range = new ResultRange { From = from, To = to, Total = totalCount },
                                Public = listItemsWrapper.IsPublic ?? false,
                                Name = listItemsWrapper.Name
                            };

                            resultLists.Add(listResponse);
                        }
                    }

                    return resultLists;
                }
                
            );

            FieldAsync<CheckListType>(
                "checkList",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId", Description = "Shopper Id" },
                    new QueryArgument<StringGraphType> { Name = "productId", Description = "Product Id" },
                    new QueryArgument<StringGraphType> { Name = "sku", Description = "Product Sku" }
                ),
                resolve: async context =>
                {
                    SessionContext sessionContext = await wishListService.GetSessionContext();
                    if (!sessionContext.IsAuthenticated)
                    {
                        context.Errors.Add(new ExecutionError("Unauthorized") { Code = "Unauthorized" });
                        return null;
                    }

                    string shopperId = context.GetArgument<string>("shopperId");
                    string productId = context.GetArgument<string>("productId");
                    string sku = context.GetArgument<string>("sku");
                    ResponseListWrapper resultListWrapper = await wishListService.GetLists(shopperId, sessionContext.OrganizationId, sessionContext.CostCenterId);
                    List<string> namesList = new List<string>();
                    List<int> idsList = new List<int>();
                    CheckListResponse checkListResponse = null;
                    if (resultListWrapper != null && resultListWrapper.ListItemsWrapper != null)
                    {
                        foreach (ListItemsWrapper listItemsWrapper in resultListWrapper.ListItemsWrapper)
                        {
                            ListItem listItem = new ListItem();
                            if (!string.IsNullOrEmpty(sku))
                            {
                                listItem = listItemsWrapper.ListItems.FirstOrDefault(l => l.ProductId.Equals(productId, StringComparison.OrdinalIgnoreCase) && l.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
                            }
                            else
                            {
                                listItem = listItemsWrapper.ListItems.FirstOrDefault(l => l.ProductId.Equals(productId, StringComparison.OrdinalIgnoreCase));
                            }

                            if(listItem != null)
                            {
                                namesList.Add(listItemsWrapper.Name);
                                idsList.Add(listItem.Id ?? -1);
                            }
                        }

                        checkListResponse = new CheckListResponse
                        {
                            InList = namesList.Count > 0,
                            ListNames = namesList.ToArray(),
                            ListIds = idsList.ToArray(),
                            message = resultListWrapper.message
                        };
                    }
                    else
                    {
                        checkListResponse = new CheckListResponse
                        {
                            InList = false,
                            ListNames = new string[0],
                            ListIds = new int[0],
                            message = resultListWrapper != null ? resultListWrapper.message : "No records returned."
                        };
                    }

                    return checkListResponse;
                }
            );

            FieldAsync<StringGraphType>(
                "listNames",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId", Description = "Shopper Id" }
                ),
                resolve: async context =>
                {
                    SessionContext sessionContext = await wishListService.GetSessionContext();
                    if (!sessionContext.IsAuthenticated)
                    {
                        context.Errors.Add(new ExecutionError("Unauthorized") { Code = "Unauthorized" });
                        return null;
                    }

                    string shopperId = context.GetArgument<string>("shopperId");
                    ResponseListWrapper allLists = await wishListService.GetLists(shopperId, sessionContext.OrganizationId, sessionContext.CostCenterId);
                    IList<ListItemsWrapper> listItemsWrappers = allLists.ListItemsWrapper;
                    var distinctListNames = listItemsWrappers.GroupBy(elem => elem.Name).Select(group => group.First());
                    var listName = distinctListNames.Select(n => n.Name);

                    return listName.ToArray();
                }
            );

            FieldAsync<IntGraphType>(
                 "listSize",
                 arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "email", Description = "Filter by email" },
                    new QueryArgument<StringGraphType> { Name = "organizationId", Description = "Filter by Organization Id" },
                    new QueryArgument<StringGraphType> { Name = "costCenterId", Description = "Filter by Cost Center Id" }
                 ),
                 resolve: async context =>
                 {
                     SessionContext sessionContext = await wishListService.GetSessionContext();
                     if (!sessionContext.IsAuthenticated)
                     {
                         HttpStatusCode adminAuth = await wishListService.IsValidAdminAuthUser();
                         if (adminAuth != HttpStatusCode.OK)
                         {
                             context.Errors.Add(new ExecutionError(adminAuth.ToString()) { Code = adminAuth.ToString() });
                             return null;
                         }
                     }

                     string email = context.GetArgument<string>("email");
                     string organizationId = context.GetArgument<string>("organizationId");
                     string costCenterId = context.GetArgument<string>("costCenterId");

                     int allListSize = await wishListService.GetListSizeBase(email, organizationId, costCenterId);

                     return allListSize;
                 }
             );

            FieldAsync<ListGraphType<WishListWrapperType>>(
                 "exportList",
                 arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "email", Description = "Filter by email" },
                    new QueryArgument<StringGraphType> { Name = "organizationId", Description = "Filter by Organization Id" },
                    new QueryArgument<StringGraphType> { Name = "costCenterId", Description = "Filter by Cost Center Id" }
                 ),
                 resolve: async context =>
                 {
                     SessionContext sessionContext = await wishListService.GetSessionContext();
                     if (!sessionContext.IsAuthenticated)
                     {
                         HttpStatusCode adminAuth = await wishListService.IsValidAdminAuthUser();
                         if (adminAuth != HttpStatusCode.OK)
                         {
                             context.Errors.Add(new ExecutionError(adminAuth.ToString()) { Code = adminAuth.ToString() });
                             return null;
                         }
                     }

                     string email = context.GetArgument<string>("email");
                     string organizationId = context.GetArgument<string>("organizationId");
                     string costCenterId = context.GetArgument<string>("costCenterId");

                     WishListsWrapper wishListsWrapper = await wishListService.ExportAllWishLists(email, organizationId, costCenterId);
                     return wishListsWrapper.WishLists;
                 }
             );

            FieldAsync<ListGraphType<WishListWrapperType>>(
                 "exportListPaged",
                 arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "pageList", Description = "page list" },
                    new QueryArgument<StringGraphType> { Name = "email", Description = "Filter by email" },
                    new QueryArgument<StringGraphType> { Name = "organizationId", Description = "Filter by Organization Id" },
                    new QueryArgument<StringGraphType> { Name = "costCenterId", Description = "Filter by Cost Center Id" }
                 ),
                 resolve: async context =>
                 {
                     SessionContext sessionContext = await wishListService.GetSessionContext();
                     if (!sessionContext.IsAuthenticated)
                     {
                         HttpStatusCode adminAuth = await wishListService.IsValidAdminAuthUser();
                         if (adminAuth != HttpStatusCode.OK)
                         {
                             context.Errors.Add(new ExecutionError(adminAuth.ToString()) { Code = adminAuth.ToString() });
                             return null;
                         }
                     }

                     int pageList = context.GetArgument<int>("pageList");
                     string email = context.GetArgument<string>("email");
                     string organizationId = context.GetArgument<string>("organizationId");
                     string costCenterId = context.GetArgument<string>("costCenterId");

                     WishListsWrapper wishListsWrapper = await wishListService.ExportAllWishListsPaged(pageList, email, organizationId, costCenterId);
                     return wishListsWrapper.WishLists;
                 }
             );

            FieldAsync<StringGraphType>(
                "scopeMode",
                resolve: async context =>
                {
                    return await wishListService.GetScopeMode();
                }
            );
        }
    }
}
