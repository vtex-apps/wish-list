using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("ListItem")]
    public class ListItemType : InputObjectGraphType<ListItem>
    {
        public ListItemType()
        {
            Name = "ListItemType";

            Field(b => b.Id, nullable: true).Description("Item Id.");
            Field(b => b.ProductId).Description("Product Id");
            Field(b => b.Sku).Description("Item sku");
            Field(b => b.Title).Description("Item title");
        }
    }
}
