using CF.Common;
using CMT.DL;
using CMT.DL.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMT.UT.Controllers
{
    [TestClass()]
    public class CampaignControllerTests
    {



        public CampaignControllerTests()
        {
            ScopeDataStore.Register(new ThreadLocalScopeDataStore(), false);
            DbConnectionFactory.Register(new DbConnectionFactory(), false);
            new DbContextScope<CmtEntities>();

        }



        //[TestMethod()]
        //public void CampaignControllerTests_GetByCountryTest()
        //{
        //    var controller = new CampaignController();
        //    var result = controller.GetByCountry("US");

        //    Assert.IsTrue(typeof(IEnumerable<CampaignBO>).IsAssignableFrom(result.GetType()));
        //}

        //[TestMethod()]
        //public void CampaignControllerTests_GetTest()
        //{
        //    var controller = new CampaignController();
        //    var result = controller.Get();

        //    Assert.IsTrue(typeof(IEnumerable<CampaignBO>).IsAssignableFrom(result.GetType()));
        //}


        //[TestMethod()]
        //public void CampaignControllerTests_GetByIdTest()
        //{
        //    var controller = new CampaignController();
        //    List<CampaignBO> schemaList = controller.GetByCountry("US");


        //    var result =controller.GetById(schemaList.First().ObjectId);


        //    Assert.IsTrue(typeof(CampaignBO).IsAssignableFrom(result.GetType()));
        //}
    }
}