using System;
using Math;
using UnityEngine;

namespace Core.Radio
{
    public class RadioTower : MonoBehaviour, IRadioInterface
    {
        [SerializeField] private GameObject signalEmitter;
        [SerializeField] private ParticleSystem signalParticles;
        [SerializeField] private float overshootDistance = 0.7f;
        [SerializeField] private float signalMinRadius = 3f;
        [SerializeField] private float signalMaxRadius = 15f;
        [SerializeField] private float signalReachSpeed = 2.5f;
        [SerializeField] private float signalLoseDistance = 4f;

        [SerializeField] private AnimationCurve signalLoseCurveOverFarArea;
        
        private IRadioInterface _signalTarget;
        private bool _isActive;

        private float _signalStrength = 1f;
        private float _currentTargetDistance = 0f;
        private float _signalReachedDistance = 0;
        private float _signalVelocity;

        private float _receiveSignalStrength;

        public void Start()
        {
            _isActive = false;
        }

        public void SetTarget(IRadioInterface target)
        {
            _signalTarget = target;
            SetActiveState(_isActive);
        }

        public void SetActiveState(bool active)
        {
            if (_signalTarget == null)
            {
                _isActive = false;
            }
            else
            {
                _isActive = active;
            }
            EvaluateActiveState();
        }

        private void EvaluateActiveState()
        {
            if (_isActive)
            {
                signalParticles.Play();
                _signalTarget.NotifyConnectionAquired();
            }
            else
            {
                signalParticles.Stop();
                _signalTarget.NotifyConnectionLost();
            }
        }

        private void Update()
        {
            if (_isActive &&  _signalTarget != null)
            {
                Vector3 sourceLocation = GetAntenaLocation();
                Vector3 targetLocation = _signalTarget.GetAntenaLocation();

                var mainModule = signalParticles.main;
                
                float signalSpeed = mainModule.startSpeed.constant;
                Vector3 signalDirectionScaled = targetLocation - sourceLocation;
                Vector3 signalDirection = signalDirectionScaled.normalized;
                _currentTargetDistance = (signalDirectionScaled).magnitude;
                float travelDistance = _currentTargetDistance + overshootDistance;
                float desiredLifetime = travelDistance / signalSpeed;

                mainModule.startLifetime = desiredLifetime;
                
                OrientAntetaTo(signalDirection);

                
                if (_currentTargetDistance > signalMinRadius && _currentTargetDistance > _signalReachedDistance)
                {
                    _signalReachedDistance = MathUtils.FInterpTo(_signalReachedDistance, _currentTargetDistance, Time.deltaTime, signalReachSpeed);

                    _signalStrength = 1 - ((_currentTargetDistance - _signalReachedDistance) / signalLoseDistance);
                }
                else
                {
                    _signalReachedDistance = _currentTargetDistance;
                    _signalStrength = 1f;
                }

                if (_signalStrength > 0f)
                {
                    _signalTarget.NotifySignalStrength(_signalStrength);
                }
                else
                {
                    SetActiveState(false);
                }
                
            }
        }

        public Vector3 GetAntenaLocation()
        {
            return signalEmitter.transform.position;
        }

        public void OrientAntetaTo(Vector3 direction)
        {
            Vector3 normalizedDirection = direction.normalized;
            
            signalEmitter.transform.rotation = Quaternion.LookRotation(normalizedDirection);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            IRadioInterface mediator = other.gameObject.GetComponent<IRadioInterface>();

            if (mediator != null)
            {
                SetTarget(mediator);
                SetActiveState(true);
            }
        }
        
        public void NotifySignalStrength(float strength)
        {
            _receiveSignalStrength = strength;
        }

        public void NotifyConnectionAquired()
        {
            
        }

        public void NotifyConnectionLost()
        {
            
        }
    }
}