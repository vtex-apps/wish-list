using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;
using WishList.Services;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("CheckListType")]
    public class CheckListType : ObjectGraphType<CheckListResponse>
    {
        public CheckListType(IWishListService wishListService)
        {
            Name = "CheckListType";

            Field(b => b.InList).Description("The product is in a list");
            Field(b => b.ListNames).Description("The name of the lists that have the product.");
            Field(b => b.message, nullable: true).Description("Message returned from data layer.");
            Field(b => b.ListIds).Description("The id of the product in the list.");
        }
    }
}
