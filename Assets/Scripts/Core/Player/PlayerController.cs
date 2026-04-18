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
        
        #region Unity lifecycle
        
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

        public void NotifyConnectionAquired()
        {
            
        }

        public void NotifyConnectionLost()
        {
            playerHUD.SetSignalStrength(0);
        }
    }
}