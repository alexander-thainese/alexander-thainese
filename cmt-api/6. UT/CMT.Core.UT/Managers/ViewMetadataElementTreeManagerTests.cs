using CMT.Core.UT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL.Managers.Tests
{
    [TestClass()]
    public class ViewMetadataElementTreeManagerTests : BaseTest
    {
        [TestMethod()]
        public void GetElementsTreeTest()
        {
            using (ViewMetadataElementTreeManager manager = new ViewMetadataElementTreeManager())
            {
                List<BO.Admin.TreeElementBO> result = manager.GetElementsTree("US");

                Assert.IsTrue(result.Any());
            }
        }
    }
}