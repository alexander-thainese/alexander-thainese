using CMT.BL.Core;
using CMT.BL.S3;
using CMT.BO;
using CMT.DL;
using CMT.DL.Core;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace CMT.BL.Workers
{
    public class FileExtractorWorker : BaseWorker
    {
        private readonly ISystemLogger logger;

        public FileExtractorWorker(ISystemLogger logger)
            : base(logger)
        {
            ThreatStartAtExactTime = true;
            this.logger = logger;
        }

        protected override TimeSpan GetThreadLoopDelay()
        {
            return ApplicationSettings.FileExtractorInterval;
        }

        protected override void ProcessWork()
        {
            try
            {

                Work();
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError(GetType(), ex);
                }
            }
        }

        public static Container Container
        {
            get
            {
                Container container = new Container();

                container.Register(() => new CmtEntities(), Lifestyle.Transient);

                container.Register<ElementManager, ElementManager>(Lifestyle.Transient);
                container.Register<ElementTypeManager, ElementTypeManager>(Lifestyle.Transient);

                container.Register<ValueManager, ValueManager>(Lifestyle.Transient);
                container.Register<ValueListLevelManager, ValueListLevelManager>(Lifestyle.Transient);
                container.Register<ValueListManager, ValueListManager>(Lifestyle.Transient);
                container.Register<ValueDetailManager, ValueDetailManager>(Lifestyle.Transient);
                container.Register<ValueTagManager, ValueTagManager>(Lifestyle.Transient);

                GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
                return container;
            }
        }


        public void Work()
        {
            string directoryPath = ApplicationSettings.FileExtractorLocationPath
                + (ApplicationSettings.FileExtractorLocationPath.EndsWith("\\") ? string.Empty : "\\");
            DateTime now = DateTime.UtcNow;
            Dictionary<string, Stream> result = new Dictionary<string, Stream>();
            logger.LogAction("FileExtractor start processing", GetType());
            //using (DbConnectionScope.Create(ConfigurationManager.ConnectionStrings["CMTEntitiesConnectionString"].ConnectionString))
            {
                using (new DbContextScope<CmtEntities>())
                {
                    DbContextScope<CmtEntities>.Current.Database.CommandTimeout = ApplicationSettings.CommandTimeout;

                    Func<string, string> getFileName = (p => string.Format("{0}/{1:yyyyMMdd_HHmmss}/{2}", p.Split('_')[0], now, p));
                    Func<Dictionary<string, Stream>, Dictionary<string, Stream>, Dictionary<string, Stream>> concatDicts = ((r0, r) => r0.Concat(r).ToDictionary(p => p.Key, p => p.Value));

                    using (SchemaManager man = new BL.SchemaManager())
                    {
                        result = concatDicts(result, man.GetSchemaExtracts(now));
                        result = concatDicts(result, man.GetSchemaElementExtracts(now));
                    }

                    using (ElementManager man = Container.GetInstance<ElementManager>())
                    {
                        result = concatDicts(result, man.GetElementExtracts(now));
                    }



                    using (ValueManager man = Container.GetInstance<ValueManager>())
                    {
                        result = concatDicts(result, man.GetValuesExtracts(now));
                        result = concatDicts(result, man.GetValueDetailExtracts(now));
                        result = concatDicts(result, man.GetProductMappingExtracts(now));
                    }

                    foreach (KeyValuePair<string, Stream> item in result)
                    {
                        logger.LogAction("FileExtractor exporting " + item.Key, GetType());
                        item.Value.Seek(0, SeekOrigin.Begin);
                        if (ApplicationSettings.LoadExtractsToLocationPath)
                        {
                            string subfolderPath = Path.Combine(directoryPath, item.Key.Split('_')[0], now.ToString("yyyyMMdd_HHmmss"));

                            if (!Directory.Exists(subfolderPath))
                            {
                                Directory.CreateDirectory(subfolderPath);
                            }

                            string filePath = Path.Combine(subfolderPath, item.Key);
                            using (FileStream fileStream = File.Create(filePath))
                            {
                                item.Value.CopyTo(fileStream);
                            }
                        }
                        if (ApplicationSettings.LoadExtractsToS3)
                        {
                            S3Helper.SendFileStream(getFileName(item.Key), item.Value, ApplicationSettings.ExtractsUploaderConfiguration);
                        }
                    }
                    logger.LogAction("FileExtractor end processing", GetType());
                }
            }

        }
    }
}

