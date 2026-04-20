using System;
using Service.Audio;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleMovement : MonoBehaviour
    {
        [Header("Vehicle Settings")] [SerializeField]
        public float accelerationForce = 25f;

        [SerializeField] private float maxSpeed = 15f;
        [SerializeField] private float maxTraction = 2f;
        [SerializeField] private float idleTraction = 4f;
        [SerializeField] private float turnSpeed = 50f;
        [SerializeField] private float driftThreshold = 0.45f;
        [SerializeField] private float forwardTiltAmount = 3.3f;
        [SerializeField] private float springStiffness = 300f;
        [SerializeField] private float damperCoefficient = 15f;
        [SerializeField] private float sidewaysTiltAmount = 10f;
        [SerializeField] private float minSideSkidVelocity = 2f;
        [SerializeField] private AnimationCurve steeringCurve;

        [Header("Ground Detection")] [SerializeField]
        private float groundCheckDistance = 0.35f;

        [SerializeField] private LayerMask groundLayer = ~0;

        [Header("References")] [SerializeField]
        private Rigidbody rb;
        
        [Header("Wheel Visuals")]
        [SerializeField] private GameObject[] frontWheelMeshes;
        [SerializeField] private GameObject[] backWheelMeshes;
        [SerializeField] private float wheelRadius = 0.3f;
        [SerializeField] private float maxSteerAngle = 30f;
        
        
        [SerializeField] private AudioSource engineSoundSource;
        [SerializeField] private AudioSource skidClip;
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
        private bool tireEffectsFlag;
        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;
        
        private float wheelRotationAngle;

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

        private void Update()
        {
            UpdateEngineSound();
            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            velocityDelta = rb.linearVelocity - previousVelocity;

            CheckGrounded();

            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            float speed = horizontalVelocity.magnitude;

            // Traction high when slow (planted), eases off at top speed;
            float normalizedSpeed = speed / maxSpeed;
            float t = normalizedSpeed;
            t = t * t * t; // smoother curve
            traction = Mathf.Lerp(idleTraction, maxTraction, t);
            //if (accelerationInput == 0f)
                //traction = idleTraction;

            // Drive force projected onto the surface so the car hugs slopes correctly.
            if (isGrounded)
            {
                Vector3 driveDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                rb.AddForce(
                    accelerationForce * accelerationInput * driveDirection * Time.fixedDeltaTime,
                    ForceMode.Acceleration
                );

                // Re-read horizontal velocity now that the drive impulse has been applied.
                horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
                speed = horizontalVelocity.magnitude;
            }

            // Signed speed along flat forward
            float projectedSpeed = Vector3.Dot(horizontalVelocity, flatForward);
            
            // How well the current velocity aligns with where the car is pointing.
            float driftCoefficient = speed > 0.1f
                ? Vector3.Dot(horizontalVelocity.normalized, flatForward)
                : 1f;

            // Flip steer direction when reversing; guard against zero (kills steering).
            float directionSign = Mathf.Sign(driftCoefficient + accelerationInput);
            if (directionSign == 0f) directionSign = 1f;

            // Widen the steering arc slightly during a drift (kept mild for RC feel).
            float driftMultiplier = 1f;
            if (driftCoefficient < driftThreshold)
                driftMultiplier = Mathf.Lerp(1.5f, 1f, Mathf.Abs(driftCoefficient) / driftThreshold);

            float speed01 = Mathf.Clamp01(Mathf.Abs(projectedSpeed) / maxSpeed);
            float turnFactor = steeringCurve.Evaluate(speed01);
            
            transform.Rotate(
                steeringInput * turnSpeed * driftMultiplier
                * turnFactor * directionSign
                * Vector3.up * Time.fixedDeltaTime
            );
            
            // New: speed-based steering reduction
 
            // ---

            // Refresh after rotation so the friction step uses the new heading.
            flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            // Horizontal speed cap (vertical / gravity is intentionally untouched)
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeed);

            // Lateral friction
            // Blends velocity toward the car's forward direction. Produces the slight
            // RC drift at speed while keeping the car planted at low speed / idle.
            if (speed > 0.01f)
            {
                Vector3 projectedOnForward = Vector3.Project(horizontalVelocity.normalized, flatForward);

                // Guard: if velocity is nearly perpendicular to forward, the projection
                // approaches zero. Normalising it would produce NaN; fall back to flatForward.
                Vector3 targetDirection = projectedOnForward.sqrMagnitude > 0.001f
                    ? projectedOnForward.normalized
                    : flatForward;

                Vector3 correctedHorizontal = Vector3.Lerp(
                    horizontalVelocity.normalized,
                    targetDirection,
                    traction * Time.fixedDeltaTime
                ) * horizontalVelocity.magnitude;

                // Recompose: corrected horizontal plane + whatever vertical value physics produced.
                rb.linearVelocity = new Vector3(
                    correctedHorizontal.x,
                    rb.linearVelocity.y,
                    correctedHorizontal.z
                );
            }
            else
            {
                rb.linearVelocity = new Vector3(
                    horizontalVelocity.x,
                    rb.linearVelocity.y,
                    horizontalVelocity.z
                );
            }
            previousVelocity = rb.linearVelocity;
        }

        #endregion

        private void CheckGrounded()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                isGrounded = true;
                groundNormal = hit.normal;
            }
            else
            {
                isGrounded = false;
                groundNormal = Vector3.up;
            }

            isGrounded = true;
        }

        private void UpdateEngineSound()
        {
            if (engineSoundSource == null) return;
            float t = rb.linearVelocity.magnitude / maxSpeed;
            engineSoundSource.pitch = 0.6f + 1.5f * Mathf.Sqrt(t);
            engineSoundSource.volume = Mathf.Sqrt(t) * 1.5f;
        }
        
        private void UpdateVisuals()
        {
            UpdateTilt();
            UpdateSkidMarks();
            UpdateWheelVisuals();
        }

        private void UpdateTilt()
        {
            float centerOfGravityHeight = 0.1f;
            
            // Calculate the torque caused by acceleration
            float forwardForce = Vector3.Dot(velocityDelta * 2f / Time.deltaTime, transform.forward) * rb.mass;
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
            float sidewaysNetAngularAcceleration =
                sidewaysAngularAcceleration + sidewaysSpringForce + sidewaysDampingForce;

            sidewaysTiltVelocity += sidewaysNetAngularAcceleration * Time.deltaTime;
            sidewaysTilt += sidewaysTiltVelocity * Time.deltaTime;

            sidewaysTilt = Mathf.Clamp(sidewaysTilt, -sidewaysTiltAmount, sidewaysTiltAmount);

            // Apply all
            Vector3 localRotation = carBody.transform.localEulerAngles;
            localRotation.x = forwardTilt;
            localRotation.z = sidewaysTilt;
            carBody.transform.localEulerAngles = localRotation;
        }

        

        private void UpdateSkidMarks()
        {
            bool isSideSkidding = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right)) > minSideSkidVelocity;
            
            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            bool isBrakingSkid = false;
            
            if (isGrounded && Mathf.Abs(forwardSpeed) > 2f) 
            {
                if ((forwardSpeed > 0 && accelerationInput < -0.1f) || (forwardSpeed < 0 && accelerationInput > 0.1f))
                {
                    isBrakingSkid = true;
                }
            }
            
            if (isSideSkidding || isBrakingSkid)
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

            //foreach (var smoke in skidSmokes)
            //{
            //    smoke.Play();
            //}

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

            //foreach (var smoke in skidSmokes)
            //{
            //    smoke.Stop();
            //}

            if (skidClip != null)
            {
                //skidClip.Stop();
            }

            tireEffectsFlag = false;
        }
        
        private void UpdateWheelVisuals()
        {
            float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float rotationAmount = (speed * Time.deltaTime) / (2 * Mathf.PI * wheelRadius) * 360f;
            wheelRotationAngle += rotationAmount;
            
            foreach (var wheel in frontWheelMeshes)
            {
                wheel.transform.localRotation = Quaternion.Euler(0, steeringInput * maxSteerAngle, -wheelRotationAngle);
            }
            
            foreach (var wheel in backWheelMeshes)
            {
                wheel.transform.localRotation = Quaternion.Euler(0, 0, -wheelRotationAngle);
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            float impactForce = other.relativeVelocity.magnitude;

            
            if (impactForce > 2f)
            {
                PlayHitSound(impactForce, other.contacts[0].point);
                OnCrashed?.Invoke();
            }
        }

        private void PlayHitSound(float force, Vector3 impactPoint)
        {
            float volumeScale = Mathf.Clamp01(force / maxSpeed) * impactVolume;
            Debug.Log($"OnCollisionEnter {volumeScale}");
            Service.Services.GetService<AudioService>().PlaySound("hit", volumeScale*volumeScale);
        }
    }
}