using UnityEngine;
using UnityEngine.SceneManagement;

namespace VoxelSandbox.Core
{
    /// <summary>
    /// Bootstrap initialization script.
    /// Initializes all core services in the correct order.
    /// This should be the first scene loaded.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "MainMenu";

        private void Awake()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            // 1. Initialize Logger first (so all services can log)
            Logger.Initialize();
            Logger.Info("=== VoxelSandbox Bootstrap Started ===");
            Logger.Info($"Version: Pre-Alpha v0.1.0");

            // 2. Initialize Localization
            LocalizationService localization = new LocalizationService();
            localization.Initialize();
            ServiceLocator.Register(localization);

            // 3. Initialize Settings (load user preferences)
            SettingsService settings = new SettingsService();
            settings.Initialize();
            ServiceLocator.Register(settings);

            // 4. Apply loaded settings to localization
            localization.SetLanguage(settings.Settings.language);

            // 5. Log completion
            Logger.Info("=== All services initialized successfully ===");

            // 6. Transition to next scene
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Logger.Info($"Loading scene: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Logger.Error("Next scene name is not set in Bootstrap!");
            }
        }

        private void OnApplicationQuit()
        {
            Logger.Info("Application shutting down");
            Logger.Shutdown();
        }
    }
}
