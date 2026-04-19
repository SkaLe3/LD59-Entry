using System;
using System.Collections.Generic;
using Core.Player;
using Service.UI;
using Service.UI.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using MathUtils = Math.MathUtils;

namespace Core.Radio
{
    public class RadioTower : MonoBehaviour, IRadioInterface
    {
        [SerializeField] private Transform emitterSlot;
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

        public IRadioInterface SignalSource => _signalSource;
        private IRadioInterface _signalSource;
        private List<IRadioInterface> _signalTargets = new List<IRadioInterface>();
        private Dictionary<IRadioInterface, GameObject> _signalTargetsEmitters = new Dictionary<IRadioInterface, GameObject>();
        private Dictionary<IRadioInterface, ParticleSystem> _signalTargetEmittersParticles = new Dictionary<IRadioInterface, ParticleSystem>();
        private IRadioInterface _signalPlayerTarget;

        // Player part
        private float _signalStrength = 1f;
        private float _currentDistanceToPlayer = 0f;
        private float _signalReachedDistance = 0;

        public bool IsAvailableAsReceiver => _signalSource == null && !isSignalOrigin;
        public bool IsAvailableAsEmitter => isSignalOrigin || _signalSource != null;
        public bool IsReceivingSignal => _signalSource != null;
        public bool IsEmittingToPlayer => _signalPlayerTarget != null;

        private GameObject discoveryEmitter;
        private ParticleSystem discoverySignalParticles;
        
        private HUD gameHUD;
        
        public void Start()
        {
            _signalReachedDistance = signalMinRadius;
            
            gameHUD =  Service.Services.GetService<UIService>().GetWindow<MainWindow>().gameHUD;
            discoveryEmitter = Instantiate(signalEmitterPrefab, emitterSlot);
            discoverySignalParticles = discoveryEmitter.GetComponentInChildren<ParticleSystem>();
            
            var mainModule = discoverySignalParticles.main;
            mainModule.startSpeed = readyStartSpeed;
            mainModule.startSize = readyStartSize;
            mainModule.startLifetime = readyStartLifetime;
            var renderer = discoverySignalParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = readySignalMesh;
        }

        private void OnMobileReceiverLocated(PlayerController mobileReceiver)
        {
            RadioTower mobileReceiverSource = mobileReceiver.GetSource() as RadioTower;
            // If we are hub tower that doent receive any signal, and player doesnt have connection 
            if ((isHubTower &&  !mobileReceiver.IsConnected() && _signalSource == null) || (!isHubTower && !IsAvailableAsEmitter && !mobileReceiver.IsConnected())) return;
            
            mobileReceiver.SetInteractionTower(this);
            if (IsEmittingToPlayer) // Connected to player -> Allow disconnect
            {
                // Disconnect prompt
                gameHUD.ShowDisconnectPrompt();
                PlayerController player =_signalPlayerTarget as PlayerController;
                player.StartConnectionRequest(false);
            }
            else // Allow connect
            {
                // Handles both connecting to player, or from player to another tower
                TurnOnDiscovery(); // Spawn or enable emitter and play
                if (IsAvailableAsReceiver || !mobileReceiver.IsConnected())
                {
                    gameHUD.ShowConnectPrompt();
                    mobileReceiver.StartConnectionRequest(true);
                }
                
            }
     
            if (IsReceivingSignal)
            {
                gameHUD.ShowShutdownPrompt();
                mobileReceiver.AllowTowerShutdown();
            }
        }

        private void OnMobileReceiverLost(PlayerController mobileReceiver)
        {
            gameHUD.HideConnectPrompt();
            gameHUD.HideShutdownPrompt();
            TurnOffDiscovery();

            if (IsEmittingToPlayer)
            {
                mobileReceiver.EndConnectionRequest();
            }

            if (IsReceivingSignal)
            {
                mobileReceiver.DisallowTowerShutdown();
            }
            mobileReceiver.SetInteractionTower(null);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            Debug.LogWarning($"Gameobject name: {other.gameObject.name}");
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

            if (player != null)
            {
                OnMobileReceiverLost(player);
            }
            else
            {
                Debug.LogWarning("[Radio Tower] Lost receiver that is not a player");
            }

        }

        private void TurnOnDiscovery()
        {
            discoverySignalParticles.Play();
        }

        private void TurnOffDiscovery()
        {
            discoverySignalParticles.Stop();
        }

        private void TurnOnEmitter(IRadioInterface target)
        {
            if (!_signalTargetsEmitters.ContainsKey(target))
            {
                GameObject emitter = Instantiate(signalEmitterPrefab, emitterSlot);
                ParticleSystem emitterParticles = emitter.GetComponentInChildren<ParticleSystem>();
                _signalTargetsEmitters.Add(target, emitter);
                _signalTargetEmittersParticles.Add(target, emitterParticles);
            }
            _signalTargetEmittersParticles[target].Play();
            
        }

        private void TurnOffEmitter(IRadioInterface target)
        {
            _signalTargetEmittersParticles[target].Stop();
        }
        
        
        private void Update()
        {
            foreach (IRadioInterface target in _signalTargets)
            {
                float distanceToTarget = 0;
                Vector3 direction = SetSignalParamters(target, ref distanceToTarget);
                OrientAntetaTo(target, direction);
            }

            if (_signalPlayerTarget != null)
            {
                float distanceToTarget = 0;
                Vector3 direction = SetSignalParamters(_signalPlayerTarget, ref distanceToTarget);
                OrientAntetaTo(_signalPlayerTarget, direction);
                
                _currentDistanceToPlayer = distanceToTarget;
                if (_currentDistanceToPlayer > signalMinRadius && _currentDistanceToPlayer > _signalReachedDistance)
                {
                    _signalReachedDistance = MathUtils.FInterpTo(_signalReachedDistance, _currentDistanceToPlayer, Time.deltaTime, signalReachSpeed);
                
                    _signalStrength = 1 - ((_currentDistanceToPlayer - _signalReachedDistance) / signalLoseDistance);
                }
                else
                {
                    _signalReachedDistance = _currentDistanceToPlayer;
                    _signalStrength = 1f;
                }
                
                if (_signalStrength > 0f && !CheckConnectionLine(_signalPlayerTarget))
                {
                    PlayerController player = _signalPlayerTarget as PlayerController;
                    player.NotifySignalStrength(_signalStrength);
                }
                else
                {
                    Disconnect(_signalPlayerTarget, true);
                    NetworkManager networkManager = FindAnyObjectByType<NetworkManager>();
                    networkManager.NotifyConnectionLost();
                }
            }
            
        }

        private Vector3 SetSignalParamters(IRadioInterface target, ref float distance)
        {
            Vector3 sourceLocation = GetAntenaLocation();
            Vector3 targetLocation = target.GetAntenaLocation();
                
            var mainModule = _signalTargetEmittersParticles[target].main;
                
            float signalSpeed = mainModule.startSpeed.constant;
            Vector3 signalDirectionScaled = targetLocation - sourceLocation;
            Vector3 signalDirection = signalDirectionScaled.normalized;
            float distanceToTarget = (signalDirectionScaled).magnitude;
            float travelDistance = distanceToTarget + overshootDistance; 
            float desiredLifetime = travelDistance / signalSpeed;
            mainModule.startLifetime = desiredLifetime;

            distance = distanceToTarget;
            return signalDirection;
        }

        public Vector3 GetAntenaLocation()
        {
            return emitterSlot.transform.position;
        }

        public void OrientAntetaTo(IRadioInterface target, Vector3 direction)
        {
            Vector3 normalizedDirection = direction.normalized;
            _signalTargetsEmitters[target].transform.rotation = Quaternion.LookRotation(normalizedDirection);
        }
        
        public void NotifyConnectionEstablished(IRadioInterface from)
        {
            _signalSource = from;
            TurnOffDiscovery();
        }

        public void NotifyConnectionLost()
        {
            _signalSource = null;
        }
        

        public bool IsHubTower()
        {
            return isHubTower;
        }

        public void Connect(IRadioInterface connectionTarget)
        {
            if (connectionTarget is RadioTower) // Connecting to tower
            {
                RadioTower tower = connectionTarget as RadioTower;
                //if (!isHubTower || tower.isHubTower) // connection between big towers or to hub tower
                {
                    if (!_signalTargets.Contains(connectionTarget))
                    {
                        _signalTargets.Add(connectionTarget);
                    }
                    else
                    {
                        Debug.LogWarning("[Radio Tower] Connect called for tower that is already in the list");
                        return;
                    }
                }
                
            }
            else // Connecting to player
            {
                _signalPlayerTarget = connectionTarget;
            } 
            connectionTarget.NotifyConnectionEstablished(this);
            TurnOffDiscovery();
            TurnOnEmitter(connectionTarget);
            gameHUD.HideConnectPrompt();
        }

        public void Disconnect(IRadioInterface target, bool remote = false)
        {
            if (target is RadioTower)
            {
                _signalTargets.Remove(target);
            }
            else
            {
                _signalPlayerTarget = null;
            }
            target.NotifyConnectionLost();

            TurnOffEmitter(target);
            if (!remote)
                gameHUD.HideConnectPrompt();
        }

        public void Shutdown()
        {
            RadioTower source = _signalSource as RadioTower;
            source.Disconnect(this,true);
            gameHUD.HideShutdownPrompt();
            TurnOffDiscovery();
        }

        public bool CheckConnectionLine(IRadioInterface playerTarget)
        {
            Vector3 targetPos = playerTarget.GetAntenaLocation();
            Vector3 sourcePos = GetAntenaLocation();
            
            int layerMask = LayerMask.GetMask("Obstacle");
            bool hasHit = Physics.Linecast(sourcePos, targetPos, layerMask);

            return hasHit;
        }
    }
}