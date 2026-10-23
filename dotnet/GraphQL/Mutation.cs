using GraphQL;
using GraphQL.Types;
using System.Collections.Generic;
using WishList.GraphQL.Types;
using WishList.Models;
using WishList.Services;

// using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;

namespace WishList.GraphQL
{
    [GraphQLMetadata("Mutation")]
    public class Mutation : ObjectGraphType<object>
    {
        public Mutation(IWishListService wishListService)
        {
            Name = "Mutation";

            FieldAsync<IntGraphType>(
                "addToList",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ListItemInputType>> { Name = "listItem" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId" },
                    new QueryArgument<StringGraphType> { Name = "name" },
                    new QueryArgument<BooleanGraphType> { Name = "public" }
                ),
                resolve: async context =>
                {
                    string shopperId = context.GetArgument<string>("shopperId");
                    HttpStatusCode accessStatus = await wishListService.EnsureCanAccessShopper(shopperId);
                    if (accessStatus != HttpStatusCode.OK)
                    {
                        context.Errors.Add(new ExecutionError(accessStatus.ToString())
                        {
                            Code = accessStatus.ToString()
                        });

                        return null;
                    }

                    var listItem = context.GetArgument<ListItem>("listItem");
                    string listName = context.GetArgument<string>("name");
                    bool isPublic = context.GetArgument<bool>("public");

                    return await wishListService.SaveItem(listItem, shopperId, listName, isPublic);
                });

            FieldAsync<BooleanGraphType>(
                "removeFromList",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId" },
                    new QueryArgument<StringGraphType> { Name = "name" }
                ),
                resolve: async context =>
                {
                    string shopperId = context.GetArgument<string>("shopperId");
                    HttpStatusCode accessStatus = await wishListService.EnsureCanAccessShopper(shopperId);
                    if (accessStatus != HttpStatusCode.OK)
                    {
                        context.Errors.Add(new ExecutionError(accessStatus.ToString())
                        {
                            Code = accessStatus.ToString()
                        });

                        return null;
                    }

                    int id = context.GetArgument<int>("id");
                    string listName = context.GetArgument<string>("name");

                    return await wishListService.RemoveItem(id, shopperId, listName);
                });
        }
    }
}