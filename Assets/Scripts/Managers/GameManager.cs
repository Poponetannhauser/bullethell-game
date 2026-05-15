using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using BulletHell.Gameplay;
using BulletHell.UI;
using BulletHell.Player;
using BulletHell.Core;
using System.Collections;

namespace BulletHell.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("UI Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("Boss Settings")]
        [SerializeField] private string bossPoolKey = "EnemyBoss";

        public float survivalTime { get; private set; }
        public int currentScore { get; private set; }
        private bool isGameActive = true;
        private bool isBossPhaseTriggered;

        public event System.Action<int, int> OnScoreChanged;

        private SpawnManager spawnManager;
        private PlayerController player;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();

            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                player.OnPlayerDeath += HandleGameOver;
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            survivalTime = 0f;
            currentScore = 0;
            isGameActive = true;
            isBossPhaseTriggered = false;

            OnScoreChanged?.Invoke(currentScore, 0);
        }

        void Update()
        {
            if (!isGameActive) return;

            survivalTime += Time.deltaTime;

            // Boss spawns at the 5-minute mark
            if (!isBossPhaseTriggered && survivalTime >= 300f)
            {
                isBossPhaseTriggered = true;
                StartCoroutine(TransitionToBossPhaseRoutine());
            }

            // Dev shortcut: skip 60 seconds
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                survivalTime += 60f;
                Debug.Log($"[Dev] Time skipped to {survivalTime:F1}s");
            }
        }

        // Clears the arena of regular enemies, then spawns the boss
        private IEnumerator TransitionToBossPhaseRoutine()
        {
            if (spawnManager != null) spawnManager.StopSpawning();

            PooledObject[] allActive = FindObjectsByType<PooledObject>(FindObjectsSortMode.None);
            foreach (var obj in allActive)
            {
                if (obj != null && obj.gameObject.activeInHierarchy && obj.gameObject.CompareTag("Enemy"))
                {
                    PoolManager.Instance.ReturnToPool(obj.gameObject);
                    yield return new WaitForSeconds(0.2f);
                }
            }

            yield return new WaitForSeconds(1.5f);

            Vector3 bossSpawnPos = new Vector3(0, 10f, 0);
            if (Camera.main != null)
            {
                float screenHeight = Camera.main.orthographicSize;
                bossSpawnPos = new Vector3(0, screenHeight + 3f, 0);
            }

            PoolManager.Instance.GetPooledObject(bossPoolKey, bossSpawnPos, Quaternion.identity);
        }

        private void OnDestroy()
        {
            if (player != null) player.OnPlayerDeath -= HandleGameOver;
        }

        private void HandleGameOver()
        {
            isGameActive = false;
            LeaderboardSystem.SaveScore(currentScore);

            if (spawnManager != null) spawnManager.StopSpawning();
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        // Called by EnemyBoss when defeated
        public void HandleVictory()
        {
            isGameActive = false;
            LeaderboardSystem.SaveScore(currentScore);

            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        // Called by UI restart button
        public void RestartGame()
        {
            PoolManager.Instance.ClearAllActiveObjects();
            if (player != null) player.ResetPlayer();
            if (spawnManager != null) spawnManager.StartSpawning();
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            survivalTime = 0f;
            currentScore = 0;
            isGameActive = true;
            isBossPhaseTriggered = false;
            Time.timeScale = 1f;

            OnScoreChanged?.Invoke(currentScore, 0);
        }

        public void BackToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void AddScore(int amount, Vector3 position)
        {
            if (!isGameActive) return;

            currentScore += amount;
            OnScoreChanged?.Invoke(currentScore, 0);

            GameObject popup = PoolManager.Instance.GetPooledObject("ScorePopup", position, Quaternion.identity);
            if (popup != null)
            {
                var fs = popup.GetComponent<FloatingScore>();
                if (fs != null) fs.Setup(amount);
            }
        }
    }
}
