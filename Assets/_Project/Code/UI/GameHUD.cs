using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VoxelSandbox.UI
{
    /// <summary>
    /// AAA-quality HUD system for mobile with energy, water, inventory, and status displays.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Hotbar")]
        [SerializeField] private Transform hotbarContainer;
        [SerializeField] private GameObject hotbarSlotPrefab;
        [SerializeField] private int hotbarSize = 9;
        private Image[] hotbarSlots;
        
        [Header("Resource Bars")]
        [SerializeField] private Slider energyBar;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private Slider waterBar;
        [SerializeField] private TextMeshProUGUI waterText;
        
        [Header("Status")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI coordinatesText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private GameObject vehicleIndicator;
        [SerializeField] private GameObject buildModeIndicator;
        
        [Header("Notifications")]
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 3f;
        private float notificationTimer = 0f;
        
        [Header("Mini Map")]
        [SerializeField] private RawImage miniMapImage;
        [SerializeField] private Transform miniMapPlayerMarker;
        
        [Header("Quest/Objective")]
        [SerializeField] private GameObject questPanel;
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private Slider questProgressBar;
        
        [Header("Crosshair")]
        [SerializeField] private Image crosshair;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color interactableColor = Color.green;
        [SerializeField] private Color invalidColor = Color.red;
        
        private Core.LocalizationService localization;
        private bool isInitialized = false;
        
        private void Awake()
        {
            InitializeHotbar();
        }
        
        private void Start()
        {
            localization = Core.ServiceLocator.Get<Core.LocalizationService>();
            isInitialized = true;
            
            // Hide quest panel initially
            if (questPanel != null)
                questPanel.SetActive(false);
                
            // Hide vehicle indicator initially
            if (vehicleIndicator != null)
                vehicleIndicator.SetActive(false);
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            UpdateNotification();
        }
        
        private void InitializeHotbar()
        {
            if (hotbarContainer == null || hotbarSlotPrefab == null) return;
            
            hotbarSlots = new Image[hotbarSize];
            
            for (int i = 0; i < hotbarSize; i++)
            {
                GameObject slot = Instantiate(hotbarSlotPrefab, hotbarContainer);
                hotbarSlots[i] = slot.GetComponent<Image>();
                
                // Add slot number text
                TextMeshProUGUI slotNumber = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (slotNumber != null)
                {
                    slotNumber.text = (i + 1).ToString();
                }
            }
        }
        
        /// <summary>
        /// Update energy display.
        /// </summary>
        public void UpdateEnergy(float current, float max)
        {
            if (energyBar != null)
            {
                energyBar.value = max > 0 ? current / max : 0;
            }
            
            if (energyText != null)
            {
                energyText.text = $"{current:F0} / {max:F0} kW";
            }
        }
        
        /// <summary>
        /// Update water display.
        /// </summary>
        public void UpdateWater(float current, float max)
        {
            if (waterBar != null)
            {
                waterBar.value = max > 0 ? current / max : 0;
            }
            
            if (waterText != null)
            {
                waterText.text = $"{current:F0} / {max:F0} L";
            }
        }
        
        /// <summary>
        /// Update speed display (for vehicle).
        /// </summary>
        public void UpdateSpeed(float speedKmh)
        {
            if (speedText != null)
            {
                speedText.text = $"{speedKmh:F0} km/h";
                speedText.gameObject.SetActive(speedKmh > 0.1f);
            }
        }
        
        /// <summary>
        /// Update coordinates display.
        /// </summary>
        public void UpdateCoordinates(Vector3 position)
        {
            if (coordinatesText != null)
            {
                coordinatesText.text = $"X: {position.x:F0}  Y: {position.y:F0}  Z: {position.z:F0}";
            }
        }
        
        /// <summary>
        /// Update time display.
        /// </summary>
        public void UpdateTime(float timeInHours)
        {
            if (timeText != null)
            {
                int hours = Mathf.FloorToInt(timeInHours);
                int minutes = Mathf.FloorToInt((timeInHours - hours) * 60);
                timeText.text = $"{hours:D2}:{minutes:D2}";
            }
        }
        
        /// <summary>
        /// Show notification message.
        /// </summary>
        public void ShowNotification(string message)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
                notificationText.gameObject.SetActive(true);
                notificationTimer = notificationDuration;
            }
        }
        
        /// <summary>
        /// Show localized notification.
        /// </summary>
        public void ShowNotificationLocalized(string key, params string[] args)
        {
            if (localization != null)
            {
                string message = localization.GetString(key, args);
                ShowNotification(message);
            }
        }
        
        private void UpdateNotification()
        {
            if (notificationTimer > 0)
            {
                notificationTimer -= Time.deltaTime;
                
                if (notificationTimer <= 0 && notificationText != null)
                {
                    notificationText.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Update hotbar slot with item icon.
        /// </summary>
        public void UpdateHotbarSlot(int index, Sprite icon)
        {
            if (index >= 0 && index < hotbarSlots.Length && hotbarSlots[index] != null)
            {
                hotbarSlots[index].sprite = icon;
                hotbarSlots[index].enabled = icon != null;
            }
        }
        
        /// <summary>
        /// Highlight selected hotbar slot.
        /// </summary>
        public void SelectHotbarSlot(int index)
        {
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null)
                {
                    // Add outline or scale effect
                    hotbarSlots[i].transform.localScale = (i == index) ? Vector3.one * 1.1f : Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// Show or hide vehicle indicator.
        /// </summary>
        public void SetVehicleMode(bool inVehicle)
        {
            if (vehicleIndicator != null)
            {
                vehicleIndicator.SetActive(inVehicle);
            }
        }
        
        /// <summary>
        /// Show or hide build mode indicator.
        /// </summary>
        public void SetBuildMode(bool inBuildMode)
        {
            if (buildModeIndicator != null)
            {
                buildModeIndicator.SetActive(inBuildMode);
            }
        }
        
        /// <summary>
        /// Update crosshair color based on target.
        /// </summary>
        public void SetCrosshairState(CrosshairState state)
        {
            if (crosshair == null) return;
            
            switch (state)
            {
                case CrosshairState.Normal:
                    crosshair.color = normalColor;
                    break;
                case CrosshairState.Interactable:
                    crosshair.color = interactableColor;
                    break;
                case CrosshairState.Invalid:
                    crosshair.color = invalidColor;
                    break;
            }
        }
        
        /// <summary>
        /// Show quest/objective panel.
        /// </summary>
        public void ShowQuest(string title, string description, float progress = 0f)
        {
            if (questPanel == null) return;
            
            questPanel.SetActive(true);
            
            if (questTitleText != null)
                questTitleText.text = title;
                
            if (questDescriptionText != null)
                questDescriptionText.text = description;
                
            if (questProgressBar != null)
                questProgressBar.value = Mathf.Clamp01(progress);
        }
        
        /// <summary>
        /// Hide quest panel.
        /// </summary>
        public void HideQuest()
        {
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Update quest progress.
        /// </summary>
        public void UpdateQuestProgress(float progress)
        {
            if (questProgressBar != null)
            {
                questProgressBar.value = Mathf.Clamp01(progress);
            }
        }
    }
    
    /// <summary>
    /// Crosshair states for different interaction contexts.
    /// </summary>
    public enum CrosshairState
    {
        Normal,
        Interactable,
        Invalid
    }
}
