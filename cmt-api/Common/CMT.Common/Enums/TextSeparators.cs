using System.Data.Entity.Core.Objects.DataClasses;
using System.Runtime.Serialization;

namespace CMT.Common
{
    [EdmEnumType(Name = "TextSeparators", NamespaceName = "CMT.DL")]
    [DataContract()]
    public enum TextSeparators : byte
    {
        [EnumMember()]
        None = 0,
        [EnumMember()]
        Quotation = 1
    }
}