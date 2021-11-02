using CMT.BL.Core;
using CMT.BO;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL
{
    public class ValueListLevelManager : BaseObjectManager<CmtEntities, ValueListLevel, ValueListLevelBO>
    {
        public ValueListLevelManager(CmtEntities dbContext) : base(dbContext) { }
        public void DeleteObjectsByElementId(Guid elementId)
        {
            List<Guid> objectIds = DbQueryable.Where(p => p.ElementId == elementId).Select(p => p.ObjectId).ToList();
            if (objectIds != null && objectIds.Count > 0)
            {
                DeleteObjects(objectIds);
            }
        }
    }
}
