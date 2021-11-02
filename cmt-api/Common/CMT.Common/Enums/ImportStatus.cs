using System.Data.Entity.Core.Objects.DataClasses;
using System.Runtime.Serialization;

namespace CMT.Common
{
    [EdmEnumType(Name = "ImportStatus", NamespaceName = "CMT.DL")]
    [DataContract()]
    public enum ImportStatus : byte
    {
        [EnumMember()]
        Created = 0,
        [EnumMember()]
        Uploaded = 1,//-
        [EnumMember()]
        Imported = 2,//-
        [EnumMember()]
        Processed = 3,//-
        [EnumMember()]
        UploadFailed = 4,//-
        [EnumMember()]
        ImportFailed = 5,//-

        [EnumMember()]
        FileReady = 6,
        [EnumMember()]
        ColumnRead = 7,
        [EnumMember()]
        InitialProcessingStarted = 8,
        [EnumMember()]
        InitialProcessingCompleted = 9,
        [EnumMember()]
        CampaignIdentifierSelected = 10,
        [EnumMember()]
        DistinctProcessingStarted = 11,
        [EnumMember()]
        DistinctProcessingCompleted = 12,
        //ReadyToMap,
        //Mapping,
        [EnumMember()]
        Mapped = 13,
        [EnumMember()]
        Failed = 14
    }
}
