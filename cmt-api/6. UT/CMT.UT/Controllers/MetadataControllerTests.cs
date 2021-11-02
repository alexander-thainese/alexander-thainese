using CF.Common;
using CMT.BL;
using CMT.BO;
using CMT.Common;
using CMT.Controllers;
using CMT.DL;
using CMT.DL.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;

namespace CMT.UT.Controllers
{
    [TestClass()]
    public class MetadataControllerTests
    {
        public MetadataControllerTests()
        {
            ScopeDataStore.Register(new ThreadLocalScopeDataStore(), false);
            DbConnectionFactory.Register(new DbConnectionFactory(), false);
            new DbContextScope<CmtEntities>();
        }

        [TestInitialize]
        public void Setup()
        {
            SimpleInjectorConfig.Configure += SimpleInjectorConfig_Configure;
        }


        [TestMethod()]
        public void MetadataControllerTests_GetByCountryTest()
        {
            Mock<MetadataController> controllerMock = new Mock<MetadataController>(SimpleInjectorConfig.GetServiceInstance<ElementManager>());
            controllerMock.Setup(_ => _.AuthenticatedUserId).Returns(Guid.Empty);
            MetadataController controller = controllerMock.Object;
            List<SchemaBO> result = controller.GetByCountry("US");

            Assert.IsTrue(typeof(IEnumerable<SchemaBO>).IsAssignableFrom(result.GetType()));
        }

        [TestMethod()]
        public void MetadataControllerTests_GetTest()
        {
            Mock<MetadataController> controllerMock = new Mock<MetadataController>(
                SimpleInjectorConfig.GetServiceInstance<ElementManager>());
            controllerMock.Setup(_ => _.AuthenticatedUserId).Returns(Guid.Empty);
            MetadataController controller = controllerMock.Object;
            IEnumerable<SchemaBO> result = controller.Get();

            Assert.IsTrue(typeof(IEnumerable<SchemaBO>).IsAssignableFrom(result.GetType()));
        }

        [TestCleanup]
        public void CleanUp()
        {
            SimpleInjectorConfig.Configure -= SimpleInjectorConfig_Configure;
            SimpleInjectorConfig.DisposeContainer();
        }

        private void SimpleInjectorConfig_Configure(object sender, Common.ValueRequestEventArgs<Container> e)
        {
            Container container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
            container.Register(() => DbContextScope<CmtEntities>.Current, Lifestyle.Singleton);
            container.Register<AdminMetadataController, AdminMetadataController>(Lifestyle.Singleton);
            container.Register<ElementManager, ElementManager>(Lifestyle.Singleton);
            container.Register<ElementTypeManager, ElementTypeManager>(Lifestyle.Singleton);
            container.Register<ValueManager, ValueManager>(Lifestyle.Singleton);
            container.Register<ValueDetailManager, ValueDetailManager>(Lifestyle.Singleton);
            container.Register<ValueListManager, ValueListManager>(Lifestyle.Singleton);
            container.Register<ValueTagManager, ValueTagManager>(Lifestyle.Singleton);
            container.Register<ValueListLevelManager, ValueListLevelManager>(Lifestyle.Singleton);

            e.Value = container;
        }


    }
}