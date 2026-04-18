using System.Collections.Generic;
using Service;
using UnityEngine;

namespace Boot
{
    [CreateAssetMenu(fileName = "BootSettings", menuName = "Scriptables/Boot settings", order = 51)]
    public class BootSettings : ScriptableObject
    {
        [SerializeField] private float bootTime;
        public float BootTime => bootTime;

        [SerializeField] private int nextSceneIndex = 0;
        public int NextSceneIndex => nextSceneIndex;

        [SerializeField] private List<BaseService> services;
        public List<BaseService> Services => services;
    }
}