using CMT.Common;
using System;
using System.Collections.Generic;

namespace CMT.BL.DataDistinctor
{
    public class ProductPackData
    {
        public string GlobalPfizerBrandId { get; set; }
        public string GlobalPfizerBrandName { get; set; }
        public string GlobalCode { get; set; }
        public List<ProductPackCustomAttribute> CustomAttributes { get; }

        public ProductPackData()
        {
            CustomAttributes = new List<ProductPackCustomAttribute>();
        }

        public static ProductPackData Create(object[] row, ProductPackDataConfig config)
        {
            if (config == null) throw new ArgumentException("ProductPackDataConfig is not provided.");

            if (row[config.GlobalPfizerBrandIdColumnIndex] == null
                || row[0].GetType() != typeof(string)
                || string.IsNullOrEmpty((string)row[config.GlobalPfizerBrandIdColumnIndex]))
            {
                return null;
            }

            ProductPackData result = new ProductPackData()
            {
                GlobalPfizerBrandId = (string)row[config.GlobalPfizerBrandIdColumnIndex],
                GlobalPfizerBrandName = (string)row[config.GlobalPfizerBrandNameColumnIndex],
                GlobalCode = (string)row[config.GlobalCodeColumnIndex]
            };

            foreach (ProductPackDataRowConfig rowConfig in config.RowConfigs)
            {
                result.CustomAttributes.Add(new ProductPackCustomAttribute(rowConfig.MetaDataElementId, row[rowConfig.ColumnIndex].ToString()));
            }
            return result;
        }
    }



    public class ProductPackCustomAttribute
    {
        public ProductPackCustomAttribute(Guid metaDataElementId, string value)
        {
            MetaDataElementId = metaDataElementId;
            Value = value;
        }

        public Guid MetaDataElementId { get; set; }
        public string Value { get; set; }
    }
}