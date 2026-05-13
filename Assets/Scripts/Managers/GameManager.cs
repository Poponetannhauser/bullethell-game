using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using BulletHell.Gameplay;
using BulletHell.UI;
using BulletHell.Player;
using System.Collections; // Tambahkan ini untuk Coroutine

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
        public int highScore { get; private set; }
        private bool isGameActive = true;
        private bool isBossPhaseTriggered = false;

        // Event untuk UI mengirimkan (currentScore, highScore)
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
            
            // Cari player dan subscribe ke event kematian
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                player.OnPlayerDeath += HandleGameOver;
            }

            // Pastikan panel UI nonaktif saat dimulainya permainan
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            // Muat skor tertinggi persisten (Save System Plus Point)
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            survivalTime = 0f;
            currentScore = 0;
            isGameActive = true;
            isBossPhaseTriggered = false;
            
            OnScoreChanged?.Invoke(currentScore, highScore);
        }

        void Update()
        {
            if (!isGameActive) return;

            // Menambah waktu bertahan hidup setiap frame
            survivalTime += Time.deltaTime;

            // PEMICU BOSS: Saat mencapai Menit ke-5 (300 detik)
            if (!isBossPhaseTriggered && survivalTime >= 300f)
            {
                isBossPhaseTriggered = true;
                StartCoroutine(TransitionToBossPhaseRoutine());
            }

            // DEV CHEAT: Cara baru mendeteksi tombol F1 di New Input System
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                survivalTime += 60f;
                Debug.Log($"[Dev Cheat] Melompati Waktu! Waktu saat ini: {survivalTime:F1} detik");
            }
        }

        private IEnumerator TransitionToBossPhaseRoutine()
        {
            Debug.Log("Peringatan: Mendekati Menit ke-5! Mempersiapkan arena untuk Boss...");
            
            // 1. Hentikan pemanggilan musuh biasa dari SpawnManager
            if (spawnManager != null) spawnManager.StopSpawning();

            // 2. Kumpulkan semua objek ter-pool aktif di layar
            PooledObject[] allActive = FindObjectsByType<PooledObject>(FindObjectsSortMode.None);
            
            // 3. Hilangkan musuh biasa (ber-tag "Enemy") satu per satu dengan jeda dramatis
            foreach (var obj in allActive)
            {
                if (obj != null && obj.gameObject.activeInHierarchy && obj.gameObject.CompareTag("Enemy"))
                {
                    PoolManager.Instance.ReturnToPool(obj.gameObject);
                    yield return new WaitForSeconds(0.2f); // Efek hilang bergiliran
                }
            }

            // 4. Jeda keheningan tegang sejenak sebelum Boss masuk
            yield return new WaitForSeconds(1.5f);

            Debug.Log("BOSS MUNCUL DARI ATAS LAYAR!");
            
            // Hitung koordinat kemunculan tepat di luar batas atas layar tengah
            Vector3 bossSpawnPos = new Vector3(0, 10f, 0);
            if (Camera.main != null)
            {
                float screenHeight = Camera.main.orthographicSize;
                bossSpawnPos = new Vector3(0, screenHeight + 3f, 0);
            }

            // Panggil Boss dari PoolManager menggunakan Key yang bisa diatur di Inspector
            PoolManager.Instance.GetPooledObject(bossPoolKey, bossSpawnPos, Quaternion.identity);
        }


        private void OnDestroy()
        {
            if (player != null) player.OnPlayerDeath -= HandleGameOver;
        }

        private void HandleGameOver()
        {
            Debug.Log("Game Manager: Player mati, menghentikan game...");
            isGameActive = false;
            
            // 1. Hentikan Spawner
            if (spawnManager != null) spawnManager.StopSpawning();

            // 2. Tampilkan UI Game Over
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        // FUNGSI BARU: Dipanggil saat Boss dikalahkan (Kondisi Menang)
        public void HandleVictory()
        {
            Debug.Log("Game Manager: Boss dikalahkan! Memicu Victory...");
            isGameActive = false;

            // Tampilkan UI Menang
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        // Fungsi untuk dipanggil tombol "Restart" di UI
        public void RestartGame()
        {
            Debug.Log("Resetting Game...");
            
            // 1. Kembalikan semua ke Pool
            PoolManager.Instance.ClearAllActiveObjects();

            // 2. Reset Player
            if (player != null) player.ResetPlayer();

            // 3. Jalankan lagi Spawner
            if (spawnManager != null) spawnManager.StartSpawning();

            // 4. Matikan panel UI
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            // 5. Reset Status & Waktu
            survivalTime = 0f;
            currentScore = 0;
            isGameActive = true;
            isBossPhaseTriggered = false;
            Time.timeScale = 1f;

            OnScoreChanged?.Invoke(currentScore, highScore);
        }

        public void AddScore(int amount, Vector3 position)
        {
            if (!isGameActive) return;

            currentScore += amount;
            
            // Cek dan simpan High Score persisten (Save System Plus Point)
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }

            OnScoreChanged?.Invoke(currentScore, highScore);

            // Munculkan angka melayang (Pool Key: "ScorePopup")
            GameObject popup = PoolManager.Instance.GetPooledObject("ScorePopup", position, Quaternion.identity);
            if (popup != null)
            {
                var fs = popup.GetComponent<FloatingScore>();
                if (fs != null) fs.Setup(amount);
            }
        }
    }
}

