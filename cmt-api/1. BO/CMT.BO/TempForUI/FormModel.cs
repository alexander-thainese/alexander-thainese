using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract]
    public class Option
    {
        public Option(string name, object value)
        {
            Name = name;
            Value = value;
        }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "value")]
        public object Value { get; set; }
    }


    [DataContract]
    public class Item
    {
        public Item(Guid id, string label)
        {
            Id = id;
            Label = label;
        }
        [DataMember(Name = "id")]
        public Guid Id { get; set; }
        [DataMember(Name = "label")]
        public string Label { get; set; }
    }

    [DataContract]
    public class SchemaElementData
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int? Level { get; set; }
        [DataMember]
        public string LevelName { get; set; }
        [DataMember]
        public bool IsSingleLevel { get; set; }

        public SchemaElementData(Guid id, string name, int? level, string levelName, bool isSingleLevel)
        {
            Id = id;
            Name = name;
            Level = level;
            LevelName = levelName;
            IsSingleLevel = isSingleLevel;
        }
    }

    [DataContract]
    public class ListItemContainer
    {
        [DataMember]
        public List<Item> List { get; set; }
        [DataMember]
        public Guid ObjectId { get; set; }
        [DataMember]
        public string Name { get; set; }

    }
}
