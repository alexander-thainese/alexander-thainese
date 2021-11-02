using CMT.BL.Core;
using CMT.DL;
using System;
using System.Threading.Tasks;

namespace CMT.PV.Security
{
    public class UserManager : BaseObjectManager<CmtEntities, User, UserBO>
    {
        internal Task<UserBO> GetObjectByIdAsync(Guid id)
        {
            return Task.Run(() => GetObject(id));
        }
    }
}
