using System;
using System.Collections.Generic;
using System.Text;

namespace WishList.Models
{
    public class ValidatedUser
    {
        public string AuthStatus { get; set; }
        public string Id { get; set; }
        public string User { get; set; }
        public string CustomerId { get; set; }

        /// <summary>
        /// True on a B2B representative token. Its customerId is then the contract id of the organization unit,
        /// shared by every member of the unit (ClaimsService in vtex-id-critical), not the shopper's own id.
        /// Tokens issued on behalf of a customer (assisted sales, with an act claim) carry the customer's own
        /// user, user id and customerId, so they need no special handling here.
        /// </summary>
        public bool? IsRepresentative { get; set; }
    }

}
