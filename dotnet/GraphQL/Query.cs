using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    new QueryArgument<StringGraphType> { Name = "shopperId", Description = "Shopper Id"},
                    new QueryArgument<StringGraphType> { Name = "name", Description = "List Name" },
                    new QueryArgument<IntGraphType> { Name = "from", Description = "From" },
                    new QueryArgument<IntGraphType> { Name = "to", Description = "To" }
                ),
                resolve: async context =>
                {
                    Console.WriteLine("[-] ViewList [-]");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string name = context.GetArgument<string>("name");
                    int from = context.GetArgument<int>("from");
                    int to = context.GetArgument<int>("to");
                    IList<ListItem> resultList = new List<ListItem>();
                    int totalCount = 0;
                    var resultListsWrapper = await wishListService.GetList(shopperId, name);
                    var resultListWrapper = resultListsWrapper.ListItemsWrapper.FirstOrDefault();
                    if (resultListWrapper != null)
                    {
                        resultList = resultListWrapper.ListItems;
                        if (resultListWrapper.ListItems != null)
                        {
                            totalCount = resultList.Count;

                            if (from > 0 && to > 0)
                            {
                                resultList = await wishListService.LimitList(resultList, from, to);
                                Console.WriteLine($"totalCount = {totalCount} : Filtered to {resultList.Count}");
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

            FieldAsync<CheckListType>(
                "checkList",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "shopperId", Description = "Shopper Id" },
                    new QueryArgument<StringGraphType> { Name = "productId", Description = "Product Id" },
                    new QueryArgument<StringGraphType> { Name = "sku", Description = "Product Sku" }
                ),
                resolve: async context =>
                {
                    Console.WriteLine("[-] CheckList [-]");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string productId = context.GetArgument<string>("productId");
                    string sku = context.GetArgument<string>("sku");
                    ResponseListWrapper resultListWrapper = await wishListService.GetLists(shopperId);
                    List<string> namesList = new List<string>();
                    CheckListResponse checkListResponse = null;
                    if (resultListWrapper != null && resultListWrapper.ListItemsWrapper != null)
                    {
                        foreach (ListItemsWrapper listItemsWrapper in resultListWrapper.ListItemsWrapper)
                        {
                            //Console.WriteLine($"[{resultListWrapper.ListItemsWrapper.Count}] Name = {listItemsWrapper.Name} [{listItemsWrapper.ListItems.Count}]");
                            //foreach(ListItem item in listItemsWrapper.ListItems)
                            //{
                            //    Console.WriteLine($"[{item.Id}] '{item.ProductId}' = {productId}? {item.ProductId.Equals(productId, StringComparison.OrdinalIgnoreCase)}");
                            //}

                            if (listItemsWrapper.ListItems.Select(l => l.ProductId.Equals(productId, StringComparison.OrdinalIgnoreCase)).Any())
                            {
                                namesList.Add(listItemsWrapper.Name);
                            }
                        }

                        checkListResponse = new CheckListResponse
                        {
                            InList = namesList.Count > 0,
                            ListNames = namesList.ToArray(),
                            message = resultListWrapper.message
                        };
                    }
                    else
                    {
                        checkListResponse = new CheckListResponse
                        {
                            InList = false,
                            ListNames = new string[0],
                            message = resultListWrapper != null ? resultListWrapper.message : "No records returned."
                        };
                    }

                    //Console.WriteLine($"[-] CheckList Response {JsonConvert.SerializeObject(checkListResponse)} [-]");

                    return checkListResponse;
                }
            );
        }
    }
}