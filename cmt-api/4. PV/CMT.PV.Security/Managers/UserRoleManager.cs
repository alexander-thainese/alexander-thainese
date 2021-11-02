using CMT.BL.Core;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CMT.PV.Security
{
    public class UserRoleManager : BaseObjectManager<CmtEntities, UserRole, UserRoleBO>
    {
        internal Task<List<string>> GetUserRoles(Guid id)
        {
            Task<List<string>> result = (from ur in DbQueryable
                                         where ur.UserId == id
                                         select ur.Role.Name).ToListAsync();
            return result;
        }

        internal Task<bool> IsUserInRole(Guid id, string roleName)
        {
            Task<bool> result = (from ur in DbQueryable
                                 where ur.UserId == id
                                 && ur.Role.Name == roleName
                                 select 1).AnyAsync();
            return result;
        }
    }
}
