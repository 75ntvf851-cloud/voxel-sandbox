using UnityEngine;
using UnityEngine.UI;
using VoxelSandbox.Core;

namespace VoxelSandbox.UI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles New Game, Load Game, Settings, and Exit buttons.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "Main";

        private void Start()
        {
            if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
            if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (exitButton != null) exitButton.onClick.AddListener(OnExit);

            ShowMainMenu();
            
            Logger.Info("MainMenuUI initialized");
        }

        private void OnNewGame()
        {
            Logger.Info("New Game button clicked");
            // TODO: Load game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }

        private void OnLoadGame()
        {
            Logger.Info("Load Game button clicked");
            // TODO: Show load game menu
            Logger.Warning("Load Game not yet implemented");
        }

        private void OnSettings()
        {
            Logger.Info("Settings button clicked");
            ShowSettings();
        }

        private void OnExit()
        {
            Logger.Info("Exit button clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void ShowSettings()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnBackFromSettings()
        {
            ShowMainMenu();
        }
    }
}
