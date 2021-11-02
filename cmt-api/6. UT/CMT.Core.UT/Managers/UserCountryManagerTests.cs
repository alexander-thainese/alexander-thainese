using CMT.BO;
using CMT.Core.UT;
using CMT.DL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace CMT.PV.Security.Tests
{
    [TestClass()]
    public class UserCountryManagerTests : BaseTest
    {
        Mock<CmtEntities> db = null;
        IQueryable<UserCountry> UserCountryList = null;
        CmtEntities Context;
        Guid PolandCountryId = Guid.NewGuid();
        Guid UKCountryId = Guid.NewGuid();
        Guid USCountryId = Guid.NewGuid();

        public Guid UserId { get; private set; }

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            UserId = Guid.NewGuid();
            UserCountryList = new List<UserCountry>()
            {
                new UserCountry()
                {
                    ObjectId = Guid.NewGuid(),
                    UserId = UserId,
                    CountryId = UKCountryId,
                    Country = new Country()
                {
                    ObjectId = UKCountryId,
                    Name = "United Kingdom",
                    Code = "UK"
                },

                },
                new UserCountry()
                {
                    ObjectId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    CountryId = USCountryId,
                    Country = new Country()
                {
                    ObjectId = USCountryId,
                    Name = "United States",
                    Code = "US"
                }
                },
                new UserCountry()
                {
                    ObjectId = Guid.NewGuid(),
                    UserId = UserId,
                    CountryId = PolandCountryId,
                    Country = new Country()
                {
                    ObjectId = PolandCountryId,
                    Name = "Poland",
                    Code = "PL"
                }

                }
            }.AsQueryable();


            Mock<DbSet<UserCountry>> userCountryMockSet = new Mock<DbSet<UserCountry>>();
            userCountryMockSet.As<IDbAsyncEnumerable<UserCountry>>().Setup(m => m.GetAsyncEnumerator()).Returns(new TestDbAsyncEnumerator<UserCountry>(UserCountryList.GetEnumerator()));

            userCountryMockSet.As<IQueryable<UserCountry>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UserCountry>(UserCountryList.Provider));

            userCountryMockSet.As<IQueryable<UserCountry>>().Setup(m => m.Expression).Returns(UserCountryList.Expression);
            userCountryMockSet.As<IQueryable<UserCountry>>().Setup(m => m.ElementType).Returns(UserCountryList.ElementType);
            userCountryMockSet.As<IQueryable<UserCountry>>().Setup(m => m.GetEnumerator()).Returns(UserCountryList.GetEnumerator());

            db = new Mock<CmtEntities>();
            db.Setup(_ => _.Set<UserCountry>()).Returns(userCountryMockSet.Object);

            Context = db.Object;

        }

        [TestMethod()]
        public async Task GetUserCountriesTest()
        {
            List<CountryBO> countries = await new UserCountryManager(Context).GetUserCountries(UserId);
            Assert.AreEqual(2, countries.Count);
        }

        [TestMethod()]
        public async Task IsCountryAvailableForUserTest()
        {
            bool isCountryAvailableForUser = await new UserCountryManager(Context).IsCountryAvailableForUser(UserId, "PL");
            Assert.IsTrue(isCountryAvailableForUser);
        }
    }
}