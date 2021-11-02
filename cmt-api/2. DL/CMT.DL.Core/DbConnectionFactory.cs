using System;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;

namespace CMT.DL.Core
{
    public class DbConnectionFactory
    {
        public DbConnection CreateConnectionInternal(string connectionString)
        {
            return new EntityConnection(connectionString);
        }

        //public bool ConnectionStringsEqualInternal(string connectionString1, string connectionString2)
        //{
        //    return ConnectionStringComparer.AreEqual(connectionString1, connectionString2);
        //}

        private static DbConnectionFactory instance;
        public static DbConnectionFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    string message = string.Format("{0} has not beed registered yet.", typeof(DbConnectionFactory).FullName);
                    throw new InvalidOperationException(message);
                }

                return instance;
            }
        }

        public static void Register(DbConnectionFactory factory, bool throwOnFactoryAlreadyRegistered)
        {
            if (instance != null && throwOnFactoryAlreadyRegistered)
            {
                string message = string.Format("{0} has beed already registered.", typeof(DbConnectionFactory).FullName);
                throw new InvalidOperationException(message);
            }

            instance = factory;
        }

        public static void EnsureRegistered(DbConnectionFactory factory)
        {
            if (instance == null)
            {
                Register(factory, true);
            }
        }

        public static DbConnection CreateConnection(string connectionString)
        {
            return Instance.CreateConnectionInternal(connectionString);
        }

        public static bool ConnectionStringsEqual(string connectionString1, string connectionString2)
        {
            return true; //Instance.ConnectionStringsEqualInternal(connectionString1, connectionString2);
        }
    }
}
