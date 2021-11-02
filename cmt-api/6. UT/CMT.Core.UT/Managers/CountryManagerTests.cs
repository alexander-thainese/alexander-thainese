using CMT.BO;
using CMT.Common;
using CMT.Core.UT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL.Managers.Tests
{
    [TestClass()]
    public class CountryManagerTests : BaseTest
    {
        public CountryManagerTests()
        {
        }

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        [TestMethod()]
        public void CountryManager_GetObjectsTest()
        {
            CountryManager countryManager = SimpleInjectorConfig.GetServiceInstance<CountryManager>();
            List<CountryBO> result = countryManager.GetObjects();
            Assert.IsTrue(result.Any());

        }

        [TestMethod]
        public void CountryManager_InsertObjectTest()
        {
            CountryManager countryManager = SimpleInjectorConfig.GetServiceInstance<CountryManager>();
            CountryBO bo = new CountryBO() { Name = "Testowy", Code = "TST" };
            countryManager.InsertObject(bo);

            Assert.IsTrue(bo.ObjectId != Guid.Empty);
        }

        [TestMethod]
        public void CountryManager_GetObjectByIdTest()
        {
            CountryManager countryManager = SimpleInjectorConfig.GetServiceInstance<CountryManager>();
            CountryBO source = countryManager.GetObjects().First();
            CountryBO dest = countryManager.GetObject(source.ObjectId);

            Assert.AreEqual(source.Name, dest.Name);
        }

        [TestCleanup]
        public override void CleanUp()
        {
            CountryManager countryManager = SimpleInjectorConfig.GetServiceInstance<CountryManager>();

            CountryBO bo = countryManager.GetObjectsUsingBOPredicate(p => p.Name == "Testowy").SingleOrDefault();
            if (bo != null)
            {
                countryManager.DeleteObject(bo);
            }

            SimpleInjectorConfig.DisposeContainer();
            SimpleInjectorConfig.Configure -= SimpleInjectorConfig_Configure;
        }
    }
}