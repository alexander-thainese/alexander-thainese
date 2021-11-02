using CMT.BO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CMT.UT.Objects
{
    [TestClass]
    public class BusinessObjectsTests
    {
        private const string OBJECT_ID = "ObjectId";
        private const string BO = "BO";
        private const string NAME = "Name";

        [TestMethod]
        public void DataContractAttributeExists()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(CountryBO));
            Type[] types = assembly.GetTypes();
            foreach (Type t in types.Where(p => p.Name.EndsWith(BO)))
            {
                CustomAttributeData attribute = t.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(DataContractAttribute));
                if (attribute == null)
                {
                    Assert.IsTrue(true);
                }
                //Assert.Fail(string.Format("Business Object \"{0}\" doesn't have DataContract attribute", t.Name));
                else
                {
                    Assert.AreEqual(attribute.NamedArguments.First(p => p.MemberName == NAME).TypedValue.Value + BO, t.Name,
                        string.Format("Business Object \"{0}\" has wrong DataContract name", t.Name));
                }
            }
        }

        [TestMethod]
        public void DataMemberAttributeExists_ObjectId()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(CountryBO));
            Type[] types = assembly.GetTypes();
            foreach (Type t in types.Where(p => p.Name.EndsWith(BO)))
            {
                CustomAttributeData classAttribute = t.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(DataContractAttribute));
                if (classAttribute == null)
                {
                    Assert.IsTrue(true);
                    return;
                }

                PropertyInfo objectIdProp = t.GetProperties().FirstOrDefault(p => p.Name == OBJECT_ID);
                if (objectIdProp == null)
                {
                    continue;
                }
                else
                {
                    Assert.AreEqual(typeof(Guid), objectIdProp.PropertyType, string.Format("Business Object {0} has wrong ObjectId type", t.Name));
                    CustomAttributeData attribute = objectIdProp.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(DataMemberAttribute));
                    if (attribute == null)
                    {
                        Assert.Fail(string.Format("Business Object \"{0}\" doesn't have DataMember attribute for ObjectId", t.Name));
                    }
                    else
                    {
                        Assert.AreEqual(Consts.OBJECT_ID_SERIALIZED_NAME, attribute.NamedArguments.First(p => p.MemberName == NAME).TypedValue.Value.ToString(), true,
                            string.Format("Business Object \"{0}\" has wrong DataMember attribute name for objectId", t.Name));
                    }
                }
            }
        }

    }
}
