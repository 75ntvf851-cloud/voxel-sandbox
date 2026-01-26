using UnityEngine;
using UnityEngine.UI;
using VoxelSandbox.Core;

namespace VoxelSandbox.UI
{
    /// <summary>
    /// Settings UI controller.
    /// Handles language toggle, mouse sensitivity, invert Y, and volume settings.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button languageToggleButton;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button backButton;

        private SettingsService _settingsService;
        private LocalizationService _localizationService;
        private SettingsData _tempSettings;

        private void Start()
        {
            _settingsService = ServiceLocator.Get<SettingsService>();
            _localizationService = ServiceLocator.Get<LocalizationService>();

            if (_settingsService == null || _localizationService == null)
            {
                Logger.Error("SettingsUI: Required services not found!");
                return;
            }

            if (languageToggleButton != null) languageToggleButton.onClick.AddListener(OnLanguageToggle);
            if (applyButton != null) applyButton.onClick.AddListener(OnApply);
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
            if (backButton != null) backButton.onClick.AddListener(OnBack);

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.minValue = 0.1f;
                mouseSensitivitySlider.maxValue = 10f;
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
            }

            LoadCurrentSettings();
            Logger.Info("SettingsUI initialized");
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            if (_settingsService == null) return;

            _tempSettings = new SettingsData
            {
                language = _settingsService.Settings.language,
                mouseSensitivity = _settingsService.Settings.mouseSensitivity,
                invertY = _settingsService.Settings.invertY,
                masterVolume = _settingsService.Settings.masterVolume
            };

            UpdateUIFromSettings();
        }

        private void UpdateUIFromSettings()
        {
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = _tempSettings.mouseSensitivity;

            if (invertYToggle != null)
                invertYToggle.isOn = _tempSettings.invertY;

            if (masterVolumeSlider != null)
                masterVolumeSlider.value = _tempSettings.masterVolume;
        }

        private void OnLanguageToggle()
        {
            var newLanguage = _tempSettings.language == LocalizationService.Language.Russian
                ? LocalizationService.Language.English
                : LocalizationService.Language.Russian;

            _tempSettings.language = newLanguage;
            
            // Apply language immediately (preview)
            _localizationService.SetLanguage(newLanguage);
            _settingsService.SetLanguage(newLanguage);
            
            Logger.Info($"Language toggled to: {newLanguage}");
        }

        private void OnApply()
        {
            if (mouseSensitivitySlider != null)
                _settingsService.SetMouseSensitivity(mouseSensitivitySlider.value);

            if (invertYToggle != null)
                _settingsService.SetInvertY(invertYToggle.isOn);

            if (masterVolumeSlider != null)
                _settingsService.SetMasterVolume(masterVolumeSlider.value);

            Logger.Info("Settings applied");
            OnBack();
        }

        private void OnCancel()
        {
            // Revert to saved settings
            _localizationService.SetLanguage(_settingsService.Settings.language);
            LoadCurrentSettings();
            Logger.Info("Settings cancelled");
            OnBack();
        }

        private void OnBack()
        {
            var mainMenu = FindObjectOfType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.OnBackFromSettings();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
