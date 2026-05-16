using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using BulletHell.Core;
using BulletHell.Managers;

namespace BulletHell.UI
{
    // Controls main menu navigation, mode selection, and hover descriptions
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Hover Description")]
        [SerializeField] private GameObject descriptionPanel;
        [SerializeField] private TextMeshProUGUI descriptionText;

        void Start()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (descriptionPanel != null) descriptionPanel.SetActive(false);
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        // Mode selection — sets GameSettings before loading game scene
        public void PlayClassicMode()
        {
            AudioManager.PlayUIClick();
            GameSettings.SelectedMode = GameMode.Classic;
            SceneManager.LoadScene(gameSceneName);
        }

        public void PlayOverheatMode()
        {
            AudioManager.PlayUIClick();
            GameSettings.SelectedMode = GameMode.Overheat;
            SceneManager.LoadScene(gameSceneName);
        }

        // Panel navigation — hides main buttons, shows sub-panel
        public void OpenLeaderboard()
        {
            AudioManager.PlayUIClick();
            if (mainPanel != null) mainPanel.SetActive(false);
            if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        }

        public void OpenSettings()
        {
            AudioManager.PlayUIClick();
            if (mainPanel != null) mainPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void BackToMain()
        {
            AudioManager.PlayUIClick();
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // Called by MenuButtonHover to show/hide mode descriptions
        public void ShowDescription(string text)
        {
            if (descriptionText != null) descriptionText.text = text;
            if (descriptionPanel != null) descriptionPanel.SetActive(true);
        }

        public void HideDescription()
        {
            if (descriptionPanel != null) descriptionPanel.SetActive(false);
        }
    }
}
