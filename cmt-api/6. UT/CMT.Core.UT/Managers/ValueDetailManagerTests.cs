using CMT.BO;
using CMT.BO.Admin;
using CMT.Common;
using CMT.Core.UT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL.Tests
{
    [TestClass()]
    public class ValueDetailManagerTests : BaseTest
    {
        [TestMethod()]
        public void TranslateValueTest()
        {

            using (ValueDetailManager valueDetailManager = SimpleInjectorConfig.GetServiceInstance<ValueDetailManager>())
            {
                List<CountryBO> countries = SimpleInjectorConfig.GetServiceInstance<CountryManager>().GetObjects();

                List<ValueDetailBO> objects = valueDetailManager.GetObjects();
                if (objects.Count == 0)
                {
                    Assert.Inconclusive();
                    return;
                }

                ValueDetailBO obj = objects.First();
                TreeElementBO translation = new TreeElementBO()
                {
                    ObjectId = obj.ValueId,
                    LocalValue = obj.Value + "test"
                };
                valueDetailManager.TranslateValue(translation, countries.First(p => p.ObjectId == obj.CountryId).Code);
                ValueDetailBO newObj = valueDetailManager.GetObject(obj.ObjectId);
                valueDetailManager.UpdateObject(obj);

                Assert.AreEqual(translation.LocalValue, newObj.Value);
            }
        }

        [TestMethod()]
        public void TranslateValueTest_WrongCountryCode()
        {
            using (ValueDetailManager valueDetailManager = SimpleInjectorConfig.GetServiceInstance<ValueDetailManager>())
            {
                TreeElementBO translation = new TreeElementBO()
                {
                    ObjectId = Guid.NewGuid(),
                    LocalValue = "test"
                };
                try
                {
                    valueDetailManager.TranslateValue(translation, "XYZ");
                    Assert.IsTrue(false, "This should fail as country with this code doesn't exits");
                }
                catch (Exception e)
                {
                    Assert.IsTrue(e is ArgumentException, "Unexpected exception type. Expecting ArgumentException for wrong country code.");
                }

            }
        }

        //[TestMethod()]
        //public void TranslateValueTest_NullValue()
        //{
        //    using (var manager = new ValueDetailManager())
        //    {
        //        var translation = new TreeElementBO()
        //        {
        //            ObjectId = Guid.NewGuid(),
        //            LocalValue = ""
        //        };
        //        try
        //        {
        //            manager.TranslateValue(translation, "US");
        //            Assert.IsTrue(false, "This should fail with null argument exception");
        //        }
        //        catch(Exception e)
        //        {
        //            Assert.IsTrue(e is ArgumentNullException, "Unexpected exception type. Expecting ArgumentNullException.");
        //        }

        //    }
        //}

    }
}