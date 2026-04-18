using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Random
{
    public class RandomService : BaseService
    {
        public override Type ServiceType => typeof(RandomService);
        
        protected override Task<bool> OnInit()
        {
            return Task.FromResult(true);
        }
        
        public int Range(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public float Range(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public void Shuffle<T>(List<T> list)
        {
            if (list == null || list.Count <= 1)
                return;
            
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}