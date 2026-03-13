using UnityEngine;

namespace VoxelSandbox.Vehicle
{
    /// <summary>
    /// Smooth third-person camera controller for vehicles.
    /// Implements AAA-quality camera with smooth transitions and collision avoidance.
    /// </summary>
    public class VehicleCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0, 1f, 0);
        
        [Header("Camera Settings")]
        [SerializeField] private float distance = 6f;
        [SerializeField] private float height = 2.5f;
        [SerializeField] private float angleX = 10f;
        
        [Header("Smoothing")]
        [SerializeField] private float positionSmoothing = 5f;
        [SerializeField] private float rotationSmoothing = 3f;
        [SerializeField] private float velocityInfluence = 0.1f;
        
        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minDistance = 1f;
        
        [Header("Look Ahead")]
        [SerializeField] private float lookAheadDistance = 3f;
        [SerializeField] private float lookAheadSpeed = 10f;
        
        private Vector3 currentVelocity;
        private float currentDistance;
        private VehicleController vehicleController;
        
        private void Start()
        {
            if (target != null)
            {
                vehicleController = target.GetComponent<VehicleController>();
            }
            
            currentDistance = distance;
        }
        
        private void LateUpdate()
        {
            if (target == null) return;
            
            UpdateCamera();
        }
        
        private void UpdateCamera()
        {
            // Calculate desired position
            Vector3 targetPoint = target.position + targetOffset;
            
            // Add velocity-based look ahead
            if (vehicleController != null)
            {
                Rigidbody rb = target.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 velocity = rb.velocity;
                    float speed = velocity.magnitude;
                    
                    if (speed > 1f)
                    {
                        Vector3 lookAhead = velocity.normalized * Mathf.Min(speed * velocityInfluence, lookAheadDistance);
                        targetPoint += Vector3.Lerp(Vector3.zero, lookAhead, Time.deltaTime * lookAheadSpeed);
                    }
                }
            }
            
            // Calculate camera rotation based on target's forward
            Quaternion targetRotation = Quaternion.Euler(angleX, target.eulerAngles.y, 0);
            
            // Calculate ideal camera position
            Vector3 idealPosition = targetPoint - (targetRotation * Vector3.forward * distance);
            idealPosition.y = targetPoint.y + height;
            
            // Check for collision
            Vector3 directionToCamera = idealPosition - targetPoint;
            float targetDistance = directionToCamera.magnitude;
            
            RaycastHit hit;
            if (Physics.SphereCast(targetPoint, collisionRadius, directionToCamera.normalized, 
                                  out hit, targetDistance, collisionMask))
            {
                currentDistance = Mathf.Max(hit.distance - collisionRadius, minDistance);
            }
            else
            {
                currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * positionSmoothing);
            }
            
            // Apply collision-adjusted distance
            Vector3 finalPosition = targetPoint - (targetRotation * Vector3.forward * currentDistance);
            finalPosition.y = targetPoint.y + height;
            
            // Smooth position
            transform.position = Vector3.SmoothDamp(transform.position, finalPosition, 
                                                   ref currentVelocity, 1f / positionSmoothing);
            
            // Smooth rotation to look at target
            Quaternion lookRotation = Quaternion.LookRotation(targetPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 
                                                 Time.deltaTime * rotationSmoothing);
        }
        
        /// <summary>
        /// Set camera target.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                vehicleController = target.GetComponent<VehicleController>();
            }
        }
        
        /// <summary>
        /// Set camera distance from target.
        /// </summary>
        public void SetDistance(float newDistance)
        {
            distance = Mathf.Max(minDistance, newDistance);
        }
        
        /// <summary>
        /// Set camera height above target.
        /// </summary>
        public void SetHeight(float newHeight)
        {
            height = newHeight;
        }
    }
}
