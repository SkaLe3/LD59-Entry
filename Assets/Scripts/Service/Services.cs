using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Service
{
    public static class Services
    {
        private static bool _isInitialized = false;
        private static Dictionary<Type, BaseService> _servicesMap;

        public static async Task InitAppWith(List<BaseService> services)
        {
            if (!_isInitialized)
            {
                _servicesMap = new Dictionary<Type, BaseService>();
                foreach (BaseService service in services)
                {
                    _servicesMap.Add(service.ServiceType, service);
                }

                List<Task> initialization = new List<Task>(services.Count);
                foreach (BaseService service in services)
                {
                    var initTask = service.Init();
                    initialization.Add(initTask);
                }

                await Task.WhenAll(initialization);

                _isInitialized = true;
            }
            
#if UNITY_EDITOR
            else
            {
                Debug.LogError("Services already initiated. Your services are not added to the list.");
            }
#endif
        }

        public static T GetSerivce<T>() where T : BaseService
        {
            return (T)_servicesMap[typeof(T)];
        }
    }
}