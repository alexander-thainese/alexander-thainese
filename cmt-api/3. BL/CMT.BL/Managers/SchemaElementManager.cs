using CMT.BL.Core;
using CMT.BO;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL
{
    public class SchemaElementManager : BaseObjectManager<CmtEntities, MetadataSchemaElement, SchemaElementBO>
    {


        public string GetElementComplexName(Guid objectId, byte level)
        {
            var query = (from s in DbContext.MetadataSchemaElements
                         join e in DbContext.MetadataElements on s.ElementId equals e.ObjectId
                         from vll in DbContext.ValueListLevels.Where(o => o.ElementId == e.ObjectId).DefaultIfEmpty()
                         where s.ObjectId == objectId
                         select new
                         {
                             Name = e.Name,
                             Level = vll != null ? vll.Level : -1,
                             LevelName = vll != null ? vll.Name : null,
                             FullName = vll != null ? e.Name + " - " + vll.Name : e.Name
                         }).ToList();

            var item = query.Single(o => o.Level == level);

            return query.Count == 1 ? item.Name : item.FullName;
        }

        public void UpdateSelectedSchemaElements(Guid schemaId, List<Guid> elementIds)
        {
            List<MetadataSchemaElement> schemaItems = (from se in DbQueryable
                                                       where se.SchemaId == schemaId
                                                       select se).ToList();

            foreach (MetadataSchemaElement i in schemaItems.Where(se => !elementIds.Contains(se.ElementId)))
            {
                DbQueryable.Remove(i);
            }

            foreach (Guid i in elementIds.Where(id => !schemaItems.Any(si => si.ElementId == id)))
            {
                MetadataSchemaElement obj = new MetadataSchemaElement()
                {
                    SchemaId = schemaId,
                    ElementId = i
                };
                DbQueryable.Add(obj);
            }
            DbContext.SaveChanges();
        }

        public Guid GetElementId(Guid schemaElementId)
        {
            Guid retvalue = (from se in DbQueryable
                             where se.ObjectId == schemaElementId
                             select se.ElementId).SingleOrDefault();

            return retvalue;

        }
    }
}
