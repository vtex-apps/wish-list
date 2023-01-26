using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("ListItem")]
    public class ListItemsType : ObjectGraphType<ListItemsWrapper>
    {
        public ListItemsType()
        {
            Name = "ListItemsType";

            Field(b => b.ListItems, type: typeof(ListGraphType<ListItemType>)).Description("List items");
            Field(b => b.IsPublic, nullable: true).Description("If the list is public");
            Field(b => b.Name, nullable: true).Description("List name");
        }
    }
}
