using System.Data.Entity.Core.Objects.DataClasses;
using System.Runtime.Serialization;

namespace CMT.Common
{
    [EdmEnumType(Name = "ColumnSeparators", NamespaceName = "CMT.Common")]
    [DataContract()]
    public enum ColumnSeparators : byte
    {
        [EnumMember()]
        Semicolon = 0,
        [EnumMember()]
        Comma = 1,
        [EnumMember()]
        Tab = 2,
        [EnumMember()]
        Space = 3,
        [EnumMember()]
        Pipe = 4,
        [EnumMember()]
        None = 5,
    }
}