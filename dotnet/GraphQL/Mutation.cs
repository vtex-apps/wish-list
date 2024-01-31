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

                    HttpStatusCode isValidAuthUser = await wishListService.IsValidAuthUser();
                    if (isValidAuthUser != HttpStatusCode.OK)
                    {
                        context.Errors.Add(new ExecutionError(isValidAuthUser.ToString())
                        {
                            Code = isValidAuthUser.ToString()
                        });

                        return null;
                    }
                    
                    var listItem = context.GetArgument<ListItem>("listItem");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string listName = context.GetArgument<string>("name");
                    bool isPublic = context.GetArgument<bool>("public");

                    return wishListService.SaveItem(listItem, shopperId, listName, isPublic);
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

                    HttpStatusCode isValidAuthUser = await wishListService.IsValidAuthUser();

                    if (isValidAuthUser != HttpStatusCode.OK)
                    {
                        context.Errors.Add(new ExecutionError(isValidAuthUser.ToString())
                        {
                            Code = isValidAuthUser.ToString()
                        });

                        return null;
                    }

                    int id = context.GetArgument<int>("id");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string listName = context.GetArgument<string>("name");

                    return wishListService.RemoveItem(id, shopperId, listName);
                });
        }
    }
}