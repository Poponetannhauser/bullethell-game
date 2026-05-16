using UnityEngine;
using BulletHell.Managers;

namespace BulletHell.Gameplay
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public string poolKey;
        public float weight = 100f;
        public float minSurvivalTime = 0f;
    }

    // Spawn musuh dari luar layar berdasarkan weight
    // Spawnrate meningkat seiring waktu untuk menaikkan kesulitan
    public class SpawnManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private EnemySpawnConfig[] enemyConfigs = new EnemySpawnConfig[]
        {
            new EnemySpawnConfig { poolKey = "Enemy",        weight = 100f, minSurvivalTime = 0f  },
            new EnemySpawnConfig { poolKey = "EnemyTurret",  weight = 35f,  minSurvivalTime = 30f },
            new EnemySpawnConfig { poolKey = "EnemyStrafer", weight = 25f,  minSurvivalTime = 60f }
        };

        [Header("Difficulty Scaling")]
        [SerializeField] private float baseSpawnInterval = 1.5f;
        [SerializeField] private float minSpawnInterval = 0.4f;
        [SerializeField] private float intervalReductionPerSecond = 0.005f;
        [SerializeField] private float spawnPadding = 2f;

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
                float survivalTime = GameManager.Instance != null ? GameManager.Instance.survivalTime : 0f;
                float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (survivalTime * intervalReductionPerSecond));
                nextSpawnTime = Time.time + interval;
            }
        }

        public void StopSpawning() => isSpawning = false;

        public void StartSpawning()
        {
            isSpawning = true;
            nextSpawnTime = Time.time + baseSpawnInterval;
        }

        private void SpawnEnemy()
        {
            if (enemyConfigs == null || enemyConfigs.Length == 0) return;

            float survivalTime = GameManager.Instance != null ? GameManager.Instance.survivalTime : 0f;

            float totalWeight = 0f;
            foreach (var config in enemyConfigs)
                if (survivalTime >= config.minSurvivalTime) totalWeight += config.weight;

            if (totalWeight <= 0f) return;

            float roll = Random.Range(0, totalWeight);
            float cumulative = 0f;
            string selectedKey = "";

            foreach (var config in enemyConfigs)
            {
                if (survivalTime >= config.minSurvivalTime)
                {
                    cumulative += config.weight;
                    if (roll <= cumulative) { selectedKey = config.poolKey; break; }
                }
            }

            if (!string.IsNullOrEmpty(selectedKey))
                PoolManager.Instance.GetPooledObject(selectedKey, GetRandomSpawnPosition(), Quaternion.identity);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            float h = mainCam.orthographicSize;
            float w = h * mainCam.aspect;
            int side = Random.Range(0, 4);

            return side switch
            {
                0 => new Vector3(Random.Range(-w, w), h + spawnPadding, 0),
                1 => new Vector3(Random.Range(-w, w), -h - spawnPadding, 0),
                2 => new Vector3(-w - spawnPadding, Random.Range(-h, h), 0),
                _ => new Vector3(w + spawnPadding, Random.Range(-h, h), 0),
            };
        }
    }
}
