using System;
using System.Collections.Generic;
using Core.Player;
using Math;
using Service.UI;
using Service.UI.Windows;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace Core.Radio
{
    public class RadioTower : MonoBehaviour, IRadioInterface
    {
        [SerializeField] private GameObject signalEmitterPrefab;
        [SerializeField] private float overshootDistance = 0.7f;
        [SerializeField] private float signalMinRadius = 3f;
        [SerializeField] private float signalMaxRadius = 15f;
        [SerializeField] private float signalReachSpeed = 2.5f;
        [SerializeField] private float signalLoseDistance = 4f;
        [SerializeField] private bool isHubTower;

        [SerializeField] private AnimationCurve signalLoseCurveOverFarArea;
        
        [Header("Visuals")]
        [SerializeField] private Mesh readySignalMesh;
        [SerializeField] private Mesh normalSignalMesh;
        [SerializeField] private float normalStartSpeed = 7f;
        [SerializeField] private float readyStartSpeed = 0f;
        [SerializeField] private float normalStartSize = 0.3f;
        [SerializeField] private float readyStartSize = 1f;
        [SerializeField] private float readyStartLifetime = 0.6f;

        public bool isSignalOrigin;
        
        private IRadioInterface _signalSource;
        private List<IRadioInterface> _signalTargets = new List<IRadioInterface>();
        private IRadioInterface _signalPlayerTarget;

        // Player part
        private float _signalStrength = 1f;
        private float _currentDistanceToPlayer = 0f;
        private float _signalReachedDistance = 0;

        public bool IsAvailableAsReceiver => _signalSource == null;
        public bool IsAvailableAsEmitter => isSignalOrigin || _signalSource != null;
        public bool IsReceivingSignal => _signalSource != null;
        public bool IsEmittingToPlayer => _signalPlayerTarget != null;

        private GameObject discroveryEmitter;
        private ParticleSystem discroverySignalParticles;
        
        private HUD gameHUD;
        
        public void Start()
        {
            _signalReachedDistance = signalMinRadius;
            
            gameHUD =  Service.Services.GetService<UIService>().GetWindow<MainWindow>().gameHUD;
            discroveryEmitter = Instantiate(signalEmitterPrefab, transform);
        }

        private void OnMobileReceiverLocated(PlayerController mobileReceiver)
        {
            if (IsEmittingToPlayer) // Allow disconnect
            {
                // Disconnect prompt
                gameHUD.ShowDisconnectPrompt();
                PlayerController player =_signalPlayerTarget as PlayerController;
                player.StartConnectionRequest(false, this);
            }
            else // Allow connect
            {
                if (IsAvailableAsEmitter) // This tower is origin, or receives signal from someone
                {
                    TurnOnDiscovery(); // Spawn or enable emitter and play
                    
                }
            }
            //Shutdown
            
                if (!_isActive) // Does not send signal anywhere
                {
                    if (IsAvailableAsEmitter()) // Can send signal
                    {
                        UpdateSignalVisuals(false);
                        signalParticles.Play();
                        gameHUD.ShowConnectPrompt();
                        player.StartConnectionRequest(true, this);
                    }
                }
                else // Sends signal somewhere
                {
                    
                    if (_signalTarget == mediator)
                    {
                        gameHUD.ShowDisconnectPrompt();
                        player.StartConnectionRequest(false, this);
                    }
                }
                if (_isReceivingSignal)
                {
                    gameHUD.ShowShutdownPrompt();
                    player.AllowTowerShutdown(this);
                }
            
        }
        
        private void OnTriggerEnter(Collider other)
        {
            IRadioInterface mediator = other.gameObject.GetComponent<IRadioInterface>();
            PlayerController player = mediator as PlayerController;

            if (player != null)
            {
                OnMobileReceiverLocated(player);
            }
            else
            {
                Debug.LogWarning("[Radio Tower] Located receiver that is not a player");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IRadioInterface mediator = other.gameObject.GetComponent<IRadioInterface>();
            PlayerController player = mediator as PlayerController;
            if (mediator != null && player != null)
            {
                if (!_isActive)
                {
                    signalParticles.Stop();
                    gameHUD.HideConnectPrompt();
                    player.EndConnectionRequest();
                }
                gameHUD.HideShutdownPrompt();
                
                if (_isReceivingSignal)
                {
                    player.DisallowTowerShutdown();
                }
            }
        }
        

        public void SetTarget(IRadioInterface target)
        {
            if (target is RadioTower)
            {
                if (!_signalTargets.Contains(target))
                {
                    _signalTargets.Add(target);
                }
                else
                {
                    Debug.LogError("SetTarget with already added target");
                }
            }
            else
            {
                _signalPlayerTarget =  target;
                SetActiveState(_isActive);
            }
            
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
                UpdateSignalVisuals(true);
                _signalTarget.NotifyConnectionAquired(this);
            }
            else
            {
                signalParticles.Stop();
                _signalTarget.NotifyConnectionLost();
            }
        }

        private void UpdateSignalVisuals(bool transmitSignal)
        {
            var mainModule = signalParticles.main;
            if (transmitSignal)
            {
                mainModule.startSpeed = normalStartSpeed;
                mainModule.startSize = normalStartSize;
                var renderer = signalParticles.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Mesh;
                renderer.mesh = normalSignalMesh;
            }
            else
            {
                mainModule.startSpeed = readyStartSpeed;
                mainModule.startSize = readyStartSize;
                mainModule.startLifetime = readyStartLifetime;
                var renderer = signalParticles.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Mesh;
                renderer.mesh = readySignalMesh;
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
                //Debug.Log($"signal strength: {_signalStrength}");
                Debug.Log($"is active: {_isActive}");

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
        
        
        
        
        public void NotifySignalStrength(float strength)
        {
            _receiveSignalStrength = strength;
        }

        public void NotifyConnectionAquired(IRadioInterface from)
        {
            signalParticles.Stop();
            
            _signalSource = from;
            _isReceivingSignal = true;
        }

        public void NotifyConnectionLost()
        {
            _signalSource = null;
            _isReceivingSignal = false;
        }

        public bool IsAvailableAsReceiver()
        {
            return _towerType == ETowerType.Receiver;
        }

        public bool IsAvailableAsEmitter()
        {
            return _towerType == ETowerType.Emitter || _isEmitterOverride;
        }

        public ETowerType GetTowerType()
        {
            return _towerType;
        }

        public bool IsHubTower()
        {
            return isHubTower;
        }

        public void SetType(ETowerType towerType)
        {
            _towerType = towerType;
        }

        public void Connect(IRadioInterface connectionTarget)
        {
            SetTarget(connectionTarget);
            SetActiveState(true);
            gameHUD.HideConnectPrompt();

            if (!isHubTower)
            {
                SetType(ETowerType.Emitter);
            }
        }

        public void Disconnect(bool remote = false)
        {
            SetActiveState(false);
            if (!remote)
                gameHUD.HideConnectPrompt();
        }

        public void Shutdown()
        {
            RadioTower source = _signalSource as RadioTower;
            source.Disconnect(true);
            gameHUD.HideShutdownPrompt();

        }
    }
}