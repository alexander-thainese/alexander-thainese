using System.Data.Entity.Core.Objects.DataClasses;
using System.Runtime.Serialization;

namespace CMT.Common
{
    [EdmEnumType(Name = "ImportType", NamespaceName = "CMT.DL")]
    [DataContract()]
    public enum ImportType : byte
    {
        [EnumMember()]
        File = 0,
        [EnumMember()]
        API = 1

    }
}