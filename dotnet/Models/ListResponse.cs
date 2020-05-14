using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WishList.Models
{
    public class ListResponse
    {
        [DataMember(Name = "data")]
        public DataElement Data { get; set; }

        [DataMember(Name = "range")]
        public ResultRange Range { get; set; }

        [DataMember(Name = "public")]
        public bool Public { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    public class ResultRange
    {
        [DataMember(Name = "total")]
        public long Total { get; set; }

        [DataMember(Name = "from")]
        public long From { get; set; }

        [DataMember(Name = "to")]
        public long To { get; set; }
    }

    public class DataElement : IEnumerable<ListItem>
    {
        public IList<ListItem> data;

        public IEnumerator<ListItem> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
