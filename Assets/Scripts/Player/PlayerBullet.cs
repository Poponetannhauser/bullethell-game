using UnityEngine;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
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
            
            // Kembalikan ke pool setelah beberapa detik (Life Time)
            Invoke("ReturnToPool", data.bulletLifeTime);
        }

        void OnDisable()
        {
            // Batalkan Invoke kalau peluru mati sebelum waktunya (misal kena musuh)
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
            // Jangan sampai Player bunuh diri pakai peluru sendiri (Friendly Fire)
            if (other.CompareTag("Player")) return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            
            if (damageable != null)
            {
                damageable.TakeDamage(data.damage);
                
                // Peluru hancur setelah kena target
                ReturnToPool();
            }
        }

    }
}


