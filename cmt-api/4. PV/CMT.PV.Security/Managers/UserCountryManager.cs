using CMT.BL.Core;
using CMT.BO;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CMT.PV.Security
{
    public class UserCountryManager : BaseObjectManager<CmtEntities, UserCountry, UserCountryBO>
    {
        public UserCountryManager(CmtEntities context) : base(context)
        {
        }
        public async Task<List<CountryBO>> GetUserCountries(Guid userId)
        {
            return await (from uc in DbQueryable
                          where uc.UserId == userId
                          orderby uc.Country.Name
                          select new CountryBO { ObjectId = uc.ObjectId, Code = uc.Country.Code, Name = uc.Country.Name }).ToListAsync();
        }

        public Task<bool> IsCountryAvailableForUser(Guid userId, string countryCode)
        {
            Task<bool> result = (from ur in DbQueryable
                                 where ur.UserId == userId
                                 && ur.Country != null && ur.Country.Code == countryCode
                                 select 1).AnyAsync();
            return result;
        }
    }
}
