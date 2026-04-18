using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Service
{
    public abstract class BaseService : MonoBehaviour
    {
        public abstract Type ServiceType { get; }

        public bool isInitialized { get; private set; } = false;

        public async Task Init()
        {
            if (!isInitialized)
            {
                isInitialized = await OnInit();
                if (!isInitialized)
                {
                    throw new Exception($"Service type of {ServiceType} is not initialized");
                }
            }
        }

        protected abstract Task<bool> OnInit();

        protected virtual void OnValidate()
        {
            gameObject.name = GetType().Name;
        }
    }
}