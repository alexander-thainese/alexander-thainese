using CMT.DL;
using CMT.PV.Security;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CMT.BL.DataDistinctor
{
    public class BaseUploader
    {
        public static Container Container
        {
            get
            {
                Container container = new Container();

                container.Register(() => new CmtEntities(), Lifestyle.Transient);
                container.Register<UserCountryManager, UserCountryManager>(Lifestyle.Transient);
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
    }
}
