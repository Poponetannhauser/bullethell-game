using UnityEngine;
using TMPro;
using BulletHell.Managers;

namespace BulletHell.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText; // Menampilkan skor tertinggi persisten

        void Start()
        {
            // Berlangganan ke event score di GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            }
        }

        void Update()
        {
            if (GameManager.Instance != null && timerText != null)
            {
                // Format waktu: Menit:Detik
                float t = GameManager.Instance.survivalTime;
                string minutes = ((int)t / 60).ToString("00");
                string seconds = (t % 60).ToString("00");
                timerText.text = minutes + ":" + seconds;
            }
        }

        private void UpdateScoreDisplay(int currentScore, int highScore)
        {
            if (scoreText != null)
            {
                scoreText.text = "Score: " + currentScore.ToString("N0"); // Pakai pemisah ribuan
            }

            if (highScoreText != null)
            {
                highScoreText.text = "High Score: " + highScore.ToString("N0");
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            }
        }
    }
}
