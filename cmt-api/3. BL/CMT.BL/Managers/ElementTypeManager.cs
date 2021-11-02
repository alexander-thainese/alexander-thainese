using CMT.BL.Core;
using CMT.BO;
using CMT.DL;

namespace CMT.BL
{
    public class ElementTypeManager : BaseObjectManager<CmtEntities, ElementType, ElementTypeBO>
    {
        public ElementTypeManager(CmtEntities dbContext) : base(dbContext)
        {

        }
    }
}
