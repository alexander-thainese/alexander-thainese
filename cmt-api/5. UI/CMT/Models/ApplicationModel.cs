using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMT.Models
{
    [DataContract(Name = "Application")]
    public class ApplicationModel
    {
        [DataMember(Name = "Channel")]
        public Guid ChannelId { get; set; }
        [DataMember]
        public List<SelectItemModel> Sources { get; set; }

        public ApplicationModel()
        {
            Sources = new List<Models.SelectItemModel>();
        }
    }
}