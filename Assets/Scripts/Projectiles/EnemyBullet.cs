using UnityEngine;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBullet : MonoBehaviour
    {
        [SerializeField] private WeaponDataSO data;
        private Rigidbody2D rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void OnEnable()
        {
            if (data == null) return;

            rb.linearVelocity = transform.up * data.bulletSpeed;
            
            Invoke("ReturnToPool", data.bulletLifeTime);
        }

        void OnDisable()
        {
            CancelInvoke();
        }

        private void ReturnToPool()
        {
            PoolManager.Instance.ReturnToPool(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CheckCollision(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CheckCollision(collision.gameObject);
        }

        private void CheckCollision(GameObject other)
        {
            // Anti friendly fire
            if (other.CompareTag("Enemy")) return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            
            if (damageable != null)
            {
                damageable.TakeDamage(data.damage);
                ReturnToPool();
            }
        }
    }
}
