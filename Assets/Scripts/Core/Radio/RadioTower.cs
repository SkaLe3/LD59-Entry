using System;
using UnityEngine;

namespace Core.Radio
{
    public class RadioTower : MonoBehaviour
    {
        [SerializeField] private GameObject signalEmitter;

        private GameObject _signalTarget;
        private bool _isActive;

        public void Start()
        {
            _isActive = false;
        }

        public void SetTarget(GameObject target)
        {
            _signalTarget = target;
            SetActiveState(_isActive);
        }

        public void SetActiveState(bool active)
        {
            if (_signalTarget == null)
            {
                _isActive = false;
                return;
            }
            _isActive = active;
        }

        private void TurnSignalEmitter()
        {
            
        }
    }
}