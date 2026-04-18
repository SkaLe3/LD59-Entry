using System;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleMovement : MonoBehaviour
    {
        [Header("Vehicle Settings")]
        [SerializeField] public float accelerationForce = 15f;
        [SerializeField] private float maxSpeed = 15f;
        [SerializeField] private float maxTraction = 1f;
        [SerializeField] private float idleTraction = 3f;
        [SerializeField] private float turnSpeed = 50f;
        [SerializeField] private float driftTreshold = 0.45f;
        [SerializeField] private float forwardTiltAmount = 3.3f;
        [SerializeField] private float springStiffness = 300f;
        [SerializeField] private float damperCoefficient = 15f;
        [SerializeField] private float sidewaysTiltAmount = 10f;
        [SerializeField] private float minSideSkidVelocity = 2f;
        
        [Header("References")] 
        [SerializeField] private Rigidbody rb;
        [SerializeField] private AudioSource engineSoundSource;
        [SerializeField] private AudioSource skidClip;
        [SerializeField] private AudioClip[] impactSounds;
        [SerializeField] private float impactVolume = 1.0f;
        [SerializeField] private GameObject impactParticlesPrefab;
        [SerializeField] private GameObject carBody;
        [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[4];
        [SerializeField] private ParticleSystem[] skidSmokes = new ParticleSystem[4];
        
        private Vector3 acceleration;
        private Vector3 velocityDelta;
        private Vector3 previousVelocity;
        private float traction;
        private float accelerationInput;
        private float steeringInput;
        private float forwardTilt;
        private float sidewaysTilt;
        private float forwardTiltVelocity;
        private float sidewaysTiltVelocity;
        private bool tireEffectsFlag = false;

        public event Action OnCrashed;
        
        #region Unity lifecycle

        void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            if (engineSoundSource == null)
                engineSoundSource = GetComponent<AudioSource>();
            
            previousVelocity = rb.linearVelocity;
        }

        public void AddInput(float accelerationValue, float steeringValue)
        {
            accelerationInput = Mathf.Clamp(accelerationValue, -1f, 1f);
            steeringInput = Mathf.Clamp(steeringValue, -1f, 1f);
        }

        private void OnCollisionEnter(Collision other)
        {
            // TODO: Check speed and play audio. set minimum speed for activation and make volume depend on speed
        }

        private void Update()
        {
            UpdateEngineSound();
            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            // velocity delta for tilt
            velocityDelta = rb.linearVelocity - previousVelocity;

            // traction interpolation
            float t = rb.linearVelocity.magnitude / maxSpeed;
            traction = Mathf.Lerp(idleTraction, maxTraction, t * t);

            // do not slide when no input
            if (accelerationInput == 0)
                traction = idleTraction;

            // accelerate
            acceleration = accelerationForce * accelerationInput * transform.forward;
            rb.AddForce(acceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
            float projectedSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward));
            float driftCoefficient = Vector3.Dot(rb.linearVelocity.normalized, transform.forward);
            float directionSign = Mathf.Sign(driftCoefficient + accelerationInput);
            float steerAngle = steeringInput * turnSpeed;
            float driftMultiplier = 1.0f;

            // check for drifting
            if (driftCoefficient < driftTreshold)
            {
                driftMultiplier = Mathf.Lerp(1.5f, 1f, Mathf.Abs(driftCoefficient) / driftTreshold);
            }
            
            // steer
            transform.Rotate(steerAngle * driftMultiplier * projectedSpeed * directionSign * Vector3.up * Time.fixedDeltaTime);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity.normalized, Vector3.Project(rb.linearVelocity.normalized, transform.forward).normalized, traction * Time.fixedDeltaTime) * rb.linearVelocity.magnitude;

            previousVelocity = rb.linearVelocity;
        }
        
        #endregion

        private void UpdateEngineSound()
        {
            if (engineSoundSource != null)
            {
                float t = rb.linearVelocity.magnitude / maxSpeed;
                engineSoundSource.pitch = 1 + 2 * Mathf.Sqrt(t);
            }
        }
        
        private void UpdateTilt()
    {
            
        float centerOfGravityHeight = 0.1f;

        // Calculate the torque caused by acceleration
        float forwardForce = Vector3.Dot(velocityDelta * 2f /Time.deltaTime, transform.forward) * rb.mass;
        float tiltTorque = forwardForce * centerOfGravityHeight;

        // Convert torque to angular acceleration
        float momentOfInertia = rb.mass * Mathf.Pow(centerOfGravityHeight, 2); 
        float angularAcceleration = tiltTorque / momentOfInertia;

        // Apply the spring-damper system
        float springForce = -springStiffness * forwardTilt; 
        float dampingForce = -damperCoefficient * forwardTiltVelocity;
        float netAngularAcceleration = angularAcceleration + springForce + dampingForce;

        forwardTiltVelocity += netAngularAcceleration * Time.deltaTime;
        forwardTilt += forwardTiltVelocity * Time.deltaTime;

        forwardTilt = Mathf.Clamp(forwardTilt, -forwardTiltAmount, forwardTiltAmount);

        // Calculate the torque caused by lateral acceleration
        float lateralForce = Vector3.Dot(velocityDelta * 15f / Time.deltaTime, transform.right) * rb.mass;
        float sidewaysTiltTorque = lateralForce * centerOfGravityHeight;

        // Convert torque to angular acceleration
        float sidewaysAngularAcceleration = sidewaysTiltTorque / momentOfInertia;

        // Apply the spring-damper system
        float sidewaysSpringForce = -springStiffness * sidewaysTilt;
        float sidewaysDampingForce = -damperCoefficient * sidewaysTiltVelocity;
        float sidewaysNetAngularAcceleration = sidewaysAngularAcceleration + sidewaysSpringForce + sidewaysDampingForce;

        sidewaysTiltVelocity += sidewaysNetAngularAcceleration * Time.deltaTime;
        sidewaysTilt += sidewaysTiltVelocity * Time.deltaTime;

        sidewaysTilt = Mathf.Clamp(sidewaysTilt, -sidewaysTiltAmount, sidewaysTiltAmount);

        // Apply all
        Vector3 localRotation = carBody.transform.localEulerAngles;
        localRotation.x = forwardTilt;
        localRotation.z = sidewaysTilt;
        carBody.transform.localEulerAngles = localRotation;
    }
        private void UpdateVisuals()
        {
            UpdateTilt();
            UpdateSkidMarks();
        }
        
        private void UpdateSkidMarks()
        {
            if ( Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right)) > minSideSkidVelocity)
                StartSkidEffects();
            else
                StopSkidEffects();
        }
        
        private void StartSkidEffects()
        {
            if (tireEffectsFlag) return;
            foreach (var skidMark in skidMarks)
            {
                skidMark.emitting = true; 
            }
        
            foreach (var smoke in skidSmokes)
            {
                smoke.Play();
            }
            if (skidClip != null)
            {
                skidClip.Play();
            }
            tireEffectsFlag = true;
        }
        
        private void StopSkidEffects()
        {
            if (!tireEffectsFlag) return;

            foreach (var skidMark in skidMarks)
            {
                skidMark.emitting = false; 
            }
        
            foreach (var smoke in skidSmokes)
            {
                smoke.Stop();
            }
            if (skidClip != null)
            {
                skidClip.Stop();
            }
            tireEffectsFlag = false;
        }
    }
}