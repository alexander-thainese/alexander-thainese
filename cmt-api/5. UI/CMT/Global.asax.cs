using CMT.BL;
using CMT.BL.DataDistinctor;
using CMT.BL.Workers;
using CMT.BO;
using CMT.Common;
using CMT.DL.Core;
using CMT.Handlers;
using Newtonsoft.Json;
using NLog;
using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace CMT
{
    public class WebApiApplication : HttpApplication
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            LogEventInfo info = new LogEventInfo(LogLevel.Info, GetType().Name, "Application Start. Version: " + System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location));
            info.Properties["DateTime"] = DateTime.Now.ToUniversalTime();
            logger.Log(info);

            DbConnectionFactory.Register(new DbConnectionFactory(), false);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Web.Configuration.Initialize();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
            //new StructureDiscoveryWorker(new DummyLogger()).Start();
            LoadCMTConfig();
            SimpleInjectorConfig.Configure += (sender, eventArgs) => eventArgs.Value = SimpleInjectorHelper.Configure();
            SimpleInjectorConfig.GetInstance();


            if (!ApplicationSettings.DisableWorkers)
            {
                new BrandUploadWorker(new CMTLogger()).Start();
                new AttributeUploadWorker(new CMTLogger()).Start();
                new FileExtractorWorker(new CMTLogger()).Start();
            }

            Error += WebApiApplication_Error;
        }

        protected void Application_End(object sender, EventArgs e)
        {
            LogEventInfo info = new LogEventInfo(LogLevel.Info, GetType().Name, "Application End");
            info.Properties["DateTime"] = DateTime.Now.ToUniversalTime();
            logger.Log(info);
        }

        private void WebApiApplication_Error(object sender, System.EventArgs e)
        {

            HttpServerUtility server = HttpContext.Current.Server;
            Exception lastException = server.GetLastError();
            logger.Error(lastException, "test message");
        }

        private void LoadCMTConfig()
        {
            using (SettingManager mgr = new SettingManager())
            {
                SettingBO setting = mgr.GetObjectsUsingBOPredicate(o => o.Name == "CMTConfig").Single();
                if (setting == null || string.IsNullOrEmpty(setting.Value)) throw new Exception("CMTConfig doesn't exits in Settings.");
                CMTConfig c = JsonConvert.DeserializeObject<CMTConfig>(setting.Value);
                ApplicationSettings.CMTConfig = c;
            }

        }
    }
}
