using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;
using WishList.Services;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("WishListType")]
    public class WishListType : ObjectGraphType<ListItemsWrapper>
    {
        public WishListType(IWishListService wishListService)
        {
            Name = "WishListType";

            //Field(b => b.Id).Description("Shopper Id.");
            Field(b => b.ListItems, type: typeof(ListGraphType<ListItemsType>)).Description("List of Items.");
            Field(b => b.IsPublic, nullable: true).Description("The list is public.");
            Field(b => b.Name, nullable: true).Description("The name of the list.");
        }
    }
}
