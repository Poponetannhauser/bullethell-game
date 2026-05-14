using UnityEngine;
using System.Collections;
using BulletHell.Data;
using BulletHell.Core;
using BulletHell.Managers;

namespace BulletHell.Enemies
{
    /// <summary>
    /// Kelas induk untuk semua musuh. Mengotomatisasi HP, Hit-Flash, dan Kematian.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Base Data & Visuals")]
        [SerializeField] protected EnemyDataSO data;
        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected float currentHealth;
        protected Color initialColor;

        protected virtual void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                // Mengambil warna langsung dari prefab saat pertama kali dibuat (Awake)
                // Ini mencegah bug saat pooling jika musuh mati pas lagi hit-flash
                initialColor = spriteRenderer.color;
            }
        }

        protected virtual void OnEnable()
        {
            if (data != null)
            {
                currentHealth = data.maxHealth;
            }
            else
            {
                Debug.LogWarning($"EnemyDataSO belum di-assign pada {gameObject.name}!");
            }

            // Pastikan warna kembali normal saat keluar dari pool
            if (spriteRenderer != null)
            {
                spriteRenderer.color = GetCurrentBaseColor();
            }
        }

        public virtual void TakeDamage(float amount)
        {
            currentHealth -= amount;

            // Mencegah error jika musuh mati di frame yang sama saat disable
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HitFlashRoutine());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.color = data.hitFlashColor;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = GetCurrentBaseColor();
            }
        }

        /// <summary>
        /// Mengembalikan warna yang harus ditampilkan setelah flash selesai.
        /// Bisa di-override oleh Boss (misal saat masuk fase 2).
        /// </summary>
        protected virtual Color GetCurrentBaseColor()
        {
            return initialColor;
        }

        protected virtual void Die()
        {
            // 1. Tambah skor & Munculkan Floating Score
            if (GameManager.Instance != null && data != null)
            {
                GameManager.Instance.AddScore(data.scoreValue, transform.position);
            }

            // 2. Efek Visual Shatter
            SpawnShatterEffect();

            // 3. Kembali ke Object Pool
            PoolManager.Instance.ReturnToPool(gameObject);
        }

        protected virtual void SpawnShatterEffect()
        {
            if (data == null) return;

            GameObject explosion = PoolManager.Instance.GetPooledObject("ShatterEffect", transform.position, Quaternion.identity);
            if (explosion != null)
            {
                // Warna shatter tetap mengambil dari SO agar konsisten secara desain
                var shatter = explosion.GetComponent<BulletHell.VFX.ShatterExplosion>();
                if (shatter != null) shatter.Setup(data.shatterColor);
            }
        }

        // Common utility untuk tabrakan dengan Player
        protected virtual void HandlePlayerCollision(GameObject other, float contactDamage = 10f)
        {
            if (other.CompareTag("Player"))
            {
                IDamageable player = other.GetComponentInParent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(contactDamage);
                }
                Die();
            }
        }
    }
}
