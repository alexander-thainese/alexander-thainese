using CF.Common;
using CMT.BL;
using CMT.BO;
using CMT.Common;
using CMT.Controllers;
using CMT.DL;
using CMT.DL.Core;
using CMT.Models;
using CMT.UT.DataMockups;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CMT.UT.Controllers
{
    [TestClass()]
    public class AdminMetadataControllerTests
    {
        Mock<CmtEntities> DbContextMock;
        public AdminMetadataControllerTests()
        {
            ScopeDataStore.Register(new ThreadLocalScopeDataStore(), false);
            DbConnectionFactory.Register(new DbConnectionFactory(), false);
        }

        [TestInitialize]
        public void Setup()
        {
            ElementDataMockup mockups = new ElementDataMockup();
            Mock<CmtEntities> dbContextMock = new Mock<CmtEntities>();
            MetadataElementId = Guid.NewGuid();
            mockups.CreateValueListLevels(MetadataElementId);
            mockups.CreateMetadataElements(MetadataElementId);
            LovElementTypeId = mockups.ElementTypes.Single(_ => _.Type == ElementTypeType.Lov).ObjectId;
            NonLovElementTypeId = mockups.ElementTypes.First(_ => _.Type != ElementTypeType.Lov).ObjectId;

            Mock<DbSet<ValueListLevel>> valueListLevelMock = SetupDbSet(mockups.ValueListLevels);
            Mock<DbSet<ElementType>> elementTypeMock = SetupDbSet(mockups.ElementTypes);
            Mock<DbSet<MetadataElement>> elementMock = SetupDbSet(mockups.MetadataElements);
            Mock<DbSet<ValueDetail>> valueDetailsMock = SetupDbSet(mockups.ValueDetails);
            Mock<DbSet<ValueList>> valueListMock = SetupDbSet(mockups.ValueLists);
            Mock<DbSet<Value>> valueMock = SetupDbSet(mockups.Values);


            elementMock.Setup(_ => _.Include("ElementType")).Returns(elementMock.Object);

            dbContextMock.Setup(_ => _.Set<ElementType>()).Returns(elementTypeMock.Object);
            dbContextMock.Setup(_ => _.Set<MetadataElement>()).Returns(elementMock.Object);
            dbContextMock.Setup(_ => _.Set<ValueList>()).Returns(valueListMock.Object);
            dbContextMock.Setup(_ => _.Set<Value>()).Returns(valueMock.Object);
            dbContextMock.Setup(_ => _.Set<ValueDetail>()).Returns(valueDetailsMock.Object);
            dbContextMock.Setup(_ => _.Set<ValueListLevel>()).Returns(valueListLevelMock.Object);

            dbContextMock.Setup(_ => _.Set<ValueListLevel>().RemoveRange(It.IsAny<IEnumerable<ValueListLevel>>()))
                         .Callback<IEnumerable<ValueListLevel>>((data) => RemoveFromStore(data, mockups.ValueListLevels));
            dbContextMock.Setup(_ => _.Set<ValueDetail>().RemoveRange(It.IsAny<IEnumerable<ValueDetail>>()))
                         .Callback<IEnumerable<ValueDetail>>((data) => RemoveFromStore(data, mockups.ValueDetails));
            dbContextMock.Setup(_ => _.Set<Value>().Remove(It.IsAny<Value>()))
                         .Callback<Value>((data) => RemoveFromStore(new List<Value> { data }, mockups.Values));
            dbContextMock.Setup(_ => _.Set<ValueList>().Remove(It.IsAny<ValueList>()))
                         .Callback<ValueList>((data) => RemoveFromStore(new List<ValueList> { data }, mockups.ValueLists));

            dbContextMock.Setup(_ => _.ElementTypes).Returns(elementTypeMock.Object);
            dbContextMock.Setup(_ => _.MetadataElements).Returns(elementMock.Object);
            dbContextMock.Setup(_ => _.ValueLists).Returns(valueListMock.Object);
            dbContextMock.Setup(_ => _.Values).Returns(valueMock.Object);
            dbContextMock.Setup(_ => _.ValueDetails).Returns(valueDetailsMock.Object);
            dbContextMock.Setup(_ => _.ValueListLevels).Returns(valueListLevelMock.Object);



            DbContextMock = dbContextMock;

            AuditDataHelper.CollectAuditData += AuditDataHelper_CollectAuditData;

            SimpleInjectorConfig.Configure += SimpleInjectorConfig_Configure;
        }

        private void SimpleInjectorConfig_Configure(object sender, Common.ValueRequestEventArgs<Container> e)
        {
            Container container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
            container.Register(() => DbContextMock.Object, Lifestyle.Singleton);
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

        private static void RemoveFromStore<T>(IEnumerable<T> data, List<T> collection) where T : class
        {
            int count = data.Count();
            List<T> objectsToRemove = data.ToList();
            for (int i = 0; i < count; i++)
            {
                collection.Remove(objectsToRemove.Skip(i).First());
            }
        }

        private static Mock<DbSet<TObject>> SetupDbSet<TObject>(IEnumerable<TObject> data) where TObject : class
        {
            Mock<DbSet<TObject>> dbSetMock = new Mock<DbSet<TObject>>();
            IQueryable<TObject> dataQueryable = data.AsQueryable();
            dbSetMock.As<IQueryable<TObject>>().Setup(m => m.Provider).Returns(dataQueryable.Provider);
            dbSetMock.As<IQueryable<TObject>>().Setup(m => m.Expression).Returns(dataQueryable.Expression);
            dbSetMock.As<IQueryable<TObject>>().Setup(m => m.ElementType).Returns(dataQueryable.ElementType);
            dbSetMock.As<IQueryable<TObject>>().Setup(m => m.GetEnumerator()).Returns(dataQueryable.GetEnumerator());
            return dbSetMock;
        }

        private void AuditDataHelper_CollectAuditData(object sender, AuditDataEventArgs e)
        {
            e.User = "System";
        }

        private static Guid LovElementTypeId;

        public Guid NonLovElementTypeId { get; private set; }

        private static Guid MetadataElementId;

        [TestMethod()]
        public void UpdateElementTypeTest_InvalidElementIdOrInvalidTypeId_ShouldReturnFalse()
        {
            AdminMetadataController adminMetadataController = SimpleInjectorConfig.GetServiceInstance<AdminMetadataController>();

            adminMetadataController.UpdateElementType(CreateElement(Guid.Empty, Guid.NewGuid())).Should().BeFalse();
            adminMetadataController.UpdateElementType(CreateElement(Guid.NewGuid(), Guid.Empty)).Should().BeFalse();
            adminMetadataController.UpdateElementType(CreateElement(MetadataElementId, Guid.NewGuid())).Should().BeFalse();
            adminMetadataController.UpdateElementType(CreateElement(Guid.NewGuid(), LovElementTypeId)).Should().BeFalse();
            adminMetadataController.UpdateElementType(CreateElement(Guid.NewGuid(), Guid.NewGuid())).Should().BeFalse();

        }

        [TestMethod()]
        public void UpdateElementTypeTest_ValidElementAndType_ShouldReturnTrue()
        {
            AdminMetadataController adminMetadataController = SimpleInjectorConfig.GetServiceInstance<AdminMetadataController>();

            adminMetadataController.UpdateElementType(CreateElement(MetadataElementId, NonLovElementTypeId)).Should().BeTrue();

            DbContextMock.Object.Set<ValueListLevel>().Count().Should().Be(0);
            DbContextMock.Object.Set<ValueDetail>().Count().Should().Be(0);
            DbContextMock.Object.Set<Value>().Count().Should().Be(0);
            DbContextMock.Object.Set<ValueList>().Count().Should().Be(0);
        }

        [TestMethod()]
        public void UpdateElementTypeTest_ValidElementAndSameType_ShouldReturnFalse()
        {
            AdminMetadataController adminMetadataController = SimpleInjectorConfig.GetServiceInstance<AdminMetadataController>();

            adminMetadataController.GetMetadataElementBO(MetadataElementId).TypeId.Should().Be(LovElementTypeId);
            adminMetadataController.UpdateElementType(CreateElement(MetadataElementId, LovElementTypeId)).Should().BeFalse();
            adminMetadataController.GetMetadataElementBO(MetadataElementId).TypeId.Should().Be(LovElementTypeId);

            DbContextMock.Object.Set<ValueListLevel>().Count().Should().BeGreaterThan(0);
            DbContextMock.Object.Set<ValueDetail>().Count().Should().BeGreaterThan(0);
            DbContextMock.Object.Set<Value>().Count().Should().BeGreaterThan(0);
            DbContextMock.Object.Set<ValueList>().Count().Should().BeGreaterThan(0);
        }

        [TestMethod()]
        public void AddValueTagTest_InsertObjects_Called()
        {
            List<ValueTagModel> valueTagModelList = new List<ValueTagModel>();
            Mock<ValueTagManager> valueTagManagerMock = new Mock<ValueTagManager>(null);

            AdminMetadataController adminMetadataController = new AdminMetadataController(null, null, null, null, null, null, valueTagManagerMock.Object);
            bool result = adminMetadataController.AddValueTag(valueTagModelList);
            valueTagManagerMock.Verify(x => x.InsertObjects(It.IsAny<List<ValueTagBO>>()), Times.Once);
            Assert.IsTrue(true);
        }

        [TestMethod()]
        [DataRow(true)]
        [DataRow(false)]
        public void CanDeleteDependentElementTest(bool canDelete)
        {
            Guid elementId = Guid.NewGuid();
            Mock<ElementManager> elementManagerMock = new Mock<ElementManager>(null, null, null, null);
            elementManagerMock.Setup(elementManager => elementManager.CanDeleteDerivedElement(elementId)).Returns(canDelete).Verifiable();

            AdminMetadataController adminMetadataController = new AdminMetadataController(elementManagerMock.Object, null, null, null, null, null, null);
            bool result = adminMetadataController.CanDeleteDependentElement(elementId);
            elementManagerMock.Verify(x => x.CanDeleteDerivedElement(elementId), Times.Once);
            Assert.AreEqual(canDelete, result);
        }

        [TestCleanup]
        public void CleanUp()
        {
            SimpleInjectorConfig.Configure -= SimpleInjectorConfig_Configure;
            SimpleInjectorConfig.DisposeContainer();
        }

        private ElementBO CreateElement(Guid elementId, Guid typeId)
        {
            return new ElementBO()
            {
                ObjectId = elementId,
                TypeId = typeId
            };
        }

    }
}