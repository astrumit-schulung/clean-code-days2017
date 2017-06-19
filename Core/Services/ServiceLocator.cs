using System;
using System.Collections.Generic;
using StructureMap;

namespace Core.Services
{
    public class ServiceLocator
    {
        private readonly IDictionary<Type, object> services = new Dictionary<Type, object>();
        private IContainer diContainer;

        private ServiceLocator()
        {
            diContainer = CreateAutowiringDiContainer();
        }

        private static Container CreateAutowiringDiContainer()
        {
            var container = new Container(new AutoScanner());
            return container;
        }

        public static ServiceLocator Instance { get; } = new ServiceLocator();

        public TService GetService<TService>() where TService : class
        {
            object service;
            if (!services.TryGetValue(typeof(TService), out service))
            {
                service = diContainer.GetInstance<TService>();
            }
            return service as TService;
        }

        public void Register<TService>(TService service)
        {
            services[typeof(TService)] = service;
        }

        private sealed class AutoScanner : Registry
        {
            public AutoScanner()
            {
                Scan(scanner =>
                {
                    scanner.AssembliesAndExecutablesFromApplicationBaseDirectory();
                    scanner.WithDefaultConventions();
                    scanner.LookForRegistries();
                });
            }
        }
    }
}