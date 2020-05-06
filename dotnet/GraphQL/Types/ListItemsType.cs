using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("ListItemsType")]
    public class ListItemsType : ListGraphType<ListItemType>
    {
        public ListItemsType()
        {
            Name = "ListItemsType";

            
        }
    }
}
