using AutoMapper;
using CMT.BO;
using CMT.Common;
using CMT.Common.Interfaces;
using CMT.DL.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

namespace CMT.BL.Core
{
    public class BaseObjectManager<TDbContext, TDbObject, TBusinessObject> : IDisposable
        where TDbObject : class, IDbObject, new()
        where TBusinessObject : class, IBusinessObject, new()
        where TDbContext : DbContext, new()
    {
        private readonly bool ownsDbContextInstance;
        protected IMapper Mapper { get; set; }
        private bool disposed;

        public virtual TDbContext DbContext { get; protected set; }
        private bool IsAuditable { get; set; }

        public DbSet<TDbObject> DbSet
        {
            get
            {
                return DbContext.Set<TDbObject>();
            }
        }

        public virtual DbSet<TDbObject> DbQueryable
        {
            get
            {
                DbSet<TDbObject> queryable = DbSet;


                if (typeof(ISoftDeleteDbObject).IsAssignableFrom(typeof(TDbObject)))
                {
                    Expression<Func<TDbObject, bool>> sPredicate = PredicateConverter.Convert<ISoftDeleteDbObject, TDbObject>(o => !o.IsDeleted);
                    queryable = (DbSet<TDbObject>)queryable.Where(sPredicate);
                }
                return queryable;
            }
        }

        // #warning Do poprawy - zmiana na scope'a
        public BaseObjectManager()
            : this(DbContextScope<TDbContext>.Current ?? new TDbContext(), DbContextScope<TDbContext>.Current == null)
        {
        }

        protected BaseObjectManager(TDbContext dbContext)
            : this(dbContext, true)
        {
        }

        protected BaseObjectManager(TDbContext objectContext, bool ownsDbContextInstance)
        {
            DbContext = objectContext;
            objectContext.Database.CommandTimeout = ApplicationSettings.CommandTimeout;
            this.ownsDbContextInstance = ownsDbContextInstance;
            IsAuditable = typeof(IAuditableObject).IsAssignableFrom(typeof(TDbObject));
            InitializeMapper();
        }

        public virtual void InitializeMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TDbObject, TBusinessObject>();
                cfg.CreateMap<TBusinessObject, TDbObject>();
            });

            Mapper = config.CreateMapper();
        }

        protected virtual TDbObject GetDbObject(Guid objectId)
        {
            return DbSet.Single(p => p.ObjectId.Equals(objectId));
        }



        protected virtual bool IsReadOnly
        {
            get { return false; }
        }


        public TBusinessObject ConvertToBusinessObject(TDbObject dbObject)
        {
            TBusinessObject businessObject = new TBusinessObject();

            Mapper.Map(dbObject, businessObject);

            return businessObject;
        }

        public TBo ConvertToBusinessObject<TBo, TDbo>(TDbo dbObject) where TBo : class, new()
        {
            TBo businessObject = new TBo();

            Mapper.Map(dbObject, businessObject);

            return businessObject;
        }

        protected void UpdateDbObject(TDbObject dbObject, TBusinessObject businessObject)
        {
            Mapper.Map(businessObject, dbObject);
        }

        protected virtual void UpdateAuditData(TDbObject dbObject, bool isCreating)
        {
            if (IsAuditable)
            {
                UpdateAuditDataInternal((IAuditableObject)dbObject, isCreating);
            }
        }
        protected virtual void UpdateAuditDataInternal(IAuditableObject dbObject, bool isCreating)
        {
            AuditDataEventArgs eventArgs = AuditDataHelper.GetAuditData();

            if (isCreating)
            {
                dbObject.CreateUser = eventArgs.User;
                dbObject.CreateDate = DateTime.UtcNow;
            }
            else
            {
                dbObject.ChangeUser = eventArgs.User;
                dbObject.ChangeDate = DateTime.UtcNow;
            }
        }
        protected virtual void UpdateAuditData(TBusinessObject businessObject, TDbObject dbObject, bool isInserting)
        {
            if (IsAuditable)
            {
                UpdateAuditDataInternal((IAuditableObject)businessObject, (IAuditableObject)dbObject, isInserting);
            }
        }
        protected virtual void UpdateAuditDataInternal(IAuditableObject businessObject, IAuditableObject dbObject, bool isInserting)
        {
            if (isInserting)
            {
                businessObject.CreateDate = dbObject.CreateDate;
                businessObject.CreateUser = dbObject.CreateUser;
            }
            else
            {
                businessObject.ChangeDate = dbObject.ChangeDate;
                businessObject.ChangeUser = dbObject.ChangeUser;
            }
        }

        protected IEnumerable<TBusinessObject> ConvertToBusinessObjects(IQueryable<TDbObject> query)
        {
            IEnumerable<TBusinessObject> businessObjects = query.AsEnumerable().Select(o => ConvertToBusinessObject(o));

            return businessObjects;
        }

        public virtual List<Guid> GetSystemRolesInEntityContext(Guid objectId)
        {
            return null;
        }

        public bool ObjectExists(Guid objectId, bool throwOnNotExists)
        {
            if (objectId == Guid.Empty)
            {
                return false;
            }

            bool exists = DbQueryable.Any(o => o.ObjectId.Equals(objectId));

            if (!exists && throwOnNotExists)
            {
                string message = string.Format("Object of type {0} with object id = {1} does not exist.", typeof(TDbObject).Name, objectId.ToString());
                throw new Exception(message);
            }

            return exists;
        }

        public bool ObjectExists(Guid objectId)
        {
            return ObjectExists(objectId, false);
        }

        public bool ObjectsExist(Expression<Func<TDbObject, bool>> predicate)
        {
            return DbQueryable.Any(predicate);
        }

        public bool ObjectsExistUsingBOPredicate(Expression<Func<TBusinessObject, bool>> predicate)
        {
            return DbQueryable.Any(ConvertToDbPredicate(predicate));
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public virtual List<TBusinessObject> GetObjects()
        {
            List<TBusinessObject> list = ConvertToBusinessObjects(DbQueryable).ToList();

            return list;
        }

        public List<TBusinessObject> GetObjects(IQueryable<TDbObject> query)
        {
            List<TBusinessObject> list = ConvertToBusinessObjects(query).ToList();

            return list;
        }

        public List<TBusinessObject> GetObjects(Expression<Func<TDbObject, bool>> predicate, List<string> includes = null)
        {
            IQueryable<TDbObject> query = DbQueryable;
            if (includes != null)
            {
                foreach (string include in includes)
                {
                    query = query.Include(include);
                }
            }
            query = query.Where(predicate);

            List<TBusinessObject> list = ConvertToBusinessObjects(query).ToList();

            return list;
        }

        public List<TBusinessObject> GetObjects(Expression<Func<TDbObject, bool>> predicate, params Expression<Func<TDbObject, bool>>[] predicates)
        {
            IQueryable<TDbObject> query = ApplyPredicates(DbQueryable.Where(predicate), predicates);
            List<TBusinessObject> list = ConvertToBusinessObjects(query).ToList();

            return list;
        }

        public static IQueryable<TDbObject> ApplyPredicates(IQueryable<TDbObject> query, params Expression<Func<TDbObject, bool>>[] predicates)
        {
            foreach (Expression<Func<TDbObject, bool>> predicate in predicates)
            {
                query = query.Where(predicate);
            }

            return query;
        }

        public List<TBusinessObject> GetObjectsUsingBOPredicate(Expression<Func<TBusinessObject, bool>> predicate, List<string> includes = null)
        {
            return GetObjects(ConvertToDbPredicate(predicate), includes);
        }

        protected Expression<Func<TDbObject, bool>> ConvertToDbPredicate(Expression<Func<TBusinessObject, bool>> predicate)
        {
            return PredicateConverter.Convert<TBusinessObject, TDbObject>(predicate);
        }

        public int GetObjectCount()
        {
            int count = DbQueryable.Count();
            return count;
        }

        public int GetObjectCount(Expression<Func<TDbObject, bool>> predicate)
        {
            int count = DbQueryable.Where(predicate).Count();
            return count;
        }

        public int GetObjectCountUsingBOPredicate(Expression<Func<TBusinessObject, bool>> predicate)
        {
            return GetObjectCount(ConvertToDbPredicate(predicate));
        }

        public virtual bool TryGetObject(Guid objectId, out TBusinessObject businessObject)
        {
            if (objectId == Guid.Empty)
            {
                businessObject = null;
                return false;
            }

            try
            {
                businessObject = GetObject(objectId);
                return true;
            }
            catch (Exception)
            {
                businessObject = null;
                return false;
            }
        }

#if !MONOSYNC


#endif
        public TBusinessObject GetObject(Guid objectId)
        {
            TDbObject dbObject = GetDbObject(objectId);
            TBusinessObject businessObject = ConvertToBusinessObject(dbObject);

            return businessObject;
        }

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public void InsertObject(TBusinessObject businessObject)
        {
            InsertObject(businessObject, false);
        }

        public void InsertObject(TBusinessObject businessObject, bool preserveObjectId)
        {
            try
            {

                //if (this.IsReadOnly)
                //    throw new InvalidOperationException(CF_BL.ObjectManagerBase_DeleteObjects_Manager_operates_on_sql_view_hence_record_adding_is_not_possible);

                //using (DbConnectionScope connectionScope = DbConnectionScope.Current)
                {
                    using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                    {
                        TDbObject dbObject = CreateDbObject(businessObject, preserveObjectId);

                        DbSet.Add(dbObject);

                        DbContext.SaveChanges();

                        // Update primary key
                        businessObject.ObjectId = dbObject.ObjectId;

                        UpdateAuditData(businessObject, dbObject, true);

                        transactionScope.Complete();

                    }
                }
            }
            catch (SqlException exception)
            {
                HandleSqlException(exception);
            }
            catch (DbEntityValidationException exception)
            {
                throw exception;
            }
        }

        public void InsertObjects(IEnumerable businessObjects, bool preserveObjectId)
        {
            InsertObjects(businessObjects.Cast<TBusinessObject>(), preserveObjectId);
        }

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public virtual void InsertObjects(IEnumerable<TBusinessObject> businessObjects)
        {
            InsertObjects(businessObjects, false);
        }

        public void InsertObjects(IEnumerable<TBusinessObject> businessObjects, bool preserveObjectId)
        {
            try
            {
                //using (DbConnectionScope connectionScope = DbConnectionScope.Current)
                {
                    using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                    {
                        Dictionary<TBusinessObject, TDbObject> dbObjectsByInsertedBusinessObjects = new Dictionary<TBusinessObject, TDbObject>();

                        foreach (TBusinessObject businessObject in businessObjects)
                        {
                            TDbObject dbObject = CreateDbObject(businessObject, preserveObjectId);
                            businessObject.ObjectId = dbObject.ObjectId;

                            DbSet.Add(dbObject);

                            dbObjectsByInsertedBusinessObjects.Add(businessObject, dbObject);
                        }

                        DbContext.SaveChanges();

                        foreach (KeyValuePair<TBusinessObject, TDbObject> pair in dbObjectsByInsertedBusinessObjects)
                        {
                            UpdateAuditData(pair.Key, pair.Value, true);
                            pair.Key.ObjectId = pair.Value.ObjectId;
                        }

                        transactionScope.Complete();
                    }
                }
            }
            catch (SqlException exception)
            {
                HandleSqlException(exception);
            }
        }

        protected TDbObject CreateDbObject(TBusinessObject businessObject, bool preserveObjectId)
        {
            TDbObject dbObject = new TDbObject();
            //TDbObject dbObject = this.ObjectSet.Create();

            if (preserveObjectId)
            {
                dbObject.ObjectId = businessObject.ObjectId;
            }

            UpdateDbObject(dbObject, businessObject);

            UpdateAuditData(dbObject, true);

            return dbObject;
        }

        private static void HandleSqlException(SqlException exception)
        {
            //if (exception.Number == 547) //statement conflicted with constraint http://msdn.microsoft.com/en-us/library/aa937592(v=SQL.80).aspx
            //    throw new ConstraintConflictedException(CF_BL.ObjectManagerBase_HandleSqlException_Message, exception);
            //else
            throw exception;
        }

        [DataObjectMethod(DataObjectMethodType.Update)]
        public void UpdateObject(TBusinessObject businessObject)
        {
            try
            {
                //using (DbConnectionScope connectionScope = DbConnectionScope.Current)
                {
                    using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                    {
                        TDbObject dbObject = GetDbObject(businessObject.ObjectId);
                        TBusinessObject existingBusinessObject = ConvertToBusinessObject(dbObject);

                        UpdateDbObject(dbObject, businessObject);

                        UpdateAuditData(dbObject, false);

                        DbContext.SaveChanges();

                        UpdateAuditData(businessObject, dbObject, false);

                        transactionScope.Complete();

                    }
                }
            }
            catch (SqlException exception)
            {
                HandleSqlException(exception);
            }
        }

        public void UpdateObjects(IEnumerable businessObjects)
        {
            UpdateObjects(businessObjects.Cast<TBusinessObject>());
        }

        [DataObjectMethod(DataObjectMethodType.Update)]
        public void UpdateObjects(IEnumerable<TBusinessObject> businessObjects)
        {
            try
            {
                //using (DbConnectionScope connectionScope = DbConnectionScope.Current)
                {
                    using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                    {
                        HashSet<string> changedFieldNames = new HashSet<string>();

                        //Key = TBusinessObject, Value = TDbObject 
                        Dictionary<TBusinessObject, TBusinessObject> existingBusinessObjectByBusinessObjectDictionary = new Dictionary<TBusinessObject, TBusinessObject>();
                        Dictionary<TBusinessObject, TDbObject> dbObjectsByBusinessObjectDictionary = new Dictionary<TBusinessObject, TDbObject>();

                        foreach (TBusinessObject businessObject in businessObjects)
                        {
                            TDbObject dbObject = GetDbObject(businessObject.ObjectId);
                            TBusinessObject existingBusinessObject = ConvertToBusinessObject(dbObject);

                            existingBusinessObjectByBusinessObjectDictionary.Add(businessObject, existingBusinessObject);
                            dbObjectsByBusinessObjectDictionary.Add(businessObject, dbObject);

                            UpdateDbObject(dbObject, businessObject);
                            UpdateAuditData(dbObject, false);
                            //TraceOperation(businessObject, BusinessObjectOperationType.Update);

                        }

                        DbContext.SaveChanges();

                        foreach (KeyValuePair<TBusinessObject, TDbObject> pair in dbObjectsByBusinessObjectDictionary)
                        {
                            UpdateAuditData(pair.Key, pair.Value, false);
                        }

                        transactionScope.Complete();
                    }
                }
            }
            catch (SqlException exception)
            {
                HandleSqlException(exception);
            }
            catch (DbEntityValidationException exception)
            {
                throw exception;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public void DeleteObject(TBusinessObject businessObject)
        {
            DeleteObject(businessObject.ObjectId);
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public virtual void DeleteObject(Guid objectId)
        {
            try
            {
                //using (DbConnectionScope connectionScope = DbConnectionScope.Current)
                {
                    using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                    {
                        TDbObject dbObject = GetDbObject(objectId);

                        DeleteObjectCore(dbObject);

                        DbContext.SaveChanges();

                        transactionScope.Complete();

                    }
                }
            }
            catch (SqlException exception)
            {
                HandleSqlException(exception);
            }
        }

        public void InsertObject(IBusinessObject businessObject)
        {
            InsertObject((TBusinessObject)businessObject);
        }

        public void UpdateObject(IBusinessObject businessObject)
        {
            UpdateObject((TBusinessObject)businessObject);
        }

        public IBusinessObject CreateBusinessObject()
        {
            return new TBusinessObject();
        }

        protected virtual void DeleteObjectCore(TDbObject dbObject)
        {
            DbSet.Remove(dbObject);
        }

        public void DeleteObjects(IEnumerable businessObjects)
        {
            DeleteObjects(businessObjects.Cast<TBusinessObject>());
        }

        public void DeleteObjects(IEnumerable<TBusinessObject> businessObjects)
        {
            DeleteObjects(businessObjects.Select(o => o.ObjectId));
        }

        public void DeleteObjects(IEnumerable<Guid> objectIds)
        {
            try
            {
                //using (DbConnectionScope connectionScope = DbConnectionScope.Current)
                {
                    using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                    {
                        IEnumerable<TDbObject> dbObjects = DbSet.Where(o => objectIds.Contains(o.ObjectId));
                        DbSet.RemoveRange(dbObjects);

                        DbContext.SaveChanges();

                        transactionScope.Complete();
                    }
                }
            }
            catch (SqlException exception)
            {
                HandleSqlException(exception);
            }
        }

        protected virtual void DeleteDependentObjects(Guid objectId)
        {
        }

        protected virtual void DeleteOrphans(TBusinessObject businessObject)
        {
        }

        public virtual Type GetBusinessObjectType()
        {
            return typeof(TBusinessObject);
        }

        public virtual Type GetBusinessObjectType(TBusinessObject businessObject)
        {
            if (businessObject == null)
            {
                throw new ArgumentNullException("businessObject");
            }

            return businessObject.GetType();
        }

        #region -------------------------------------------- DisplayName ------------------------------------------

        public virtual string GetDisplayName(TBusinessObject businessObject)
        {
            if (businessObject == null)
            {
                throw new ArgumentNullException("businessObject");
            }

            return businessObject.DisplayName;
        }

        public virtual string GetDisplayName(Guid objectId)
        {
            TBusinessObject businessObject = GetObject(objectId);
            return GetDisplayName(businessObject);
        }

        public virtual Dictionary<Guid, string> GetDisplayNames(IEnumerable<Guid> objectIds)
        {
            if (objectIds == null)
            {
                throw new ArgumentNullException("objectIds");
            }

            List<TBusinessObject> businessObjects = GetObjectsUsingBOPredicate(o => objectIds.Contains(o.ObjectId));

            return businessObjects.ToDictionary(o => o.ObjectId, o => GetDisplayName(o));
        }

        public IQueryable GetObjectsQueryable()
        {
            return DbQueryable;
        }

        #endregion

        public void Dispose()
        {
            if (!disposed)
            {
                if (ownsDbContextInstance)
                {
                    DbContext.Dispose();
                }

                disposed = true;
            }
        }
    }
}
