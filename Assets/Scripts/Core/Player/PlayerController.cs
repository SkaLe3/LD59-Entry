using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Player
{
    [RequireComponent(typeof(VehicleMovement))]
    public class PlayerController : MonoBehaviour
    {
        private VehicleMovement vehicleMovement;
        private Rigidbody rb;
        
        private Vector2 _inputVector;
        
        #region Unity lifecycle

        private void Start()
        {
            vehicleMovement = GetComponent<VehicleMovement>();
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
    }
}