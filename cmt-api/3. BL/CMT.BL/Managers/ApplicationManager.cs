using CMT.BL.Core;
using CMT.BO;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL
{
    public class ApplicationManager : BaseObjectManager<CmtEntities, Application, ApplicationBO>
    {
        public Dictionary<Guid, List<ApplicationBO>> GetApplicationsForChannels()
        {
            Dictionary<Guid, List<ApplicationBO>> result = new Dictionary<Guid, List<ApplicationBO>>();

            var query = (from s in DbContext.Channels
                         select new
                         {
                             ChannelId = s.ObjectId,
                             Applications = s.Applications
                         }).ToList();

            foreach (var item in query)
            {
                result.Add(item.ChannelId, item.Applications.Select(o => ConvertToBusinessObject(o)).ToList());
            }

            return result;
        }
    }



}
