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
    public class ViewMetadataElementTreeManager : BaseObjectManager<CmtEntities, ViewMetadataElementTree, TreeElementBO>
    {
        public ViewMetadataElementTreeManager()
        {
            SetMapper();
        }

        private void SetMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ViewMetadataElementTree, TreeElementBO>()
                .ForMember(p => p.DefaultValue, opt => opt.MapFrom(p => new ListValue(p.DefaultValueText, p.DefaultValue)));
            });
            Mapper = config.CreateMapper();

        }

        public List<TreeElementBO> GetElementsTree(string countryCode)
        {
            return GetElementsTree(countryCode, null);
        }

        public List<TreeElementBO> GetElementsTree(string countryCode, string searchTerm)
        {
            List<TreeElementBO> result = new List<TreeElementBO>();
            IOrderedQueryable<ViewMetadataElementTree> query = (from s in DbQueryable
                                                                where string.IsNullOrEmpty(s.CountryCode) || s.CountryCode == countryCode
                                                                select s
                        ).OrderBy(p => p.Name);
            List<TreeElementBO> objects = ConvertToBusinessObjects(query).ToList();
            searchTerm = searchTerm?.ToLowerInvariant() ?? string.Empty;

            foreach (TreeElementBO obj in objects.Where(p => !p.ParentId.HasValue))
            {
                Guid rootId = obj.ObjectId;
                TreeHelper.CreateTree(obj, objects.Where(p => p.RootId == rootId && p.ParentId.HasValue).ToList(), searchTerm);
                obj.LocalValues = obj.Children.Sum(p => p.LocalValues);
                obj.AllValues = obj.Children.Sum(p => p.AllValues);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    obj.SearchTermFound = obj.Children.Any(p => p.SearchTermFound) || obj.Name.ToLowerInvariant().Contains(searchTerm);
                }
            }

            return objects.Where(p => !p.ParentId.HasValue && (p.SearchTermFound || string.IsNullOrEmpty(searchTerm))).ToList();
        }
    }
}
