using System;
using System.Collections.Generic;

namespace VoxelSandbox.Core
{
    /// <summary>
    /// Simple service locator pattern for dependency injection.
    /// Services are registered during bootstrap and accessed throughout the game.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                Logger.Warning($"ServiceLocator: Service {type.Name} is already registered. Replacing.");
            }

            _services[type] = service;
            Logger.Info($"ServiceLocator: Registered {type.Name}");
        }

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            
            if (_services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            Logger.Error($"ServiceLocator: Service {type.Name} not found!");
            return null;
        }

        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Clear()
        {
            _services.Clear();
            Logger.Info("ServiceLocator: All services cleared");
        }
    }
}
