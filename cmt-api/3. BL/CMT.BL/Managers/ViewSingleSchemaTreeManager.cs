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
    public class ViewSingleSchemaTreeManager : BaseObjectManager<CmtEntities, GetSingleSchemaTreeResult, TreeElementBO>
    {
        public ViewSingleSchemaTreeManager()
        {
            SetMapper();
        }

        private void SetMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<GetSingleSchemaTreeResult, TreeElementBO>()
                .ForMember(p => p.DefaultValue, opt => opt.MapFrom(p => Convert(p)));
            });
            Mapper = config.CreateMapper();

        }

        private object Convert(GetSingleSchemaTreeResult p)
        {
            return p.DefaultValueText == null && p.DefaultValue == null ? null : new ListValue(p.DefaultValueText, p.DefaultValue);
        }

        public List<TreeElementBO> GetSchemaTree(string countryCode, Guid schema)
        {
            List<TreeElementBO> result = new List<TreeElementBO>();
            IQueryable<GetSingleSchemaTreeResult> query = (from s in DbContext.GetSingleSchemaTree(countryCode, schema)
                                                           select s).AsQueryable();
            List<TreeElementBO> objects = ConvertToBusinessObjects(query).ToList();

            result = objects.Where(p => !p.ParentId.HasValue).ToList();
            foreach (TreeElementBO obj in result)
            {
                Guid? schemaId = obj.RootId;
                TreeHelper.CreateTree(obj, objects.Where(p => p.RootId == schemaId).ToList(), null);
                obj.LocalValues = obj.Children.Sum(p => p.LocalValues);
                obj.AllValues = obj.Children.Sum(p => p.AllValues);
            }
            return result.OrderBy(p => p.Name).ToList();
        }
    }
}
