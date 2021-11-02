using CMT.BL;
using CMT.BO;
using CMT.Common;
using CMT.Core.UT;
using CMT.DL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace CMT.BL.Tests
{
    [TestClass()]
    public class ElementManagerTests : BaseTest
    {
        MetadataElement me;
        Mock<CmtEntities> db = null;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        [TestMethod()]
        public void GetElementDetailsTest()
        {

            using (ElementManager elementManager = SimpleInjectorConfig.GetServiceInstance<ElementManager>())
            {
                List<ElementBO> objects = elementManager.GetObjects();
                if (objects.Count == 0)
                {
                    Assert.Inconclusive();
                    return;
                }
                ElementBO obj = objects.First();
                ExtElementBO objDetails = elementManager.GetElementDetails(obj.ObjectId);
                Assert.AreEqual(obj.Description, objDetails.Description);
            }
        }
        //[TestMethod]
        //public void CreateValueListLevels()
        //{
        //    using (ElementManager elementManager = SimpleInjectorConfig.GetServiceInstance<ElementManager>())
        //    {
        //        ElementBO extElement = new ElementBO();
        //        //extElement.SourceElementId = Guid.NewGuid();
        //        elementManager.CreateValueListLevels(extElement);
        //    }

        //    throw new NotImplementedException();
        //}


        [TestMethod()]
        public void UpdateElementDetailsTest_WrongId()
        {
            using (ElementManager manager = SimpleInjectorConfig.GetServiceInstance<ElementManager>())
            {
                List<ElementBO> objects = manager.GetObjects();
                if (objects.Count == 0)
                {
                    Assert.Inconclusive();
                    return;
                }
                ElementBO obj = objects.First();
                obj.Description += "Test";
                ExtElementBO orgDetails = manager.GetElementDetails(obj.ObjectId);
                try
                {
                    obj.ObjectId = Guid.NewGuid();
                    manager.UpdateLovLabels(obj as ExtElementBO);
                    Assert.Fail();
                }
                catch
                {
                    Assert.IsTrue(true);
                }
            }
        }

        [TestMethod()]
        public void RemoveDependentElementTest()
        {
            Guid elementId = Guid.NewGuid();
            ElementBO element = new ElementBO()
            {
                ObjectId = elementId,
                Attributes = ElementAttributes.Derived
            };
            CmtEntities cmtEntities = SimpleInjectorConfig.GetServiceInstance<CmtEntities>();
            ElementTypeManager elementTypeManager = SimpleInjectorConfig.GetServiceInstance<ElementTypeManager>();
            ValueListManager valueListManager = SimpleInjectorConfig.GetServiceInstance<ValueListManager>();
            ValueListLevelManager valueListLevelManager = SimpleInjectorConfig.GetServiceInstance<ValueListLevelManager>();

            Mock<ElementManager> mockElementManager = new Mock<ElementManager>(cmtEntities, elementTypeManager, valueListManager, valueListLevelManager) { CallBase = true };
            mockElementManager.Setup(_ => _.TryGetObject(elementId, out element)).Returns(true).Verifiable();
            me = new MetadataElement()
            {
                ObjectId = Guid.NewGuid()
            };
            mockElementManager.Protected().Setup<MetadataElement>("GetDbObject", ItExpr.IsAny<Guid>()).Returns(me).Verifiable();
            mockElementManager.Setup(p => p.DeleteObject(elementId, false)).Verifiable();
            ElementManager elementManager = mockElementManager.Object;
            Assert.IsTrue(elementManager.RemoveDerivedElement(elementId));

            mockElementManager.Verify(p => p.DeleteObject(elementId, false), Times.Once);
        }


        [TestMethod()]
        [DataRow(true, true, false)]
        [DataRow(true, false, true)]
        [DataRow(false, true, false)]
        [DataRow(false, false, false)]
        public void CanDeleteDerivedElementTest(bool isDerivedElement, bool existsInSchema, bool result)
        {
            Guid elementId = Guid.NewGuid();
            ElementBO element = new ElementBO()
            {
                ObjectId = elementId,
                Attributes = isDerivedElement ? ElementAttributes.Derived : 0
            };

            MockSchemaElement(existsInSchema ? elementId : Guid.Empty);
            CmtEntities cmtEntities = db.Object;
            ElementTypeManager elementTypeManager = SimpleInjectorConfig.GetServiceInstance<ElementTypeManager>();
            ValueListManager valueListManager = SimpleInjectorConfig.GetServiceInstance<ValueListManager>();
            ValueListLevelManager valueListLevelManager = SimpleInjectorConfig.GetServiceInstance<ValueListLevelManager>();
            Mock<ElementManager> mockElementManager = new Mock<ElementManager>(cmtEntities, elementTypeManager, valueListManager, valueListLevelManager) { CallBase = true };
            mockElementManager.Setup(_ => _.TryGetObject(elementId, out element)).Returns(true).Verifiable();

            ElementManager elementManager = mockElementManager.Object;
            bool canDelete = elementManager.CanDeleteDerivedElement(elementId);
            Assert.AreEqual(result, canDelete);
        }


        private void MockSchemaElement(Guid elementId)
        {
            IQueryable<MetadataSchemaElement> metadataSchemaElementList = new List<MetadataSchemaElement>()
            {
                new MetadataSchemaElement()
                {
                    ElementId = elementId
                }
            }.AsQueryable();


            Mock<DbSet<MetadataSchemaElement>> metadataSchemaElementMockSet = new Mock<DbSet<MetadataSchemaElement>>();
            metadataSchemaElementMockSet.As<IDbAsyncEnumerable<MetadataSchemaElement>>().Setup(m => m.GetAsyncEnumerator()).Returns(new TestDbAsyncEnumerator<MetadataSchemaElement>(metadataSchemaElementList.GetEnumerator()));

            metadataSchemaElementMockSet.As<IQueryable<MetadataSchemaElement>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UserCountry>(metadataSchemaElementList.Provider));

            metadataSchemaElementMockSet.As<IQueryable<MetadataSchemaElement>>().Setup(m => m.Expression).Returns(metadataSchemaElementList.Expression);
            metadataSchemaElementMockSet.As<IQueryable<MetadataSchemaElement>>().Setup(m => m.ElementType).Returns(metadataSchemaElementList.ElementType);
            metadataSchemaElementMockSet.As<IQueryable<MetadataSchemaElement>>().Setup(m => m.GetEnumerator()).Returns(metadataSchemaElementList.GetEnumerator());

            db = new Mock<CmtEntities>();
            db.Setup(_ => _.MetadataSchemaElements).Returns(metadataSchemaElementMockSet.Object);
        }

    }
}