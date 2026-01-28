using UnityEngine;
using UnityEngine.EventSystems;

namespace VoxelSandbox.Input
{
    /// <summary>
    /// Mobile-optimized touch input system for AAA quality controls.
    /// Provides virtual joysticks, buttons, and touch gestures.
    /// </summary>
    public class MobileInputManager : MonoBehaviour
    {
        [Header("Virtual Joystick - Movement")]
        [SerializeField] private RectTransform movementJoystickArea;
        [SerializeField] private RectTransform movementJoystickHandle;
        [SerializeField] private float movementJoystickRadius = 50f;
        
        [Header("Virtual Joystick - Camera")]
        [SerializeField] private RectTransform cameraJoystickArea;
        [SerializeField] private RectTransform cameraJoystickHandle;
        [SerializeField] private float cameraJoystickRadius = 50f;
        [SerializeField] private float cameraSensitivity = 2f;
        
        [Header("Vehicle Controls")]
        [SerializeField] private RectTransform vehicleSteeringWheel;
        [SerializeField] private RectTransform vehicleAccelerator;
        [SerializeField] private RectTransform vehicleBrake;
        [SerializeField] private bool vehicleControlsActive = false;
        
        [Header("Build Mode")]
        [SerializeField] private GameObject buildModeUI;
        [SerializeField] private bool buildModeActive = false;
        
        // Input states
        private Vector2 movementInput = Vector2.zero;
        private Vector2 cameraInput = Vector2.zero;
        private float vehicleAcceleration = 0f;
        private float vehicleSteering = 0f;
        private bool vehicleBraking = false;
        
        // Touch tracking
        private int movementTouchId = -1;
        private int cameraTouchId = -1;
        private Vector2 movementTouchStart;
        private Vector2 cameraTouchStart;
        
        // Gesture detection
        private float lastTapTime = 0f;
        private Vector2 lastTapPosition;
        private const float DOUBLE_TAP_TIME = 0.3f;
        
        private void Awake()
        {
            // Register with ServiceLocator
            Core.ServiceLocator.Register(this);
        }
        
        private void Update()
        {
            ProcessTouchInput();
            UpdateVirtualJoysticks();
            
            // Detect gestures
            DetectDoubleTap();
            DetectPinch();
        }
        
        private void ProcessTouchInput()
        {
            // Reset inputs if no active touches in their areas
            bool movementTouchActive = false;
            bool cameraTouchActive = false;
            
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                Touch touch = UnityEngine.Input.GetTouch(i);
                
                // Check if touch is over UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    ProcessUITouch(touch);
                    continue;
                }
                
                // Process movement joystick
                if (IsInArea(touch.position, movementJoystickArea))
                {
                    ProcessMovementTouch(touch);
                    movementTouchActive = true;
                }
                // Process camera joystick
                else if (IsInArea(touch.position, cameraJoystickArea))
                {
                    ProcessCameraTouch(touch);
                    cameraTouchActive = true;
                }
            }
            
            // Reset if no touches
            if (!movementTouchActive)
            {
                movementInput = Vector2.zero;
                movementTouchId = -1;
            }
            
            if (!cameraTouchActive)
            {
                cameraInput = Vector2.zero;
                cameraTouchId = -1;
            }
        }
        
        private void ProcessMovementTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (movementTouchId == -1)
                    {
                        movementTouchId = touch.fingerId;
                        movementTouchStart = touch.position;
                    }
                    break;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (movementTouchId == touch.fingerId)
                    {
                        Vector2 delta = touch.position - movementTouchStart;
                        float distance = delta.magnitude;
                        
                        if (distance > movementJoystickRadius)
                        {
                            delta = delta.normalized * movementJoystickRadius;
                        }
                        
                        movementInput = delta / movementJoystickRadius;
                    }
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (movementTouchId == touch.fingerId)
                    {
                        movementInput = Vector2.zero;
                        movementTouchId = -1;
                    }
                    break;
            }
        }
        
        private void ProcessCameraTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (cameraTouchId == -1)
                    {
                        cameraTouchId = touch.fingerId;
                        cameraTouchStart = touch.position;
                    }
                    break;
                    
                case TouchPhase.Moved:
                    if (cameraTouchId == touch.fingerId)
                    {
                        cameraInput = touch.deltaPosition * cameraSensitivity;
                    }
                    break;
                    
                case TouchPhase.Stationary:
                    if (cameraTouchId == touch.fingerId)
                    {
                        cameraInput = Vector2.zero;
                    }
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (cameraTouchId == touch.fingerId)
                    {
                        cameraInput = Vector2.zero;
                        cameraTouchId = -1;
                    }
                    break;
            }
        }
        
        private void ProcessUITouch(Touch touch)
        {
            // Handle vehicle controls
            if (vehicleControlsActive)
            {
                if (IsInArea(touch.position, vehicleAccelerator))
                {
                    vehicleAcceleration = touch.phase == TouchPhase.Ended ? 0f : 1f;
                }
                else if (IsInArea(touch.position, vehicleBrake))
                {
                    vehicleBraking = touch.phase != TouchPhase.Ended;
                }
                else if (IsInArea(touch.position, vehicleSteeringWheel))
                {
                    // Calculate steering based on touch position relative to center
                    Vector2 center = RectTransformToScreenPoint(vehicleSteeringWheel);
                    Vector2 direction = (touch.position - center).normalized;
                    vehicleSteering = direction.x;
                }
            }
        }
        
        private void UpdateVirtualJoysticks()
        {
            // Update movement joystick visual
            if (movementJoystickHandle != null)
            {
                movementJoystickHandle.anchoredPosition = movementInput * movementJoystickRadius;
            }
            
            // Camera joystick doesn't need visual update (it's gesture-based)
        }
        
        private void DetectDoubleTap()
        {
            if (UnityEngine.Input.touchCount == 1)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    float timeSinceLastTap = Time.time - lastTapTime;
                    float distance = Vector2.Distance(touch.position, lastTapPosition);
                    
                    if (timeSinceLastTap < DOUBLE_TAP_TIME && distance < 50f)
                    {
                        OnDoubleTap(touch.position);
                    }
                    
                    lastTapTime = Time.time;
                    lastTapPosition = touch.position;
                }
            }
        }
        
        private void DetectPinch()
        {
            if (UnityEngine.Input.touchCount == 2)
            {
                Touch touch0 = UnityEngine.Input.GetTouch(0);
                Touch touch1 = UnityEngine.Input.GetTouch(1);
                
                if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                    Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                    
                    float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
                    float currentMagnitude = (touch0.position - touch1.position).magnitude;
                    
                    float delta = currentMagnitude - prevMagnitude;
                    
                    OnPinch(delta);
                }
            }
        }
        
        private bool IsInArea(Vector2 screenPoint, RectTransform rectTransform)
        {
            if (rectTransform == null) return false;
            
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint);
        }
        
        private Vector2 RectTransformToScreenPoint(RectTransform rectTransform)
        {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                rectTransform.position,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            return localPoint;
        }
        
        // Event callbacks
        private void OnDoubleTap(Vector2 position)
        {
            // Can be used for jump, toggle fly mode, etc.
            Core.Logger.LogInfo($"Double tap detected at {position}");
        }
        
        private void OnPinch(float delta)
        {
            // Can be used for zoom, scale, etc.
            // Core.Logger.LogInfo($"Pinch delta: {delta}");
        }
        
        // Public API
        
        /// <summary>
        /// Get movement input (-1 to 1 for x and y).
        /// </summary>
        public Vector2 GetMovementInput()
        {
            return movementInput;
        }
        
        /// <summary>
        /// Get camera input (delta movement).
        /// </summary>
        public Vector2 GetCameraInput()
        {
            return cameraInput;
        }
        
        /// <summary>
        /// Get vehicle acceleration input (0 to 1).
        /// </summary>
        public float GetVehicleAcceleration()
        {
            return vehicleAcceleration;
        }
        
        /// <summary>
        /// Get vehicle steering input (-1 to 1).
        /// </summary>
        public float GetVehicleSteering()
        {
            return vehicleSteering;
        }
        
        /// <summary>
        /// Get vehicle braking state.
        /// </summary>
        public bool GetVehicleBraking()
        {
            return vehicleBraking;
        }
        
        /// <summary>
        /// Enable or disable vehicle controls.
        /// </summary>
        public void SetVehicleControlsActive(bool active)
        {
            vehicleControlsActive = active;
            
            if (vehicleSteeringWheel != null)
                vehicleSteeringWheel.gameObject.SetActive(active);
            if (vehicleAccelerator != null)
                vehicleAccelerator.gameObject.SetActive(active);
            if (vehicleBrake != null)
                vehicleBrake.gameObject.SetActive(active);
        }
        
        /// <summary>
        /// Enable or disable build mode UI.
        /// </summary>
        public void SetBuildModeActive(bool active)
        {
            buildModeActive = active;
            
            if (buildModeUI != null)
                buildModeUI.SetActive(active);
        }
        
        /// <summary>
        /// Set camera sensitivity.
        /// </summary>
        public void SetCameraSensitivity(float sensitivity)
        {
            cameraSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
        }
        
        private void OnDestroy()
        {
            Core.ServiceLocator.Unregister<MobileInputManager>();
        }
    }
}
