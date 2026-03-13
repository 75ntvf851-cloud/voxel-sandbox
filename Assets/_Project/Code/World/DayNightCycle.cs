using UnityEngine;
using Unity.Profiling;

namespace VoxelSandbox.World
{
    /// <summary>
    /// Day/night cycle system with dynamic lighting for mobile.
    /// Optimized for URP with smooth transitions.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float dayDuration = 600f; // 10 minutes per full day
        [SerializeField] private float currentTime = 0.3f; // Start at dawn (0-1 range)
        [SerializeField] private bool autoProgress = true;
        
        [Header("Sun")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Gradient sunColorGradient;
        [SerializeField] private AnimationCurve sunIntensityCurve;
        
        [Header("Moon")]
        [SerializeField] private Light moonLight;
        [SerializeField] private Transform moonTransform;
        [SerializeField] private Color moonColor = new Color(0.5f, 0.6f, 0.8f);
        [SerializeField] private float moonIntensity = 0.3f;
        
        [Header("Ambient")]
        [SerializeField] private Gradient ambientColorGradient;
        [SerializeField] private AnimationCurve ambientIntensityCurve;
        
        [Header("Skybox")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private string skyboxExposureProperty = "_Exposure";
        [SerializeField] private AnimationCurve skyboxExposureCurve;
        
        [Header("Fog")]
        [SerializeField] private bool useFog = true;
        [SerializeField] private Gradient fogColorGradient;
        [SerializeField] private AnimationCurve fogDensityCurve;
        
        private static readonly ProfilerMarker s_UpdateCycleMarker = new ProfilerMarker("DayNightCycle.UpdateCycle");
        
        private void Awake()
        {
            InitializeGradients();
            InitializeCurves();
        }
        
        private void Start()
        {
            // Configure lights for mobile optimization
            if (sunLight != null)
            {
                sunLight.shadows = LightShadows.Soft;
                sunLight.shadowStrength = 0.7f;
                sunLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
            }
            
            if (moonLight != null)
            {
                moonLight.shadows = LightShadows.None; // No moon shadows for performance
                moonLight.color = moonColor;
            }
            
            // Initial update
            UpdateCycle(0);
        }
        
        private void Update()
        {
            if (autoProgress)
            {
                // Progress time
                currentTime += (Time.deltaTime / dayDuration);
                if (currentTime >= 1f)
                {
                    currentTime = 0f;
                }
            }
            
            UpdateCycle(Time.deltaTime);
        }
        
        private void UpdateCycle(float deltaTime)
        {
            using (s_UpdateCycleMarker.Auto())
            {
                // Update sun rotation
                if (sunTransform != null)
                {
                    float sunAngle = currentTime * 360f - 90f; // -90 to start at horizon
                    sunTransform.rotation = Quaternion.Euler(sunAngle, 170f, 0);
                }
                
                // Update moon rotation (opposite to sun)
                if (moonTransform != null)
                {
                    float moonAngle = (currentTime + 0.5f) * 360f - 90f;
                    moonTransform.rotation = Quaternion.Euler(moonAngle, 170f, 0);
                }
                
                // Update sun light
                if (sunLight != null)
                {
                    sunLight.color = sunColorGradient.Evaluate(currentTime);
                    sunLight.intensity = sunIntensityCurve.Evaluate(currentTime);
                }
                
                // Update moon light
                if (moonLight != null)
                {
                    // Moon is only visible at night
                    bool isNight = currentTime > 0.75f || currentTime < 0.25f;
                    moonLight.enabled = isNight;
                    
                    if (isNight)
                    {
                        float nightTime = currentTime > 0.75f ? (currentTime - 0.75f) * 4f : (currentTime + 0.25f) * 4f;
                        moonLight.intensity = Mathf.Sin(nightTime * Mathf.PI) * moonIntensity;
                    }
                }
                
                // Update ambient lighting
                Color ambientColor = ambientColorGradient.Evaluate(currentTime);
                float ambientIntensity = ambientIntensityCurve.Evaluate(currentTime);
                RenderSettings.ambientLight = ambientColor * ambientIntensity;
                
                // Update skybox
                if (skyboxMaterial != null && skyboxMaterial.HasProperty(skyboxExposureProperty))
                {
                    float exposure = skyboxExposureCurve.Evaluate(currentTime);
                    skyboxMaterial.SetFloat(skyboxExposureProperty, exposure);
                }
                
                // Update fog
                if (useFog)
                {
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = fogColorGradient.Evaluate(currentTime);
                    RenderSettings.fogDensity = fogDensityCurve.Evaluate(currentTime);
                }
            }
        }
        
        /// <summary>
        /// Set time of day (0-1 range, 0 = midnight, 0.5 = noon).
        /// </summary>
        public void SetTime(float time)
        {
            currentTime = Mathf.Clamp01(time);
            UpdateCycle(0);
        }
        
        /// <summary>
        /// Get current time of day (0-1 range).
        /// </summary>
        public float GetTime()
        {
            return currentTime;
        }
        
        /// <summary>
        /// Get current time in hours (0-24).
        /// </summary>
        public float GetTimeInHours()
        {
            return currentTime * 24f;
        }
        
        /// <summary>
        /// Set whether time automatically progresses.
        /// </summary>
        public void SetAutoProgress(bool auto)
        {
            autoProgress = auto;
        }
        
        /// <summary>
        /// Set day duration in seconds.
        /// </summary>
        public void SetDayDuration(float duration)
        {
            dayDuration = Mathf.Max(10f, duration);
        }
        
        /// <summary>
        /// Check if it's currently day time.
        /// </summary>
        public bool IsDay()
        {
            return currentTime >= 0.25f && currentTime <= 0.75f;
        }
        
        /// <summary>
        /// Check if it's currently night time.
        /// </summary>
        public bool IsNight()
        {
            return currentTime > 0.75f || currentTime < 0.25f;
        }
        
        private void InitializeGradients()
        {
            // Initialize sun color gradient if not set
            if (sunColorGradient == null || sunColorGradient.colorKeys.Length == 0)
            {
                sunColorGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.0f);   // Midnight - dark blue
                colorKeys[1] = new GradientColorKey(new Color(1.0f, 0.6f, 0.3f), 0.25f);  // Sunrise - orange
                colorKeys[2] = new GradientColorKey(new Color(1.0f, 1.0f, 0.9f), 0.5f);   // Noon - bright white
                colorKeys[3] = new GradientColorKey(new Color(1.0f, 0.5f, 0.2f), 0.75f);  // Sunset - red-orange
                colorKeys[4] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1.0f);   // Midnight - dark blue
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
                
                sunColorGradient.SetKeys(colorKeys, alphaKeys);
            }
            
            // Initialize ambient color gradient
            if (ambientColorGradient == null || ambientColorGradient.colorKeys.Length == 0)
            {
                ambientColorGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.2f, 0.3f), 0.0f);
                colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.6f, 0.5f), 0.25f);
                colorKeys[2] = new GradientColorKey(new Color(0.7f, 0.8f, 1.0f), 0.5f);
                colorKeys[3] = new GradientColorKey(new Color(0.8f, 0.5f, 0.4f), 0.75f);
                colorKeys[4] = new GradientColorKey(new Color(0.2f, 0.2f, 0.3f), 1.0f);
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
                
                ambientColorGradient.SetKeys(colorKeys, alphaKeys);
            }
            
            // Initialize fog color gradient
            if (fogColorGradient == null || fogColorGradient.colorKeys.Length == 0)
            {
                fogColorGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.15f), 0.0f);
                colorKeys[1] = new GradientColorKey(new Color(0.9f, 0.7f, 0.6f), 0.25f);
                colorKeys[2] = new GradientColorKey(new Color(0.6f, 0.7f, 0.9f), 0.5f);
                colorKeys[3] = new GradientColorKey(new Color(0.9f, 0.6f, 0.5f), 0.75f);
                colorKeys[4] = new GradientColorKey(new Color(0.1f, 0.1f, 0.15f), 1.0f);
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
                
                fogColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
        
        private void InitializeCurves()
        {
            // Initialize sun intensity curve if not set
            if (sunIntensityCurve == null || sunIntensityCurve.keys.Length == 0)
            {
                sunIntensityCurve = new AnimationCurve();
                sunIntensityCurve.AddKey(0.0f, 0.0f);    // Midnight - no sun
                sunIntensityCurve.AddKey(0.25f, 0.8f);   // Sunrise
                sunIntensityCurve.AddKey(0.5f, 1.2f);    // Noon - brightest
                sunIntensityCurve.AddKey(0.75f, 0.8f);   // Sunset
                sunIntensityCurve.AddKey(1.0f, 0.0f);    // Midnight - no sun
            }
            
            // Initialize ambient intensity curve
            if (ambientIntensityCurve == null || ambientIntensityCurve.keys.Length == 0)
            {
                ambientIntensityCurve = new AnimationCurve();
                ambientIntensityCurve.AddKey(0.0f, 0.3f);
                ambientIntensityCurve.AddKey(0.25f, 0.7f);
                ambientIntensityCurve.AddKey(0.5f, 1.0f);
                ambientIntensityCurve.AddKey(0.75f, 0.7f);
                ambientIntensityCurve.AddKey(1.0f, 0.3f);
            }
            
            // Initialize skybox exposure curve
            if (skyboxExposureCurve == null || skyboxExposureCurve.keys.Length == 0)
            {
                skyboxExposureCurve = new AnimationCurve();
                skyboxExposureCurve.AddKey(0.0f, 0.3f);
                skyboxExposureCurve.AddKey(0.25f, 0.9f);
                skyboxExposureCurve.AddKey(0.5f, 1.2f);
                skyboxExposureCurve.AddKey(0.75f, 0.9f);
                skyboxExposureCurve.AddKey(1.0f, 0.3f);
            }
            
            // Initialize fog density curve
            if (fogDensityCurve == null || fogDensityCurve.keys.Length == 0)
            {
                fogDensityCurve = new AnimationCurve();
                fogDensityCurve.AddKey(0.0f, 0.015f);   // Night fog
                fogDensityCurve.AddKey(0.25f, 0.010f);  // Morning mist
                fogDensityCurve.AddKey(0.5f, 0.005f);   // Clear day
                fogDensityCurve.AddKey(0.75f, 0.012f);  // Evening haze
                fogDensityCurve.AddKey(1.0f, 0.015f);   // Night fog
            }
        }
    }
}
