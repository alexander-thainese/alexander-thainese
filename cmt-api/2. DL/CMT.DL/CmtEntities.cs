using System.Data;

namespace CMT.DL
{
    public partial class CmtEntities : ICmtEntities
    {
        public static CmtEntities Create()
        {
            return new CmtEntities();
        }

        //public List<GetValuesAttributeResult> GetValuesAttribute()
        //{
        //    return Database.SqlQuery<GetValuesAttributeResult>("exec dbo.GET_VALUES_ATTRIBUTE").ToList<GetValuesAttributeResult>();
        //}

        //public void DeleteColumnMapping(Guid importId, Guid? schemaElementId, string userName)
        //{
        //    SqlParameter[] sqlParameters = new SqlParameter[]
        //    {
        //        new SqlParameter("@IMPORT_ID", importId),
        //        new SqlParameter("@SCHEMA_ELEMENT_ID", schemaElementId),
        //        new SqlParameter("@USER_NAME", userName)
        //    };
        //    Database.ExecuteSqlCommand("exec DELETE_COLUMN_MAPPING @IMPORT_ID, @SCHEMA_ELEMENT_ID, @USER_NAME", sqlParameters);
        //}

        //public List<GetElementsForCampaignAndImportResult3> GetElementsForCampaignAndImport(Guid campaignId, Guid? importId, Guid schemaId, bool showIndirect,int startIndex,int count)
        //{
        //    throw new NotImplementedException();
        //}

        //public ObjectResult<CloneSchemaResult> CloneSchema(Guid? SourceSchemaId, string Name, Guid ChannelId, string DefinedBy)
        //{
        //    throw new NotImplementedException();
        //}

        //public List<GetSchemaTreeResult> GetSchemaTree(string countryCode)
        //{
        //    SqlParameter countryCodeParameter = new SqlParameter("@COUNTRY_CODE", countryCode); 

        //    return Database.SqlQuery<GetSchemaTreeResult>("exec dbo.CMT_GET_SCHEMA_TREE @COUNTRY_CODE", countryCodeParameter).ToList();
        //}
        protected override void Dispose(bool disposing)
        {
            //if (disposing && Database.Connection != null && Database.Connection.State == ConnectionState.Open)
            //{
            //    Database.Connection.Close();
            //    Database.Connection.Dispose();
            //}
            base.Dispose(disposing);
        }
    }
}
