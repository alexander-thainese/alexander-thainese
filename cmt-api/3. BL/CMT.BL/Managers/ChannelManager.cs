using CMT.BL.Core;
using CMT.BO;
using CMT.DL;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL
{
    public class ChannelManager : BaseObjectManager<CmtEntities, Channel, ChannelBO>
    {
        public List<ChannelBO> GetActiveChannels(string countryCode)
        {
            IOrderedQueryable<Channel> query = from o in DbQueryable
                                               where o.MetadataSchema
                                                   .Any(p => p.IsActive
                                                       && p.Countries.Any(c => c.Code == countryCode)
                                                   )
                                               orderby o.Name
                                               select o;

            return ConvertToBusinessObjects(query).ToList();
        }
    }
}
