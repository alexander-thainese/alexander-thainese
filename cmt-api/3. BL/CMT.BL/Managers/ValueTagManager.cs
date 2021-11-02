using AutoMapper;
using CMT.BL.Core;
using CMT.BO;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CMT.BL
{
    public class ValueTagManager : BaseObjectManager<CmtEntities, ValueTag, ValueTagBO>
    {
        public ValueTagManager(CmtEntities dbContext) : base(dbContext)
        {
        }

        public List<ValueTagBO> GetValueTags(Guid valueId)
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ValueTag, ValueTagBO>()

                .ForMember(dest => dest.SchemaTag, opt => opt.MapFrom(src => src.SchemaTag))
                .ForMember(dest => dest.TagElementId, opt => opt.MapFrom(src => src.TagElementId))
                .ForMember(dest => dest.TagValueId, opt => opt.MapFrom(src => src.TagValueId))
                .ForMember(dest => dest.Element, opt => opt.MapFrom(src => src.MetadataElement.Name))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.Country.Code))
                .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value_TagValueId.TextValue));
            });
            Mapper = config.CreateMapper();

            IQueryable<ValueTag> valueTags = from vt in DbQueryable
                                             .Include(p => p.Value_TagValueId)
                                             .Include(p => p.MetadataElement)
                                             .Include(p => p.Country)
                                             where vt.ValueId == valueId
                                             select vt;

            List<ValueTagBO> valueTagBOs = ConvertToBusinessObjects(valueTags).ToList();
            InitializeMapper();
            return valueTagBOs;
        }

        public bool RemoveTagsByValueId(Guid valueId)
        {
            IEnumerable<Guid> valueTags = (from vt in DbQueryable
                                           where vt.TagValueId == valueId
                                           select vt.ObjectId).ToList();
            this.DeleteObjects(valueTags);
            return true;
        }


    }
}
