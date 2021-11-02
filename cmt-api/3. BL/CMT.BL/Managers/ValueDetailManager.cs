using CMT.BL.Core;
using CMT.BO;
using CMT.BO.Admin;
using CMT.DL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CMT.BL
{
    public class ValueDetailManager : BaseObjectManager<CmtEntities, ValueDetail, ValueDetailBO>
    {
        public ValueDetailManager(CmtEntities dbContext) : base(dbContext)
        {
        }

        private ValueDetailBO GetTranslationByCountryCode(Guid valueId, string countryCode)
        {
            IQueryable<ValueDetail> values = (from vd in DbQueryable
                                              where vd.ValueId == valueId && vd.Country.Code == countryCode
                                              select vd);
            return ConvertToBusinessObjects(values).FirstOrDefault();
        }

        public void TranslateValue(TreeElementBO translation, string countryCode)
        {
            Guid countryId = (from c in DbContext.Countries
                              where c.Code == countryCode
                              select c.ObjectId).FirstOrDefault();
            if (countryId == Guid.Empty)
            {
                throw new ArgumentException("Wrong countryCode");
            }

            ValueDetailBO currentValue = GetTranslationByCountryCode(translation.ObjectId, countryCode);
            if (string.IsNullOrEmpty(translation.LocalValue) && string.IsNullOrEmpty(translation.LocalCode))
            {
                DeleteObject(currentValue);
                return;
            }

            Guid? valueListId = (from v in DbContext.Values
                                 where v.ObjectId == translation.ObjectId
                                 select v.ValueListId).Single();

            Guid? currentValueId = (currentValue == null) ? (Guid?)null : currentValue.ValueId;

            List<ValueDetail> query = (from vd in DbContext.ValueDetails
                                       where vd.Value1.ValueListId == valueListId &&
                                             vd.Value == translation.LocalValue &&
                                             (currentValueId == null || vd.ValueId != currentValueId)
                                       select vd).ToList();

            if (query.Any())
            {
                throw new UserFriendlyException("Translation already exists on this level", HttpStatusCode.NotAcceptable);
            }

            if (currentValue == null)
            {

                currentValue = new ValueDetailBO()
                {
                    ValueId = translation.ObjectId,
                    Value = translation.LocalValue ?? string.Empty,
                    LocalCode = translation.LocalCode,
                    CountryId = countryId
                };
                InsertObject(currentValue);
            }
            else
            {
                currentValue.Value = translation.LocalValue;
                currentValue.LocalCode = translation.LocalCode;
                UpdateObject(currentValue);
            }
        }

        public async Task<Dictionary<string, string>> GetValueTranslations(Guid id)
        {
            return await (from c in DbQueryable
                          where c.ValueId == id
                          select new { c.Country.Code, c.Value }).ToDictionaryAsync(k => k.Code, v => v.Value);
        }
    }
}
