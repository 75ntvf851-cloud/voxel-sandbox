using UnityEngine;
using TMPro;
using VoxelSandbox.Core;

namespace VoxelSandbox.UI
{
    /// <summary>
    /// Debug overlay showing FPS, version, mode, and other debug info.
    /// Toggle with F3 key.
    /// </summary>
    public class DebugOverlayUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI memoryText;

        [Header("Settings")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;

        private LocalizationService _localization;
        private float _deltaTime = 0f;
        private float _updateTimer = 0f;
        private bool _isVisible = false;

        private void Start()
        {
            _localization = ServiceLocator.Get<LocalizationService>();
            
            if (_localization != null)
            {
                _localization.OnLanguageChanged += UpdateStaticTexts;
            }

            SetVisible(false);
            UpdateStaticTexts();
            
            Logger.Info("DebugOverlayUI initialized");
        }

        private void OnDestroy()
        {
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= UpdateStaticTexts;
            }
        }

        private void Update()
        {
            // Toggle visibility
            if (Input.GetKeyDown(toggleKey))
            {
                SetVisible(!_isVisible);
            }

            if (!_isVisible) return;

            // Calculate FPS
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

            // Update timer
            _updateTimer += Time.unscaledDeltaTime;
            
            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                UpdateDynamicTexts();
            }
        }

        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(visible);
            }

            Logger.Info($"Debug overlay: {(visible ? "shown" : "hidden")}");
        }

        private void UpdateStaticTexts()
        {
            if (_localization == null) return;

            if (versionText != null)
            {
                versionText.text = _localization.GetString("hud_version", "Pre-Alpha v0.1.0");
            }
        }

        private void UpdateDynamicTexts()
        {
            if (_localization == null) return;

            // FPS
            if (fpsText != null)
            {
                float fps = 1f / _deltaTime;
                fpsText.text = _localization.GetString("hud_fps", Mathf.Ceil(fps).ToString("F0"));
            }

            // Memory
            if (memoryText != null)
            {
                float memoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
                memoryText.text = _localization.GetString("debug_memory", memoryMB.ToString("F1"));
            }

            // Mode (placeholder - will be set by other systems)
            if (modeText != null)
            {
                modeText.text = _localization.GetString("hud_mode", "Normal");
            }

            // Position (if player exists)
            if (positionText != null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var pos = player.transform.position;
                    positionText.text = _localization.GetString("hud_position", 
                        pos.x.ToString("F1"), pos.y.ToString("F1"), pos.z.ToString("F1"));
                }
            }
        }

        public void SetMode(string mode)
        {
            if (modeText != null && _localization != null)
            {
                modeText.text = _localization.GetString("hud_mode", mode);
            }
        }
    }
}
