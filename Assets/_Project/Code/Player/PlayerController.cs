using UnityEngine;
using UnityEngine.InputSystem;
using VoxelSandbox.Core;

namespace VoxelSandbox.Player
{
    /// <summary>
    /// First-person player controller with WASD movement, mouse look, jump, and fly mode.
    /// Uses New Input System.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float flySpeed = 10f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Look")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 90f;

        [Header("Input")]
        [SerializeField] private KeyCode flyToggleKey = KeyCode.F;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode languageToggleKey = KeyCode.F9;

        private CharacterController _controller;
        private SettingsService _settings;
        private LocalizationService _localization;
        
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _jumpInput;
        
        private Vector3 _velocity;
        private float _pitch = 0f;
        private bool _isFlying = false;
        private bool _isPaused = false;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            _settings = ServiceLocator.Get<SettingsService>();
            _localization = ServiceLocator.Get<LocalizationService>();
            
            if (_settings != null)
            {
                lookSensitivity = _settings.Settings.mouseSensitivity;
                _settings.OnSettingsChanged += OnSettingsChanged;
            }

            Logger.Info("PlayerController initialized");
        }

        private void OnDestroy()
        {
            if (_settings != null)
            {
                _settings.OnSettingsChanged -= OnSettingsChanged;
            }
        }

        private void Update()
        {
            HandleInput();
            
            if (_isPaused) return;

            HandleMovement();
            HandleLook();
        }

        private void HandleInput()
        {
            // Movement (WASD)
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(horizontal, vertical);

            // Look (Mouse)
            _lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Jump
            _jumpInput = Input.GetButtonDown("Jump");

            // Fly toggle
            if (Input.GetKeyDown(flyToggleKey))
            {
                _isFlying = !_isFlying;
                Logger.Info($"Fly mode: {(_isFlying ? "enabled" : "disabled")}");
            }

            // Pause toggle
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }

            // Language toggle
            if (Input.GetKeyDown(languageToggleKey))
            {
                if (_localization != null)
                {
                    var newLang = _localization.CurrentLanguage == LocalizationService.Language.Russian
                        ? LocalizationService.Language.English
                        : LocalizationService.Language.Russian;
                    
                    _localization.SetLanguage(newLang);
                    
                    if (_settings != null)
                    {
                        _settings.SetLanguage(newLang);
                    }
                }
            }
        }

        private void HandleMovement()
        {
            float currentSpeed = _isFlying ? flySpeed : walkSpeed;
            
            // Calculate move direction relative to camera
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            moveDirection.Normalize();

            if (_isFlying)
            {
                // Fly mode - free movement in all directions
                Vector3 flyVelocity = moveDirection * currentSpeed;
                
                // Vertical movement
                if (Input.GetKey(KeyCode.Space))
                {
                    flyVelocity.y = currentSpeed;
                }
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    flyVelocity.y = -currentSpeed;
                }

                _controller.Move(flyVelocity * Time.deltaTime);
            }
            else
            {
                // Ground mode - apply gravity
                if (_controller.isGrounded && _velocity.y < 0)
                {
                    _velocity.y = -2f;
                }

                Vector3 move = moveDirection * currentSpeed;

                // Jump
                if (_jumpInput && _controller.isGrounded)
                {
                    _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                // Apply gravity
                _velocity.y += gravity * Time.deltaTime;

                move.y = _velocity.y;

                _controller.Move(move * Time.deltaTime);
            }
        }

        private void HandleLook()
        {
            if (cameraTransform == null) return;

            float mouseSensitivity = _settings != null ? _settings.Settings.mouseSensitivity : lookSensitivity;
            bool invertY = _settings != null && _settings.Settings.invertY;

            float yaw = _lookInput.x * mouseSensitivity;
            float pitch = _lookInput.y * mouseSensitivity * (invertY ? 1f : -1f);

            transform.Rotate(Vector3.up * yaw);

            _pitch += pitch;
            _pitch = Mathf.Clamp(_pitch, -maxLookAngle, maxLookAngle);
            
            cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            
            Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _isPaused;
            
            // TODO: Show pause menu
            Logger.Info($"Pause: {_isPaused}");
        }

        private void OnSettingsChanged()
        {
            if (_settings != null)
            {
                lookSensitivity = _settings.Settings.mouseSensitivity;
            }
        }

        public bool IsFlying => _isFlying;
        public bool IsPaused => _isPaused;
    }
}
