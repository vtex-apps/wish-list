using System;
using System.Collections.Generic;
using System.Text;

namespace WishList.Models
{
    public class CheckListResponse
    {
        public bool InList { get; set; }
        public string[] ListNames { get; set; }
        public int[] ListIds { get; set; }
        public string message { get; set; }
    }
}
