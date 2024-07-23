using System;
using System.Collections.Generic;
using System.Text;

namespace WishList.Models
{
    public class ValidatedEmailToken
    {
        public string UserId { get; set; }
        public string User { get; set; }
        public string UserType { get; set; }
        public string TokenType { get; set; }
        public string Account { get; set; }
        public string Audience { get; set; }
    }

}
