using System;
using System.Collections.Generic;

namespace CMT.Common
{
    public class CMTConfig
    {
        public List<AWSConfigurationItem> AWSConfigurationItems { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public ProductPackDataConfig ProductPackConfig { get; set; }

        public CMTConfig()
        {
            ProductPackConfig = new ProductPackDataConfig();
            AWSConfigurationItems = new List<AWSConfigurationItem>();
        }
    }

    public class ProductPackDataConfig
    {
        public List<ProductPackDataRowConfig> RowConfigs { get; set; }
        public int GlobalPfizerBrandIdColumnIndex { get; set; }
        public int GlobalPfizerBrandNameColumnIndex { get; set; }
        public int GlobalCodeColumnIndex { get; set; }
        public string SearchPatternTemplate { get; set; }
    }

    public class ProductPackDataRowConfig
    {
        public ProductPackDataRowConfig(Guid metaDataElementId, int columnIndex)
        {
            MetaDataElementId = metaDataElementId;
            ColumnIndex = columnIndex;
        }

        public Guid MetaDataElementId { get; set; }
        public int ColumnIndex { get; set; }
    }

    public class AWSConfigurationItem
    {
        public string Name { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string BucketName { get; set; }
        public string Directory { get; set; }
    }
}
