using CMT.BL.Core;
using CMT.BO;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL
{
    public class ValueListManager : BaseObjectManager<CmtEntities, ValueList, ValueListBO>
    {
        public ValueManager ValueManager { get; }

        public ValueListManager(CmtEntities dbContext, ValueManager valueManager) : base(dbContext)
        {
            ValueManager = valueManager;
        }

        public void DeleteObjectsRecursively(Guid valueListId)
        {
            List<ValueBO> values = GetValuesByValueListId(valueListId);
            DeleteValues(values);
            DeleteObject(valueListId);
        }

        private List<ValueBO> GetValuesByValueListId(Guid valueListId)
        {
            return ValueManager.GetObjectsUsingBOPredicate(_ => _.ValueListId == valueListId);
        }

        private void DeleteValues(List<ValueBO> values)
        {
            if (!values.Any())
            {
                return;
            }

            foreach (ValueBO value in values)
            {
                if (value.ChildListId.HasValue)
                {
                    DeleteObjectsRecursively(value.ChildListId.Value);
                }
                ValueManager.DeleteObject(value.ObjectId, false);
            }
        }
    }
}
