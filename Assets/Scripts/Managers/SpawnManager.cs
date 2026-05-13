using UnityEngine;
using BulletHell.Managers;

namespace BulletHell.Gameplay
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public string poolKey;
        public float weight = 100f; // Peluang/bobot kemunculan
        public float minSurvivalTime = 0f; // Syarat waktu bertahan hidup minimal (detik)
    }

    public class SpawnManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private EnemySpawnConfig[] enemyConfigs = new EnemySpawnConfig[]
        {
            new EnemySpawnConfig { poolKey = "Enemy", weight = 100f, minSurvivalTime = 0f },
            new EnemySpawnConfig { poolKey = "EnemyTurret", weight = 35f, minSurvivalTime = 30f },
            new EnemySpawnConfig { poolKey = "EnemyStrafer", weight = 25f, minSurvivalTime = 60f }
        };

        [Header("Difficulty Scaling")]
        [SerializeField] private float baseSpawnInterval = 1.5f;
        [SerializeField] private float minSpawnInterval = 0.4f;
        [SerializeField] private float intervalReductionPerSecond = 0.005f; // Makin lama makin cepat muncul musuh
        [SerializeField] private float spawnPadding = 2f; // Jarak kemunculan di luar layar

        private float nextSpawnTime;
        private Camera mainCam;
        private bool isSpawning = true;

        void Start()
        {
            mainCam = Camera.main;
        }

        void Update()
        {
            if (!isSpawning) return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnEnemy();
                
                // Menghitung jeda dinamis berdasarkan waktu bertahan hidup dari GameManager
                float currentSurvivalTime = GameManager.Instance != null ? GameManager.Instance.survivalTime : 0f;
                float currentInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (currentSurvivalTime * intervalReductionPerSecond));
                
                nextSpawnTime = Time.time + currentInterval;
            }
        }

        public void StopSpawning()
        {
            isSpawning = false;
        }

        public void StartSpawning()
        {
            isSpawning = true;
            nextSpawnTime = Time.time + baseSpawnInterval;
        }

        private void SpawnEnemy()
        {
            if (enemyConfigs == null || enemyConfigs.Length == 0) return;

            float currentSurvivalTime = GameManager.Instance != null ? GameManager.Instance.survivalTime : 0f;

            // 1. Hitung total bobot (weight) dari semua musuh yang memenuhi syarat waktu
            float totalWeight = 0f;
            foreach (var config in enemyConfigs)
            {
                if (currentSurvivalTime >= config.minSurvivalTime)
                {
                    totalWeight += config.weight;
                }
            }

            if (totalWeight <= 0f) return;

            // 2. Pilih nilai acak berdasarkan total bobot
            float randomValue = Random.Range(0, totalWeight);
            float currentWeightSum = 0f;
            string selectedPoolKey = "";

            foreach (var config in enemyConfigs)
            {
                if (currentSurvivalTime >= config.minSurvivalTime)
                {
                    currentWeightSum += config.weight;
                    if (randomValue <= currentWeightSum)
                    {
                        selectedPoolKey = config.poolKey;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(selectedPoolKey))
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                PoolManager.Instance.GetPooledObject(selectedPoolKey, spawnPosition, Quaternion.identity);
            }
        }




        private Vector3 GetRandomSpawnPosition()
        {
            // Tentukan sisi mana musuh akan muncul (0: Atas, 1: Bawah, 2: Kiri, 3: Kanan)
            int side = Random.Range(0, 4);
            Vector3 spawnPoint = Vector3.zero;

            // Hitung dimensi kamera
            float screenHeight = mainCam.orthographicSize;
            float screenWidth = screenHeight * mainCam.aspect;

            switch (side)
            {
                case 0: // ATAS
                    spawnPoint = new Vector3(Random.Range(-screenWidth, screenWidth), screenHeight + spawnPadding, 0);
                    break;
                case 1: // BAWAH
                    spawnPoint = new Vector3(Random.Range(-screenWidth, screenWidth), -screenHeight - spawnPadding, 0);
                    break;
                case 2: // KIRI
                    spawnPoint = new Vector3(-screenWidth - spawnPadding, Random.Range(-screenHeight, screenHeight), 0);
                    break;
                case 3: // KANAN
                    spawnPoint = new Vector3(screenWidth + spawnPadding, Random.Range(-screenHeight, screenHeight), 0);
                    break;
            }

            return spawnPoint;
        }
    }
}
