using UnityEngine;
using Unity.Profiling;

namespace VoxelSandbox.Vehicle
{
    /// <summary>
    /// AAA-quality vehicle controller for rover/buggy.
    /// Implements realistic wheel physics for mobile devices.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("Wheel Configuration")]
        [SerializeField] private WheelCollider frontLeftWheel;
        [SerializeField] private WheelCollider frontRightWheel;
        [SerializeField] private WheelCollider rearLeftWheel;
        [SerializeField] private WheelCollider rearRightWheel;
        
        [SerializeField] private Transform frontLeftTransform;
        [SerializeField] private Transform frontRightTransform;
        [SerializeField] private Transform rearLeftTransform;
        [SerializeField] private Transform rearRightTransform;
        
        [Header("Vehicle Properties")]
        [SerializeField] private float maxMotorTorque = 1500f;
        [SerializeField] private float maxSteeringAngle = 30f;
        [SerializeField] private float brakeTorque = 3000f;
        [SerializeField] private float maxSpeed = 20f; // m/s
        
        [Header("Mobile Controls")]
        [SerializeField] private float accelerationInput = 0f;
        [SerializeField] private float steeringInput = 0f;
        [SerializeField] private bool brakeInput = false;
        
        [Header("Audio")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private float minEnginePitch = 0.8f;
        [SerializeField] private float maxEnginePitch = 2.0f;
        
        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem[] dustParticles;
        [SerializeField] private ParticleSystem[] wheelTrailParticles;
        
        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float cameraDistance = 5f;
        [SerializeField] private float cameraHeight = 2f;
        [SerializeField] private float cameraSmoothSpeed = 5f;
        
        private Rigidbody rb;
        private float currentSpeed;
        private bool isGrounded;
        
        private static readonly ProfilerMarker s_UpdatePhysicsMarker = new ProfilerMarker("VehicleController.UpdatePhysics");
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            // Configure rigidbody for vehicle physics
            rb.mass = 1200f; // Typical small vehicle mass
            rb.centerOfMass = new Vector3(0, -0.5f, 0); // Lower center of mass for stability
            rb.drag = 0.05f;
            rb.angularDrag = 0.5f;
        }
        
        private void Start()
        {
            // Configure wheel colliders
            ConfigureWheelCollider(frontLeftWheel);
            ConfigureWheelCollider(frontRightWheel);
            ConfigureWheelCollider(rearLeftWheel);
            ConfigureWheelCollider(rearRightWheel);
            
            // Initialize audio
            if (engineAudioSource != null)
            {
                engineAudioSource.loop = true;
                engineAudioSource.Play();
            }
        }
        
        private void FixedUpdate()
        {
            using (s_UpdatePhysicsMarker.Auto())
            {
                UpdatePhysics();
                UpdateWheelVisuals();
            }
        }
        
        private void Update()
        {
            UpdateAudio();
            UpdateParticleEffects();
            
            // Calculate current speed
            currentSpeed = rb.velocity.magnitude;
            
            // Check if grounded
            isGrounded = IsVehicleGrounded();
        }
        
        /// <summary>
        /// Set vehicle input from mobile controls or keyboard.
        /// </summary>
        /// <param name="acceleration">-1 to 1 (negative for reverse)</param>
        /// <param name="steering">-1 to 1 (negative for left)</param>
        /// <param name="brake">True to brake</param>
        public void SetInput(float acceleration, float steering, bool brake)
        {
            accelerationInput = Mathf.Clamp(acceleration, -1f, 1f);
            steeringInput = Mathf.Clamp(steering, -1f, 1f);
            brakeInput = brake;
        }
        
        private void UpdatePhysics()
        {
            // Speed limiting
            if (currentSpeed > maxSpeed)
            {
                accelerationInput = Mathf.Min(accelerationInput, 0);
            }
            
            // Apply motor torque
            float motorTorque = accelerationInput * maxMotorTorque;
            rearLeftWheel.motorTorque = motorTorque;
            rearRightWheel.motorTorque = motorTorque;
            
            // Apply steering
            float steeringAngle = steeringInput * maxSteeringAngle;
            frontLeftWheel.steerAngle = steeringAngle;
            frontRightWheel.steerAngle = steeringAngle;
            
            // Apply brakes
            float brake = brakeInput ? brakeTorque : 0f;
            frontLeftWheel.brakeTorque = brake;
            frontRightWheel.brakeTorque = brake;
            rearLeftWheel.brakeTorque = brake;
            rearRightWheel.brakeTorque = brake;
        }
        
        private void UpdateWheelVisuals()
        {
            UpdateWheelTransform(frontLeftWheel, frontLeftTransform);
            UpdateWheelTransform(frontRightWheel, frontRightTransform);
            UpdateWheelTransform(rearLeftWheel, rearLeftTransform);
            UpdateWheelTransform(rearRightWheel, rearRightTransform);
        }
        
        private void UpdateWheelTransform(WheelCollider collider, Transform transform)
        {
            if (collider == null || transform == null) return;
            
            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);
            
            transform.position = position;
            transform.rotation = rotation;
        }
        
        private void UpdateAudio()
        {
            if (engineAudioSource == null) return;
            
            // Calculate engine pitch based on speed and acceleration
            float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
            float accelerationFactor = Mathf.Abs(accelerationInput);
            float targetPitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, Mathf.Max(speedFactor, accelerationFactor * 0.5f));
            
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * 3f);
            engineAudioSource.volume = Mathf.Lerp(0.3f, 1.0f, Mathf.Max(speedFactor, accelerationFactor));
        }
        
        private void UpdateParticleEffects()
        {
            bool shouldEmitDust = isGrounded && (Mathf.Abs(accelerationInput) > 0.1f || currentSpeed > 2f);
            
            // Update dust particles
            if (dustParticles != null)
            {
                foreach (var dust in dustParticles)
                {
                    if (dust != null)
                    {
                        var emission = dust.emission;
                        emission.enabled = shouldEmitDust;
                        
                        if (shouldEmitDust)
                        {
                            var main = dust.main;
                            main.startSpeed = Mathf.Lerp(1f, 5f, currentSpeed / maxSpeed);
                        }
                    }
                }
            }
            
            // Update wheel trail particles
            if (wheelTrailParticles != null)
            {
                foreach (var trail in wheelTrailParticles)
                {
                    if (trail != null)
                    {
                        var emission = trail.emission;
                        emission.enabled = shouldEmitDust && Mathf.Abs(steeringInput) > 0.3f;
                    }
                }
            }
        }
        
        private bool IsVehicleGrounded()
        {
            return (frontLeftWheel != null && frontLeftWheel.isGrounded) ||
                   (frontRightWheel != null && frontRightWheel.isGrounded) ||
                   (rearLeftWheel != null && rearLeftWheel.isGrounded) ||
                   (rearRightWheel != null && rearRightWheel.isGrounded);
        }
        
        private void ConfigureWheelCollider(WheelCollider wheel)
        {
            if (wheel == null) return;
            
            // Configure suspension
            JointSpring suspensionSpring = wheel.suspensionSpring;
            suspensionSpring.spring = 35000f;
            suspensionSpring.damper = 4500f;
            suspensionSpring.targetPosition = 0.5f;
            wheel.suspensionSpring = suspensionSpring;
            
            wheel.suspensionDistance = 0.3f;
            wheel.forceAppPointDistance = 0f;
            
            // Configure friction curves for realistic handling
            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.extremumValue = 1f;
            forwardFriction.asymptoteSlip = 0.8f;
            forwardFriction.asymptoteValue = 0.5f;
            forwardFriction.stiffness = 1f;
            wheel.forwardFriction = forwardFriction;
            
            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.extremumValue = 1f;
            sidewaysFriction.asymptoteSlip = 0.5f;
            sidewaysFriction.asymptoteValue = 0.75f;
            sidewaysFriction.stiffness = 1f;
            wheel.sidewaysFriction = sidewaysFriction;
        }
        
        /// <summary>
        /// Get current vehicle speed in km/h.
        /// </summary>
        public float GetSpeedKmh()
        {
            return currentSpeed * 3.6f;
        }
        
        /// <summary>
        /// Get current vehicle speed in m/s.
        /// </summary>
        public float GetSpeed()
        {
            return currentSpeed;
        }
        
        /// <summary>
        /// Check if vehicle is on the ground.
        /// </summary>
        public bool IsGrounded()
        {
            return isGrounded;
        }
        
        /// <summary>
        /// Reset vehicle to upright position (emergency flip).
        /// </summary>
        public void ResetOrientation()
        {
            Vector3 position = transform.position;
            position.y += 2f;
            transform.position = position;
            
            Quaternion rotation = transform.rotation;
            rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            transform.rotation = rotation;
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
