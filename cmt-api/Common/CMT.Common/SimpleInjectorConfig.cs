using SimpleInjector;
using System;

namespace CMT.Common
{
    public static class SimpleInjectorConfig
    {
        private static Container _instance = null;
        public static event EventHandler<ValueRequestEventArgs<Container>> Configure;

        public static Container GetInstance()
        {
            if (_instance == null)
            {
                if (Configure == null)
                {
                    throw new Exception("Unable to get container configuration.");
                }

                ValueRequestEventArgs<Container> eventArgs = new ValueRequestEventArgs<Container>(string.Empty);
                Configure(null, eventArgs);
                _instance = eventArgs.Value;
                _instance.Verify();
            }

            return _instance;
        }

        public static void DisposeContainer()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.Dispose();
            _instance = null;
        }

        public static T GetServiceInstance<T>() where T : class
        {
            return GetInstance().GetInstance<T>();
        }

        public static object GetServiceInstance(Type serviceType)
        {
            return GetInstance().GetInstance(serviceType);
        }
    }
}
