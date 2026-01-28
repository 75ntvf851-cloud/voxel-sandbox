using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSandbox.Core
{
    /// <summary>
    /// Centralized localization service supporting multiple languages with runtime switching.
    /// Default language: Russian. Supports English.
    /// </summary>
    public class LocalizationService
    {
        public enum Language
        {
            Russian,
            English
        }

        private Dictionary<string, string> _currentStrings = new Dictionary<string, string>();
        private Language _currentLanguage = Language.Russian;

        public event Action OnLanguageChanged;

        public Language CurrentLanguage => _currentLanguage;

        public void Initialize()
        {
            LoadLanguage(Language.Russian);
            Logger.Info($"LocalizationService initialized with language: {_currentLanguage}");
        }

        public void SetLanguage(Language language)
        {
            if (_currentLanguage == language) return;

            _currentLanguage = language;
            LoadLanguage(language);
            OnLanguageChanged?.Invoke();

            Logger.Info($"Language changed to: {language}");
        }

        public string GetString(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                Logger.Warning("LocalizationService: Empty key requested");
                return "[EMPTY_KEY]";
            }

            if (_currentStrings.TryGetValue(key, out string value))
            {
                try
                {
                    return args != null && args.Length > 0 ? string.Format(value, args) : value;
                }
                catch (Exception e)
                {
                    Logger.Error($"LocalizationService: Format error for key '{key}': {e.Message}");
                    return value;
                }
            }

            Logger.Warning($"LocalizationService: Missing key '{key}' for language {_currentLanguage}");
            return $"[{key}]";
        }

        private void LoadLanguage(Language language)
        {
            _currentStrings.Clear();

            string fileName = language == Language.Russian ? "RU" : "EN";
            string path = $"_Project/Localization/{fileName}";

            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset == null)
            {
                Logger.Error($"LocalizationService: Failed to load language file at Resources/{path}");
                return;
            }

            try
            {
                var data = JsonUtility.FromJson<LocalizationData>($"{{\"entries\":{textAsset.text}}}");
                
                // Parse JSON manually since Unity's JsonUtility doesn't support dictionaries directly
                var jsonDict = MiniJSON.Json.Deserialize(textAsset.text) as Dictionary<string, object>;
                
                if (jsonDict != null)
                {
                    foreach (var kvp in jsonDict)
                    {
                        // Skip comment keys
                        if (kvp.Key.StartsWith("_comment")) continue;
                        
                        _currentStrings[kvp.Key] = kvp.Value?.ToString() ?? "";
                    }
                }

                Logger.Info($"LocalizationService: Loaded {_currentStrings.Count} keys for {language}");
            }
            catch (Exception e)
            {
                Logger.Error($"LocalizationService: Failed to parse language file: {e.Message}");
            }
        }

        [Serializable]
        private class LocalizationData
        {
            public Dictionary<string, string> entries;
        }
    }
}
