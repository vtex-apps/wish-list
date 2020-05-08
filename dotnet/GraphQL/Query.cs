using GraphQL;
using GraphQL.Types;
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
                    var resultListWrapper = await wishListService.GetList(shopperId, name);
                    var resultList = resultListWrapper.ListItems;
                    int totalCount = resultList.Count;

                    if (from > 0 && to > 0)
                    {
                        resultList = await wishListService.LimitList(resultList, from, to);
                        Console.WriteLine($"totalCount = {totalCount} : Filtered to {resultList.Count}");
                    }

                    ListResponse listResponse = new ListResponse
                    {
                        Data = new DataElement { data = resultList },
                        Range = new ResultRange { From = from, To = to, Total = totalCount }
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
                    var resultListWrapper = await wishListService.GetLists(shopperId);
                    List<string> namesList = new List<string>();
                    CheckListResponse checkListResponse = null;
                    if (resultListWrapper != null && resultListWrapper.ListItemsWrapper != null)
                    {
                        foreach (ListItemsWrapper listItemsWrapper in resultListWrapper.ListItemsWrapper)
                        {
                            if (listItemsWrapper.ListItems.Select(l => l.ProductId.Equals(productId)).FirstOrDefault())
                            {
                                namesList.Add(listItemsWrapper.Name);
                            }
                        }

                        checkListResponse = new CheckListResponse
                        {
                            InList = namesList.Count > 0,
                            ListNames = namesList.ToArray()
                        };
                    }

                    return checkListResponse;
                }
            );
        }
    }
}