using UnityEngine;
using System.Collections;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private Rigidbody2D rb;
        private Transform playerTransform;
        private float currentHealth;
        private Color initialColor;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void OnEnable()
        {
            // Reset darah dan warna saat keluar dari pool
            if (data != null) 
            {
                currentHealth = data.maxHealth;
                if (spriteRenderer != null) initialColor = spriteRenderer.color;
            }
            else Debug.LogError("Data EnemyDataSO belum di-assign di Inspector!");
            
            // Cari player di scene
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) 
            {
                playerTransform = player.transform;
            }
            else 
            {
                Debug.LogWarning("Musuh tidak menemukan objek dengan Tag 'Player'!");
            }
        }

        void FixedUpdate()
        {
            if (playerTransform == null || data == null) return;

            // 1. HITUNG ARAH KE PLAYER
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            
            // 2. GERAK PAKAI VELOCITY (Cara modern)
            rb.linearVelocity = direction * data.moveSpeed;

            // 3. ROTASI (Menghadap ke Player)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            
            // Efek visual saat terkena hit
            if (gameObject.activeInHierarchy) StartCoroutine(HitFlashRoutine());

            if (currentHealth <= 0) Die();
        }

        private IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.color = data.hitFlashColor;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = initialColor;
            }
        }

        private void Die()
        {
            // Beritahu GameManager untuk tambah skor
            if (GameManager.Instance != null && data != null)
            {
                GameManager.Instance.AddScore(data.scoreValue, transform.position);
            }

            // Munculkan efek pecah (Nier Automata Voxel Shatter style)
            GameObject explosion = PoolManager.Instance.GetPooledObject("ShatterEffect", transform.position, Quaternion.identity);
            if (explosion != null) 
            {
                var shatter = explosion.GetComponent<BulletHell.VFX.ShatterExplosion>();
                if (shatter != null) shatter.Setup(initialColor);
            }

            // Kembali ke pool (Optimasi)
            PoolManager.Instance.ReturnToPool(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandlePlayerCollision(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandlePlayerCollision(collision.gameObject);
        }

        private void HandlePlayerCollision(GameObject other)
        {
            if (other.CompareTag("Player"))
            {
                IDamageable damageable = other.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(10f); // Damage tabrakan
                }
                
                Die();
            }
        }
    }
}
