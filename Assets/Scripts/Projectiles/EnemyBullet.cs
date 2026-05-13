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

            // Berikan kecepatan saat peluru aktif
            rb.linearVelocity = transform.up * data.bulletSpeed;
            
            // Kembalikan ke pool setelah durasi habis
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
            // Jangan melukai sesama musuh (Anti Friendly Fire untuk musuh)
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
