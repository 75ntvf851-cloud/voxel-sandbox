using System;
using UnityEngine;

namespace VoxelSandbox.Core
{
    /// <summary>
    /// Settings service for persistent user preferences.
    /// Stored in PlayerPrefs initially, behind a clean interface for future migration.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        public LocalizationService.Language language = LocalizationService.Language.Russian;
        public float mouseSensitivity = 2.0f;
        public bool invertY = false;
        public float masterVolume = 1.0f;
    }

    public class SettingsService
    {
        private SettingsData _settings;
        public event Action OnSettingsChanged;

        public SettingsData Settings => _settings;

        public void Initialize()
        {
            Load();
            Logger.Info("SettingsService initialized");
        }

        public void Load()
        {
            _settings = new SettingsData();

            if (PlayerPrefs.HasKey("Settings_Language"))
            {
                _settings.language = (LocalizationService.Language)PlayerPrefs.GetInt("Settings_Language");
            }

            if (PlayerPrefs.HasKey("Settings_MouseSensitivity"))
            {
                _settings.mouseSensitivity = PlayerPrefs.GetFloat("Settings_MouseSensitivity");
            }

            if (PlayerPrefs.HasKey("Settings_InvertY"))
            {
                _settings.invertY = PlayerPrefs.GetInt("Settings_InvertY") == 1;
            }

            if (PlayerPrefs.HasKey("Settings_MasterVolume"))
            {
                _settings.masterVolume = PlayerPrefs.GetFloat("Settings_MasterVolume");
            }

            Logger.Info($"Settings loaded: Language={_settings.language}, MouseSens={_settings.mouseSensitivity}");
        }

        public void Save()
        {
            PlayerPrefs.SetInt("Settings_Language", (int)_settings.language);
            PlayerPrefs.SetFloat("Settings_MouseSensitivity", _settings.mouseSensitivity);
            PlayerPrefs.SetInt("Settings_InvertY", _settings.invertY ? 1 : 0);
            PlayerPrefs.SetFloat("Settings_MasterVolume", _settings.masterVolume);
            PlayerPrefs.Save();

            OnSettingsChanged?.Invoke();
            Logger.Info("Settings saved");
        }

        public void SetLanguage(LocalizationService.Language language)
        {
            _settings.language = language;
            Save();
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            _settings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
            Save();
        }

        public void SetInvertY(bool invert)
        {
            _settings.invertY = invert;
            Save();
        }

        public void SetMasterVolume(float volume)
        {
            _settings.masterVolume = Mathf.Clamp01(volume);
            Save();
        }

        public void ResetToDefaults()
        {
            _settings = new SettingsData();
            Save();
            Logger.Info("Settings reset to defaults");
        }
    }
}
