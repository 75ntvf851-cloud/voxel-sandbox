using UnityEngine;
using TMPro;
using VoxelSandbox.Core;

namespace VoxelSandbox.UI
{
    /// <summary>
    /// LocalizedText component that automatically updates when language changes.
    /// Attach to TextMeshProUGUI components and set the localization key.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string localizationKey;
        [SerializeField] private bool formatWithArguments = false;
        
        private TextMeshProUGUI _textComponent;
        private LocalizationService _localization;
        private object[] _formatArgs;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _localization = ServiceLocator.Get<LocalizationService>();
            
            if (_localization != null)
            {
                _localization.OnLanguageChanged += UpdateText;
                UpdateText();
            }
            else
            {
                Logger.Error("LocalizedText: LocalizationService not found!");
            }
        }

        private void OnDestroy()
        {
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= UpdateText;
            }
        }

        public void SetKey(string key, params object[] args)
        {
            localizationKey = key;
            _formatArgs = args;
            formatWithArguments = args != null && args.Length > 0;
            UpdateText();
        }

        private void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey))
            {
                Logger.Warning($"LocalizedText on {gameObject.name}: No localization key set!");
                return;
            }

            if (_textComponent == null) return;

            string localizedText;
            
            if (formatWithArguments && _formatArgs != null)
            {
                localizedText = _localization.GetString(localizationKey, _formatArgs);
            }
            else
            {
                localizedText = _localization.GetString(localizationKey);
            }

            _textComponent.text = localizedText;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(localizationKey) && Application.isPlaying)
            {
                UpdateText();
            }
        }
#endif
    }
}
