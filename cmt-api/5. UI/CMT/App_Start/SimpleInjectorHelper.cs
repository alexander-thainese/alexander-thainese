using CMT.BL;
using CMT.Controllers;
using CMT.DL;
using CMT.PV.Security;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.WebApi;
using System.Web.Http;

namespace CMT
{
    public static class SimpleInjectorHelper
    {
        public static Container Configure()
        {
            Container container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            container.Register(() => new CmtEntities(), Lifestyle.Scoped);
            container.Register<UserCountryManager, UserCountryManager>(Lifestyle.Scoped);
            container.Register<ElementManager, ElementManager>(Lifestyle.Scoped);
            container.Register<ElementTypeManager, ElementTypeManager>(Lifestyle.Scoped);
            container.Register<AdminMetadataController, AdminMetadataController>(Lifestyle.Scoped);
            container.Register<ValueManager, ValueManager>(Lifestyle.Scoped);
            container.Register<ValueListLevelManager, ValueListLevelManager>(Lifestyle.Scoped);
            container.Register<ValueListManager, ValueListManager>(Lifestyle.Scoped);
            container.Register<ValueDetailManager, ValueDetailManager>(Lifestyle.Scoped);
            container.Register<ValueTagManager, ValueTagManager>(Lifestyle.Scoped);

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            return container;
        }

    }
}
