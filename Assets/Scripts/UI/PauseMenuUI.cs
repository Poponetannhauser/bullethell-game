using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

namespace BulletHell.UI
{
    // Handles pause/resume with Escape key, includes 3-second countdown on resume
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private TextMeshProUGUI countdownText;

        private bool isPaused;
        private bool isCountingDown;

        void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (countdownText != null) countdownText.gameObject.SetActive(false);
        }

        void Update()
        {
            if (isCountingDown) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        // Starts a 3-2-1-GO countdown before unpausing
        public void ResumeGame()
        {
            if (isCountingDown) return;
            StartCoroutine(ResumeCountdownRoutine());
        }

        private IEnumerator ResumeCountdownRoutine()
        {
            isCountingDown = true;

            if (pausePanel != null) pausePanel.SetActive(false);
            if (countdownText != null) countdownText.gameObject.SetActive(true);

            // Uses WaitForSecondsRealtime since Time.timeScale is 0
            for (int count = 3; count > 0; count--)
            {
                if (countdownText != null) countdownText.text = count.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }

            if (countdownText != null) countdownText.text = "GO!";
            yield return new WaitForSecondsRealtime(0.5f);

            if (countdownText != null) countdownText.gameObject.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
            isCountingDown = false;
        }

        public void BackToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
