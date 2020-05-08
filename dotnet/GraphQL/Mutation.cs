using GraphQL;
using GraphQL.Types;
using System.Collections.Generic;
using WishList.GraphQL.Types;
using WishList.Models;
using WishList.Services;

namespace WishList.GraphQL
{
    [GraphQLMetadata("Mutation")]
    public class Mutation : ObjectGraphType<object>
    {
        public Mutation(IWishListService wishListService)
        {
            Name = "Mutation";

            Field<BooleanGraphType>(
                "addToList",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ListItemInputType>> { Name = "listItem" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId" },
                    new QueryArgument<StringGraphType> { Name = "name" },
                    new QueryArgument<BooleanGraphType> { Name = "public" }
                ),
                resolve: context =>
                {
                    var listItem = context.GetArgument<ListItem>("listItem");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string listName = context.GetArgument<string>("name");
                    bool isPublic = context.GetArgument<bool>("public");

                    return wishListService.SaveItem(listItem, shopperId, listName, isPublic);
                });

            Field<BooleanGraphType>(
                "removeFromList",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "shopperId" },
                    new QueryArgument<StringGraphType> { Name = "name" }
                ),
                resolve: context =>
                {
                    int id = context.GetArgument<int>("id");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string listName = context.GetArgument<string>("name");

                    return wishListService.RemoveItem(id, shopperId, listName);
                });
        }
    }
}