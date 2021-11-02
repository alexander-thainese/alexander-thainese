//using CF.Common;
//using System;
//using System.Data;
//using System.Data.Common;

//namespace CMT.DL.Core
//{
//    public class DbConnectionScope : IDisposable, IScope
//    {
//        private static readonly string LevelDataStoreKey = string.Format("{0}_Level", typeof(DbConnectionScope).FullName);
//        private static readonly string InstanceDataStoreKey = string.Format("{0}_Instance", typeof(DbConnectionScope).FullName);

//        private static int Level
//        {
//            get
//            {
//                int value;
//                return ScopeDataStore.Instance.TryGetObject(LevelDataStoreKey, out value) ? value : 0;
//            }

//            set
//            {
//                ScopeDataStore.Instance[LevelDataStoreKey] = value;
//            }
//        }

//        private static DbConnectionScope Instance
//        {
//            get
//            {
//                DbConnectionScope instance;
//                ScopeDataStore.Instance.TryGetObject(InstanceDataStoreKey, out instance);
//                return instance;
//            }

//            set
//            {
//                ScopeDataStore.Instance[InstanceDataStoreKey] = value;
//            }
//        }

//        public static DbConnectionScope Current
//        {
//            get
//            {
//                return Instance;
//            }
//        }

//        public Guid ScopeId { get; private set; }
//        public static string GlobalConnectionString { get; protected set; }
//        public string ConnectionString { get; protected set; }
//        public bool ProfilingEnabled { get; protected set; }
//        public bool OwnsConnection { get; protected set; }
//        public DbConnection Connection { get; protected set; }

//        public static DbConnectionScope Create()
//        {
//            return Create(GlobalConnectionString);
//        }

//        public static DbConnectionScope Create(string connectionString)
//        {
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                throw new ArgumentNullException(connectionString);
//            }

//            if (Level == 0)
//            {
//                Instance = new DbConnectionScope(connectionString);
//            }
//            else
//            {
//                if (!DbConnectionFactory.ConnectionStringsEqual(Instance.ConnectionString, connectionString))
//                {
//                    throw new InvalidOperationException("Db connection scope already holds connection with different connection string than currently requested.");
//                }
//            }

//            Level++;
//            return Instance;
//        }

//        public static DbConnectionScope Create(DbConnection connection)
//        {
//            if (Level == 0)
//            {
//                Instance = new DbConnectionScope(connection, false);
//            }
//            else
//            {
//                DbConnectionScope connectionScope = Instance;
//                if (connectionScope.Connection != connection)
//                {
//                    throw new InvalidOperationException("Connection scope already holds different connection.");
//                }
//            }

//            Level++;
//            return Instance;
//        }

//        protected DbConnectionScope()
//        {
//            ScopeId = Guid.NewGuid();
//            ScopeDebugger.WriteInfo(this, ScopeDebugger.Stage.Creating, true);
//        }

//        protected DbConnectionScope(string connectionString)
//            : this()
//        {
//            ScopeDebugger.WriteInfo(this, "Scope initialized using connection string.");

//            ConnectionString = connectionString;
//            OwnsConnection = true;
//            Connection = CreateConnection(connectionString);
//        }

//        protected DbConnectionScope(DbConnection connection, bool profilingEnabled)
//            : this()
//        {
//            ScopeDebugger.WriteInfo(this, "Scope initialized using existing connection.");

//            ConnectionString = connection.ConnectionString;
//            ProfilingEnabled = profilingEnabled;
//            OwnsConnection = true;

//            //Ensure connection opened
//            if (connection.State == ConnectionState.Closed)
//            {
//                connection.Open();
//            }

//            Connection = connection;
//        }

//        public void Dispose()
//        {
//            Level--;

//            if (Level == 0)
//            {
//                DisposeInternal(this);
//            }
//            else if (Level < 0)
//            {
//                throw new Exception("Disposed out of order.");
//            }
//        }

//        protected static void DisposeInternal(DbConnectionScope connectionScope)
//        {
//            ScopeDebugger.WriteInfo(connectionScope, ScopeDebugger.Stage.Disposing, true);

//            if (connectionScope.OwnsConnection)
//            {
//                if (connectionScope.Connection.State == ConnectionState.Open)
//                {
//                    connectionScope.Connection.Close();
//                }

//                connectionScope.Connection.Dispose();
//            }
//            connectionScope.Connection = null;

//            ScopeDataStore.Instance.RemoveObject(InstanceDataStoreKey);
//            Level = 0;
//        }

//        public static void SetConnectionString(string connectionString, bool throwOnConnectionStringAlreadyExist)
//        {
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                throw new ArgumentNullException("connectionString");
//            }

//            if (!string.IsNullOrEmpty(GlobalConnectionString) && throwOnConnectionStringAlreadyExist)
//            {
//                string message = string.Format("ConnectionString in {0} has beed already initialized.", typeof(DbConnectionScope).FullName);
//                throw new InvalidOperationException(message);
//            }

//            GlobalConnectionString = connectionString;
//        }

//        public static void EnsureConnectionStringSet(string connectionString)
//        {
//            if (string.IsNullOrEmpty(GlobalConnectionString))
//            {
//                SetConnectionString(connectionString, true);
//            }
//        }

//        public static void Reset()
//        {
//            if (Instance != null)
//            {
//                DisposeInternal(Instance);
//            }
//        }

//        public static DbConnection GetConnection(string connectionString)
//        {
//            DbConnectionScope connectionScope;
//            DbConnection currentConnection = GetCurrentConnection(connectionString, out connectionScope);

//            if (currentConnection != null)
//            {
//                ScopeDebugger.WriteInfo(connectionScope, "Reusing scope connection.", true);
//                return currentConnection;
//            }

//            return Create(connectionString).Connection;
//        }

//        protected static DbConnection GetCurrentConnection(string connectionString, out DbConnectionScope connectionScope)
//        {
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                throw new ArgumentNullException("connectionString");
//            }

//            connectionScope = Instance;
//            if (connectionScope == null)
//            {
//                return null;
//            }

//            if (!DbConnectionFactory.ConnectionStringsEqual(connectionScope.ConnectionString, connectionString))
//            {
//                throw new InvalidOperationException("Db connection scope already holds connection with different connecion string than currently requested.");
//            }

//            return connectionScope.Connection;
//        }

//        protected DbConnection CreateConnection(string connectionString)
//        {
//            ScopeDebugger.WriteInfo(this, "Creating new connection.");

//            DbConnection connection = DbConnectionFactory.CreateConnection(connectionString);

//            connection.Open();

//            return connection;
//        }
//    }
//}
