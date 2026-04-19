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

        private bool _isConnected;
        private IRadioInterface _connectedTower;
        
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
        }
        
        private void Update()
        {
            vehicleMovement.AddInput(0, 0);
            vehicleMovement.AddInput(_inputVector.y, _inputVector.x);
        }
        
        #endregion
        
        public void OnMove(InputValue value)
        {
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

        public void NotifyConnectionAquired(IRadioInterface from)
        {
            _isConnected = true;
            _connectedTower = from;
        }

        public void NotifyConnectionLost()
        {
            playerHUD.SetSignalStrength(0);
            _isConnected = false;
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

        public void StartConnectionRequest(bool connect, RadioTower tower)
        {
            if (connect)
            { 
                _waitingForConnect = true;
            }
            else
            {
                _waitingForDisconnect = true;
            }

            _interactionTower = tower;
        }

        public void EndConnectionRequest()
        {
            _waitingForConnect = false;
            _waitingForDisconnect = false;
            _interactionTower = null;
        }

        public void AllowTowerShutdown(RadioTower shutdownTower)
        {
            _interactionTower = shutdownTower;
            _shutdownAllowed = true;
        }

        public void DisallowTowerShutdown()
        {
            _shutdownAllowed = false;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            RadioTower myTower = _connectedTower as RadioTower;
            if (_isConnected &&  myTower != _interactionTower)
            {
                myTower.Disconnect();
                
                myTower.Connect(_interactionTower);
                return;
            }
            
            if (_waitingForConnect)
            {
                _interactionTower.Connect(this);
            }

            if (_waitingForDisconnect)
            {
                _interactionTower.Disconnect();
            }
        }

        public void OnShutdown(InputAction.CallbackContext context)
        {
            if (_shutdownAllowed)
            {
                _interactionTower.Shutdown();
            }
        }
    }
}