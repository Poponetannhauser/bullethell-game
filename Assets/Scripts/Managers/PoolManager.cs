using System.Collections.Generic;
using UnityEngine;

namespace BulletHell.Managers
{
    [System.Serializable]
    public class Pool
    {
        public string poolKey;
        public GameObject prefab;
        public int size;
    }

    public class PoolManager : MonoBehaviour
    {
        #region Singleton
        public static PoolManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        public List<Pool> pools;
        public Dictionary<string, Queue<GameObject>> poolDictionary;

        void Start()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    CreateNewObjectInPool(pool, objectPool);
                }

                poolDictionary.Add(pool.poolKey, objectPool);
            }
        }

        private GameObject CreateNewObjectInPool(Pool pool, Queue<GameObject> queue)
        {
            GameObject obj = Instantiate(pool.prefab);
            obj.SetActive(false);
            obj.transform.SetParent(this.transform);

            // Add PooledObject component to track its poolKey
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj == null) pooledObj = obj.AddComponent<PooledObject>();
            pooledObj.poolKey = pool.poolKey;

            queue.Enqueue(obj);
            return obj;
        }

        public GameObject GetPooledObject(string poolKey)
        {
            return GetPooledObject(poolKey, Vector3.zero, Quaternion.identity, false);
        }

        public GameObject GetPooledObject(string poolKey, Vector3 position, Quaternion rotation, bool useTransform = true)
        {
            if (!poolDictionary.ContainsKey(poolKey))
            {
                Debug.LogWarning("Pool with key " + poolKey + " doesn't exist.");
                return null;
            }

            Queue<GameObject> queue = poolDictionary[poolKey];
            GameObject objectToSpawn;

            if (queue.Count > 0)
            {
                objectToSpawn = queue.Dequeue();
            }
            else
            {
                // Optional: Expand pool if empty
                Pool pool = pools.Find(p => p.poolKey == poolKey);
                objectToSpawn = CreateNewObjectInPool(pool, queue);
                queue.Dequeue(); // Remove from queue since we're about to use it
            }

            // Set transform BEFORE activating so OnEnable gets the correct rotation
            if (useTransform)
            {
                objectToSpawn.transform.position = position;
                objectToSpawn.transform.rotation = rotation;
            }

            objectToSpawn.SetActive(true);
            return objectToSpawn;
        }


        public void ReturnToPool(GameObject obj)
        {
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            
            if (pooledObj == null)
            {
                Debug.LogWarning("Object " + obj.name + " is not a pooled object.");
                obj.SetActive(false);
                return;
            }

            obj.SetActive(false);
            poolDictionary[pooledObj.poolKey].Enqueue(obj);
        }

        // FUNGSI BARU: Untuk membersihkan layar tanpa reload scene
        public void ClearAllActiveObjects()
        {
            // Cari semua objek yang punya label PooledObject di scene
            PooledObject[] activeObjects = FindObjectsByType<PooledObject>(FindObjectsSortMode.None);
            
            foreach (PooledObject obj in activeObjects)
            {
                if (obj.gameObject.activeInHierarchy)
                {
                    ReturnToPool(obj.gameObject);
                }
            }
        }
    }
}

