using System.Transactions;

namespace CMT.DL.Core
{
    public static class TransactionScopeBuilder
    {
        public static IsolationLevel Level = IsolationLevel.ReadCommitted;
        public static TransactionScope CreateScope()
        {
            return CreateScope(Level);
        }

        public static TransactionScope CreateScope(IsolationLevel isolationLevel)
        {
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = isolationLevel;

            TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options);
            return scope;
        }
    }
}
