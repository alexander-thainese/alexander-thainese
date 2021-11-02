using CF.Common;
using System.Data.Entity;

namespace CMT.DL.Core
{
    public class DbContextScope<TObjectContext> : Scope<TObjectContext>
           where TObjectContext : DbContext, new()
    {
        public DbContextScope()
                : base(Current ?? new TObjectContext(), Current == null)
        {
        }

        public DbContextScope(TObjectContext objectContext)
                : base(objectContext, false)
        {
        }
    }
}
