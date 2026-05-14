using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using BulletHell.Core;

namespace BulletHell.UI
{
    /// <summary>
    /// Pengontrol utama Main Menu. Menangani navigasi tombol, deskripsi hover,
    /// dan transisi ke scene permainan.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("UI References - Descriptions")]
        [SerializeField] private GameObject descriptionPanel;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("UI References - Sub Panels")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject settingsPanel;

        void Start()
        {
            if (descriptionPanel != null) descriptionPanel.SetActive(false);
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        // ═══════════════════════════════════════════════
        //  GAME MODE BUTTONS
        // ═══════════════════════════════════════════════

        public void PlayClassicMode()
        {
            GameSettings.SelectedMode = GameMode.Classic;
            SceneManager.LoadScene(gameSceneName);
        }

        public void PlayOverheatMode()
        {
            GameSettings.SelectedMode = GameMode.Overheat;
            SceneManager.LoadScene(gameSceneName);
        }

        // ═══════════════════════════════════════════════
        //  NAVIGATION BUTTONS
        // ═══════════════════════════════════════════════

        public void OpenLeaderboard()
        {
            CloseAllPanels();
            if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        }

        public void OpenSettings()
        {
            CloseAllPanels();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void CloseAllPanels()
        {
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void QuitGame()
        {
            Debug.Log("Exiting game...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // ═══════════════════════════════════════════════
        //  HOVER DESCRIPTION SYSTEM
        // ═══════════════════════════════════════════════

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
