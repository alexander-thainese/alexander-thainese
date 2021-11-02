using AutoMapper;
using CMT.BL.Core;
using CMT.BL.Helpers;
using CMT.BO.Admin;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL.Managers
{
    public class ViewSchemaTreeManager : BaseObjectManager<CmtEntities, GetSchemaTreeResult, TreeElementBO>
    {
        public ViewSchemaTreeManager()
        {
            SetMapper();
        }

        private void SetMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<GetSchemaTreeResult, TreeElementBO>()
                .ForMember(p => p.DefaultValue, opt => opt.MapFrom(p => new ListValue(p.DefaultValueText, p.DefaultValue)));
            });
            Mapper = config.CreateMapper();

        }
        public List<TreeElementBO> GetSchemaList(string countryCode, string searchString)
        {
            List<TreeElementBO> result = new List<TreeElementBO>();
            var q = from s in DbContext.MetadataSchemata
                    let haschildren = s.MetadataSchemaElements.Any()
                    where s.Countries.Where(c => c.Code == countryCode).Any() && (searchString != null ? s.Name.Contains(searchString) : true)
                    select new TreeElementBO { ObjectId = s.ObjectId, IsActive = s.IsActive, Name = s.Name, Type = 1, HasChildren = haschildren };


            //List<TreeElementBO> objects = ConvertToBusinessObjects(query).ToList();
            return q.ToList();
        }
        public List<TreeElementBO> GetSchemaTree(string countryCode)
        {
            return GetSchemaTree(countryCode, null);
        }
        public List<TreeElementBO> GetSchemaTree(string countryCode, string searchTerm)
        {
            List<TreeElementBO> result = new List<TreeElementBO>();
            IQueryable<GetSchemaTreeResult> query = (from s in DbContext.GetSchemaTree(countryCode)
                                                     select s
                        ).AsQueryable();
            List<TreeElementBO> objects = ConvertToBusinessObjects(query).ToList();
            searchTerm = searchTerm?.ToLowerInvariant() ?? string.Empty;

            result = objects.Where(p => !p.ParentId.HasValue).ToList();
            foreach (TreeElementBO obj in result)
            {
                Guid? schemaId = obj.RootId;
                TreeHelper.CreateTree(obj, objects.Where(p => p.RootId == schemaId).ToList(), searchTerm);
                obj.LocalValues = obj.Children.Sum(p => p.LocalValues);
                obj.AllValues = obj.Children.Sum(p => p.AllValues);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    obj.SearchTermFound = obj.Children.Any(p => p.SearchTermFound) || obj.Name.ToLowerInvariant().Contains(searchTerm);
                }
            }
            return result.Where(p => (p.SearchTermFound || string.IsNullOrEmpty(searchTerm))).OrderBy(p => p.Name).ToList();
        }

        public TreeElementBO GetSchema(Guid objectId)
        {
            return ConvertToBusinessObject((from s in DbQueryable
                                            where s.ObjectId == objectId
                                            select s).Single());
        }

    }
}
