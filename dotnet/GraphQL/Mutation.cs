using GraphQL;
using GraphQL.Types;
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
                    SessionContext sessionContext = await wishListService.GetSessionContext();
                    if (!sessionContext.IsAuthenticated)
                    {
                        context.Errors.Add(new ExecutionError("Unauthorized") { Code = "Unauthorized" });
                        return null;
                    }

                    var listItem = context.GetArgument<ListItem>("listItem");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string listName = context.GetArgument<string>("name");
                    bool isPublic = context.GetArgument<bool>("public");

                    return await wishListService.SaveItem(listItem, shopperId, listName, isPublic, sessionContext.OrganizationId, sessionContext.CostCenterId);
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
                    SessionContext sessionContext = await wishListService.GetSessionContext();
                    if (!sessionContext.IsAuthenticated)
                    {
                        context.Errors.Add(new ExecutionError("Unauthorized") { Code = "Unauthorized" });
                        return null;
                    }

                    int id = context.GetArgument<int>("id");
                    string shopperId = context.GetArgument<string>("shopperId");
                    string listName = context.GetArgument<string>("name");

                    return await wishListService.RemoveItem(id, shopperId, listName, sessionContext.OrganizationId, sessionContext.CostCenterId);
                });
        }
    }
}
