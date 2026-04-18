using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {

        [Header("Target")] [Tooltip("The Transform the camera will follow.")]
        public Transform target;
        
        public Vector3 positionOffset = new Vector3(0f, 2f, 0f);

        [Header("Follow")]
        [Tooltip("Baseline SmoothDamp smooth time when close to the player. " +
                 "Lower = snappier.")]
        public float baseSmoothTime = 0.08f;

        [Tooltip("At this player speed the camera reaches its fastest follow. " +
                 "Match this to your player's top speed.")]
        public float topSpeed = 18f;

        [Tooltip("Smooth time used when the player is at top speed. " +
                 "Gives the camera a sense of inertia at high velocity.")]
        public float fastSmoothTime = 0.04f;

        [Tooltip("If the player is this many world-units outside the visible area, " +
                 "the camera hard-clamps to keep them in frame.")]
        public float hardClampMargin = 0.5f;


        [Header("Look-Ahead")] [Tooltip("How far ahead (world units) the camera peeks in the direction of movement.")]
        public float lookAheadDistance = 2.2f;

        [Tooltip("How quickly the look-ahead offset tracks the velocity direction.")]
        public float lookAheadSmooth = 6f;


        [Header("Orthographic Size")] [Tooltip("Size at rest.")]
        public float baseSize = 5f;

        [Tooltip("Extra size added when the player is at top speed.")]
        public float maxSizeBoost = 2.5f;

        [Tooltip("Additional burst added on sudden acceleration. Feels like a punch.")]
        public float accelerationSizePunch = 0.8f;

        [Tooltip("How fast the size grows toward the target (larger = snappier zoom-out).")]
        public float sizeGrowSpeed = 4f;

        [Tooltip("How fast the size shrinks back (smaller value = satisfying slow zoom-in after stopping).")]
        public float sizeShrinkSpeed = 2f;


        [Header("Tilt")] [Tooltip("Maximum tilt angle in degrees at top speed.")]
        public float maxTiltDegrees = 4f;

        [Tooltip("Extra tilt added on top of velocity-tilt, driven by acceleration. " +
                 "Makes the camera feel alive during direction changes.")]
        public float accelerationTiltBoost = 2.5f;

        [Tooltip("How fast the tilt reaches its target.")]
        public float tiltSpeed = 8f;

        [Tooltip("Which world axis to tilt around. " +
                 "Z = roll (great for top-down / 2D side-view). " +
                 "X = pitch (front-facing 3D top-down). " +
                 "Choose the axis perpendicular to your camera's up-direction.")]
        public TiltAxis tiltAxis = TiltAxis.Z;

        [Tooltip("Which component of velocity drives the tilt. " +
                 "X = horizontal lean (side-scroller). " +
                 "Y = vertical lean (top-down). " +
                 "XY magnitude = omnidirectional lean.")]
        public TiltDriver tiltDriver = TiltDriver.X;


        public enum TiltAxis   { X, Z }
        public enum TiltDriver { X, Z, XZMagnitude }


        private Camera _cam;
        private Rigidbody  _targetRb;
        private Quaternion _baseRotation; // world rotation, never modified
        private Vector3 _smoothVelocity; // SmoothDamp internal velocity

        private Vector3 _prevTargetPos;
        private Vector3 _targetVelocity; // estimated from position delta
        private Vector3 _targetAcceleration; // estimated from velocity delta
        private Vector3 _prevTargetVelocity;

        private Vector3 _lookAheadOffset;
        private float _currentTilt;
        private float _currentSize;


        void Awake()
        {
            transform.SetParent(null, worldPositionStays: true);
            
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = baseSize;
            _currentSize = baseSize;

            _baseRotation = transform.rotation;

            if (target != null)
            {
                _targetRb = target.GetComponent<Rigidbody>();
                
                _prevTargetPos = target.position;
                _prevTargetVelocity = Vector3.zero;

                transform.position = DesiredPosition(target.position, Vector3.zero);
                // Start on top of the player
                //Vector3 startPos = target.position + new Vector3(positionOffset.x, positionOffset.y, 0f);
                //startPos.z = transform.position.z + positionOffset.z;
                //transform.position = startPos;
            }
        }


        void LateUpdate()
        {
            if (target == null) return;

            float dt = Time.smoothDeltaTime;
            if (dt <= 0f) return;


            Vector3 rawVelocity;
            
            if (_targetRb != null)
            {
                rawVelocity = _targetRb.linearVelocity;
            }
            else
            {
                rawVelocity    = (target.position - _prevTargetPos) / dt;
                _prevTargetPos = target.position;
            }
            
            _targetAcceleration = (rawVelocity - _prevTargetVelocity) / dt;
            _targetVelocity     = rawVelocity;
            _prevTargetVelocity = rawVelocity;

            float speed = _targetVelocity.magnitude;
            float accelMag = _targetAcceleration.magnitude;
            float speedT = Mathf.Clamp01(speed / Mathf.Max(topSpeed, 0.01f));
            float accelT = Mathf.Clamp01(accelMag / (topSpeed * 8f)); // punch threshold


            Vector3 lookAheadTarget = Vector3.zero;
            if (speed > 0.05f)
            {
                Vector3 flatVel = new Vector3(_targetVelocity.x, 0f, _targetVelocity.z);
                lookAheadTarget = flatVel.normalized * lookAheadDistance * speedT;
            }

            _lookAheadOffset = Vector3.Lerp(_lookAheadOffset, lookAheadTarget, dt * lookAheadSmooth);


            Vector3 desired = DesiredPosition(target.position, _lookAheadOffset);

            // Smooth time: blend between base and fast depending on speed
            float smoothTime = Mathf.Lerp(baseSmoothTime, fastSmoothTime, speedT);

            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref _smoothVelocity, smoothTime);


            HardClampToView();


            float targetSize = baseSize
                               + speedT * maxSizeBoost
                               + accelT * accelerationSizePunch;

            // Asymmetric speed: zoom out fast, zoom in slowly for linger effect
            float sizeSpeed = (_currentSize < targetSize) ? sizeGrowSpeed : sizeShrinkSpeed;
            _currentSize = Mathf.Lerp(_currentSize, targetSize, dt * sizeSpeed);
            _cam.orthographicSize = _currentSize;


            float velocityComponent = GetTiltDriverValue(_targetVelocity);
            float accelComponent = GetTiltDriverValue(_targetAcceleration);

            // Velocity gives steady lean; acceleration gives snappy punch on direction changes
            float velocityTiltT = Mathf.Clamp(velocityComponent / Mathf.Max(topSpeed, 0.01f), -1f, 1f);
            float accelTiltT = Mathf.Clamp(accelComponent / (topSpeed * 8f), -1f, 1f);

            float targetTilt = velocityTiltT * maxTiltDegrees
                               + accelTiltT * accelerationTiltBoost;

            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, dt * tiltSpeed);

            // --------
            
            Vector3 worldTiltAxis = tiltAxis == TiltAxis.X ? Vector3.right : Vector3.forward;
            Quaternion tiltQ = Quaternion.AngleAxis(-_currentTilt, worldTiltAxis);
            transform.rotation = tiltQ * _baseRotation;
        }

        Vector3 DesiredPosition(Vector3 playerPos, Vector3 lookAhead)
        {
            return new Vector3(
                playerPos.x + positionOffset.x + lookAhead.x,
                playerPos.y + positionOffset.y,
                playerPos.z + positionOffset.z + lookAhead.z);
        }


        void HardClampToView()
        {
            if (target == null) return;

            float halfH = _currentSize;
            float halfW = _currentSize * _cam.aspect;

            Vector3 localDelta = transform.InverseTransformVector(target.position - transform.position);
            
            float dx = 0f, dy = 0f;
            float lx = localDelta.x, ly = localDelta.y;
 
            if      (lx < -(halfW - hardClampMargin)) dx = lx + (halfW - hardClampMargin);
            else if (lx >   halfW - hardClampMargin)  dx = lx - (halfW - hardClampMargin);
 
            if      (ly < -(halfH - hardClampMargin)) dy = ly + (halfH - hardClampMargin);
            else if (ly >   halfH - hardClampMargin)  dy = ly - (halfH - hardClampMargin);
 
            if (dx == 0f && dy == 0f) return;
 
            Vector3 worldCorrection = transform.TransformVector(new Vector3(dx, dy, 0f));
            transform.position += worldCorrection;
            _smoothVelocity    += worldCorrection / Time.deltaTime * 0.5f;
        }

        /// Returns the scalar value of the velocity/acceleration that should drive the tilt.
        float GetTiltDriverValue(Vector3 v)
        {
            return tiltDriver switch
            {
                TiltDriver.X           => v.x,
                TiltDriver.Z           => v.z,
                TiltDriver.XZMagnitude => new Vector2(v.x, v.z).magnitude * Mathf.Sign(v.x),
                _                      => v.x
            };
        }


#if UNITY_EDITOR
        void OnValidate()
        {
            baseSize = Mathf.Max(0.1f, baseSize);
            maxSizeBoost = Mathf.Max(0f, maxSizeBoost);
            accelerationSizePunch = Mathf.Max(0f, accelerationSizePunch);
            maxTiltDegrees = Mathf.Clamp(maxTiltDegrees, 0f, 45f);
            accelerationTiltBoost = Mathf.Max(0f, accelerationTiltBoost);
            topSpeed = Mathf.Max(0.1f, topSpeed);
            hardClampMargin = Mathf.Max(0f, hardClampMargin);
        }

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            // Draw the hard-clamp rectangle
            float halfH = _currentSize;
            float halfW = _currentSize * (_cam != null ? _cam.aspect : 1.77f);
            Vector3 c = transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(c + new Vector3(-halfW + hardClampMargin, halfH - hardClampMargin, 0),
                c + new Vector3(halfW - hardClampMargin, halfH - hardClampMargin, 0));
            Gizmos.DrawLine(c + new Vector3(halfW - hardClampMargin, halfH - hardClampMargin, 0),
                c + new Vector3(halfW - hardClampMargin, -halfH + hardClampMargin, 0));
            Gizmos.DrawLine(c + new Vector3(halfW - hardClampMargin, -halfH + hardClampMargin, 0),
                c + new Vector3(-halfW + hardClampMargin, -halfH + hardClampMargin, 0));
            Gizmos.DrawLine(c + new Vector3(-halfW + hardClampMargin, -halfH + hardClampMargin, 0),
                c + new Vector3(-halfW + hardClampMargin, halfH - hardClampMargin, 0));

            // Draw look-ahead offset
            Gizmos.color = Color.yellow;
            if (target != null)
                Gizmos.DrawLine(target.position, target.position + _lookAheadOffset);
        }
#endif
    }
}