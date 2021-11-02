using CF.Common;
using CMT.BL;
using CMT.DL;
using CMT.DL.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Transactions;

namespace CMT.SchemaImporter
{
    class Program
    {
        static void Main()
        {

            DbConnectionFactory.Register(new DbConnectionFactory(), false);
            ScopeDataStore.Register(new ThreadLocalScopeDataStore(), false);
            using (StreamReader r = new StreamReader(ConfigurationManager.AppSettings["FilePath"]))
            {
                string json = r.ReadToEnd();
                List<CrtElement> items = JsonConvert.DeserializeObject<List<CrtElement>>(json);

                List<CrtElement> a = items;
                //DbConnectionScope.Create(ConfigurationManager.ConnectionStrings["CMTEntitiesConnectionString"].ConnectionString);
                new DbContextScope<CmtEntities>();
                DbContextScope<CmtEntities>.Current.Database.CommandTimeout = 900;
                using (SchemaManager man = new SchemaManager())
                {

                    using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                    {

                        MetadataSchema newCrtSChema = new MetadataSchema();
                        newCrtSChema.Name = ConfigurationManager.AppSettings["SchemaName"];
                        newCrtSChema.IsActive = false;
                        newCrtSChema.ChannelId = new Guid("0327BBA8-7335-42F6-A9BA-3E7EFA5CA27B");

                        Country c = man.DbContext.Countries.FirstOrDefault();
                        newCrtSChema.Countries.Add(c);


                        foreach (CrtElement e in items)
                        {
                            MetadataElement me = man.DbContext.MetadataElements.FirstOrDefault(o => o.Name == e.name);
                            if (me == null)
                            {
                                me = new MetadataElement();
                                me.Name = e.name;

                                if (e.field_type == "select")
                                {
                                    me.TypeId = new Guid("BA63AF9F-517A-E611-942A-00155D034947");
                                    ValueList vl = new ValueList();
                                    vl.Name = e.name;
                                    me.ValueList = vl;


                                    ValueListLevel vll = new ValueListLevel();
                                    vll.Name = e.name + " Level 1";
                                    vll.Level = 1;

                                    me.ValueListLevels.Add(vll);
                                    if (e.values != null)
                                    {
                                        foreach (Value v in e.values)
                                        {
                                            CMT.DL.Value nv = new CMT.DL.Value();
                                            nv.TextValue = v.value;
                                            nv.Status = 1;
                                            vl.Values.Add(nv);

                                        }
                                    }



                                }
                                else
                                {
                                    if (e.field_type == "text")
                                    {

                                        me.TypeId = new Guid("662AC1FC-C479-E611-942A-00155D034947");
                                    }
                                    else if (e.field_type == "date")
                                    {

                                        me.TypeId = new Guid("FA0F7595-0496-E611-942A-00155D034947");
                                    }
                                    else if (e.field_type == "number")
                                    {

                                        me.TypeId = new Guid("1400C0B7-0496-E611-942A-00155D034947");
                                    }


                                }
                                me.Status = 1;


                                DataRange dr = new DataRange();
                                dr.MetadataElement = me;
                                dr.ApplicationId = new Guid("E3E8E029-E1BA-E611-84D3-02C5858EF1CD");
                            }


                            //ValueListLevel vll2 = new ValueListLevel();
                            //vll2.Name = e.name + " Level 2";
                            //vll2.Level = 2;

                            //me.ValueListLevels.Add(vll2);

                            //ValueListLevel vll3 = new ValueListLevel();
                            //vll3.Name = e.name + " Level 3";
                            //vll3.Level = 3;

                            //me.ValueListLevels.Add(vll3);
                            MetadataSchemaElement se = new MetadataSchemaElement();
                            se.MetadataElement = me;
                            newCrtSChema.MetadataSchemaElements.Add(se);

                        }



                        DataRange drs = new DataRange();
                        drs.MetadataSchema = newCrtSChema;
                        drs.ApplicationId = new Guid("E3E8E029-E1BA-E611-84D3-02C5858EF1CD");






                        man.DbContext.SaveChanges();
                        transactionScope.Complete();


                    }


                }
            }


        }
    }
}

