namespace CMT.Common.Interfaces
{
    public interface ISoftDeleteDbObject
    {
        bool IsDeleted { get; set; }
    }
}
