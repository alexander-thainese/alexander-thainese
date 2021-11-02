using CMT.Core.UT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL.Managers.Tests
{
    [TestClass()]
    public class ViewSchemaTreeManagerTests : BaseTest
    {
        [TestMethod()]
        public void GetSchemaTreeTest()
        {
            using (ViewSchemaTreeManager viewSchemaTreeManager = new ViewSchemaTreeManager())
            {
                List<BO.Admin.TreeElementBO> result = viewSchemaTreeManager.GetSchemaTree("US");

                Assert.IsTrue(result.Any());
            }
        }
    }
}