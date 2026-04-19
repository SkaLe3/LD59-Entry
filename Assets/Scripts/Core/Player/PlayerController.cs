using Core.Radio;
using Service.UI;
using Service.UI.Windows;
using UnityEngine;
using UnityEngine.InputSystem;
using Service;

namespace Core.Player
{
    [RequireComponent(typeof(VehicleMovement))]
    public class PlayerController : MonoBehaviour, IRadioInterface
    {
        [SerializeField] private GameObject antenaObject;
        
        private VehicleMovement vehicleMovement;
        private Rigidbody rb;
        
        private Vector2 _inputVector;
        
        private float _receiveSignalStrength;
        
        private HUD playerHUD;

        private bool _shutdownAllowed;
        private bool _waitingForConnect;
        private bool _waitingForDisconnect;
        private RadioTower _interactionTower;
        private InputAction interact;
        private InputAction shutdown;
        
        private IRadioInterface _connectedTower;

        private bool gameEnded;
        
        #region Unity lifecycle
        
        private void OnEnable()
        {
            interact.performed += OnInteract;
            shutdown.performed += OnShutdown;

            interact.Enable();
            shutdown.Enable();
        }

        private void OnDisable()
        {
            interact.performed -= OnInteract;
            shutdown.performed -= OnShutdown;

            interact.Disable();
            shutdown.Disable();
        }
        private void Awake()
        {
            interact = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");
            shutdown = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/r");
        }
        
        private void Start()
        {
            vehicleMovement = GetComponent<VehicleMovement>();
            playerHUD = Service.Services.GetService<UIService>().GetWindow<MainWindow>().gameHUD;

            NetworkManager manager = FindAnyObjectByType<NetworkManager>();
            manager.OnNetworkConnected += OnNetworkConnected;
        }
        
        private void Update()
        {
            vehicleMovement.AddInput(0, 0);
            vehicleMovement.AddInput(_inputVector.y, _inputVector.x);
        }
        
        #endregion
        
        public void OnMove(InputValue value)
        {
            if (gameEnded) return;
            
            _inputVector = value.Get<Vector2>();
        }

        public Vector3 GetAntenaLocation()
        {
            return antenaObject.transform.position;
        }

        public void OrientAntetaTo(Vector3 location)
        {
            
        }

        public void NotifySignalStrength(float strength)
        {
            _receiveSignalStrength = strength;
            playerHUD.SetSignalStrength(strength);
        }

        public void NotifyConnectionEstablished(IRadioInterface from)
        {
            _connectedTower = from;
        }

        public void NotifyConnectionLost()
        {
            _connectedTower = null;
            playerHUD.SetSignalStrength(0);
        }

        public bool IsConnected()
        {
            return _connectedTower != null;
        }

        public IRadioInterface GetSource()
        {
            return _connectedTower;
        }
        
        public bool IsAvailableAsReceiver()
        {
            return false;
        }

        public bool IsAvailableAsEmitter()
        {
            return false;
        }

        public ETowerType GetTowerType()
        {
            return ETowerType.None;
        }

        public bool IsHubTower()
        {
            return false;
        }

        public void SetType(ETowerType towerType)
        {
            
        }

        public void SetInteractionTower(RadioTower tower)
        {
            _interactionTower = tower;
        }
        public void StartConnectionRequest(bool connect)
        {
            if (connect)
            { 
                _waitingForConnect = true;
            }
            else
            {
                _waitingForDisconnect = true;
            }
        }

        public void EndConnectionRequest()
        {
            _waitingForConnect = false;
            _waitingForDisconnect = false;
            _interactionTower = null;
        }

        public void AllowTowerShutdown()
        {
            _shutdownAllowed = true;
        }

        public void DisallowTowerShutdown()
        {
            _shutdownAllowed = false;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (_interactionTower == null) return;
            
            RadioTower myTower = _connectedTower as RadioTower;
            if (myTower && myTower != _interactionTower)
            {
                myTower.Disconnect(this);
                
                if (myTower.IsHubTower() && !_interactionTower.IsHubTower()) // this is hub and connects to not hub
                {
                    RadioTower sourceTower = myTower.SignalSource as RadioTower;
                    RadioTower targetTower = myTower;

                    while (sourceTower.IsHubTower())
                    {
                        sourceTower.Disconnect(targetTower);
                        targetTower = sourceTower;
                        sourceTower = targetTower.SignalSource as RadioTower;
                    }
                    sourceTower.Disconnect(targetTower);
                    sourceTower.Connect(_interactionTower);
                    return;
                }
                
                myTower.Connect(_interactionTower);
                return;
            }
            
            if (_waitingForConnect)
            {
                _interactionTower.Connect(this);
            }

            if (_waitingForDisconnect)
            {
                _interactionTower.Disconnect(this);
            }
            SetInteractionTower(null);
            EndConnectionRequest();
            DisallowTowerShutdown();
        }

        public void OnShutdown(InputAction.CallbackContext context)
        {
            if (_shutdownAllowed)
            {
                _interactionTower.Shutdown();
            }
        }

        public void OnNetworkConnected()
        {
            gameEnded = true;
        }
    }
}