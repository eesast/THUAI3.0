using System;
using System.Collections.Generic;

namespace Communication.RestServer.Services
{
    class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static readonly ServiceProvider instance = new ServiceProvider();

        public static T Get<T>() where T : class
        {
            return instance.GetService(typeof(T)) as T;
        }

        public static void Register<T>(T t) where T : class
        {
            instance.services.Add(typeof(T), t);
        }

        public object GetService(Type serviceType)
        {
            return services[serviceType];
        }
    }
}
