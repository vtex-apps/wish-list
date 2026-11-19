using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WishList.Models
{
    public class ValidatedUser
    {
        public string AuthStatus { get; set; }
        public string Id { get; set; }
        public string User { get; set; }
        public string CustomerId { get; set; }

        // Delegation (B2B representative) fields of the VTEX ID validate response
        public bool? IsRepresentative { get; set; }

        [JsonProperty("act")]
        public JToken Act { get; set; }

        [JsonProperty("delegationChain")]
        public string[] DelegationChain { get; set; }

        [JsonProperty("delegationId")]
        public string DelegationId { get; set; }

        /// <summary>
        /// True when the token acts on behalf of someone else. Its customerId may then belong to the represented
        /// customer, so it must not be used as the caller's own identity.
        /// </summary>
        public bool IsDelegated() =>
            IsRepresentative == true
            || (Act != null && Act.Type != JTokenType.Null && Act.HasValues)
            || (DelegationChain != null && DelegationChain.Length > 0)
            || !string.IsNullOrWhiteSpace(DelegationId);
    }

}
