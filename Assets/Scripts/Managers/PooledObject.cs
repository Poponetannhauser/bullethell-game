using UnityEngine;

namespace BulletHell.Managers
{
    public class PooledObject : MonoBehaviour
    {
        public string poolKey;

        public void ReturnToPool()
        {
            PoolManager.Instance.ReturnToPool(gameObject);
        }
    }
}

