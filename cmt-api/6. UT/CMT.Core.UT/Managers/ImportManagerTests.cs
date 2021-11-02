//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using CMT.BL.Managers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BO;
//using CF.Common;
//using System.Diagnostics;
//using CMT.DL.Core;
//using CMT.Core.UT;

//namespace CMT.BL.Managers.Tests
//{
//    [TestClass()]
//    public class ImportManagerTests : BaseTest
//    {
//        public ImportManagerTests()
//        {
//        }
//        [TestMethod()]
//        public void ImportManager_GetObjectsTest()
//        {
//            using (var countryManager = new ImportManager())
//            {
//                var result = countryManager.GetObjects();

//                Assert.IsTrue(result.Any());
//            }
//        }

//        [TestMethod()]
//        public void ImportManager_GetObjectsUsingPerdicateTest()
//        {
//            using (var countryManager = new ImportManager())
//            {
//                //var result = countryManager.GetObjectsUsingBOPredicate(p => p.ColumnSeparator == CMT.Common.ColumnSeparators.Semicolon);

//                Assert.IsTrue(false);
//            }
//        }
//    }
//}