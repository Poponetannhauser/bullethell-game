using UnityEngine;
using System.Collections;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyTurret : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyDataSO data;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D rb;
        private Transform playerTransform;
        private float currentHealth;
        private float nextFireTime;
        private Color initialColor;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void OnEnable()
        {
            if (data != null) 
            {
                currentHealth = data.maxHealth;
                if (spriteRenderer != null) initialColor = spriteRenderer.color;
            }

            // Cari player di scene
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }


        void FixedUpdate()
        {
            if (playerTransform == null || data == null) return;

            // 1. HITUNG ARAH & GERAK PERLAHAN KE PLAYER
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            
            // Memberikan pergerakan lambat agar tidak terjebak di luar layar saat di-spawn
            rb.linearVelocity = direction * data.moveSpeed;

            // 2. ROTASI TRACKING (Selalu mengarah ke Player)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }


        void Update()
        {
            if (playerTransform == null || data == null) return;

            // 2. LOGIKA MENEMBAK BERKALA
            if (Time.time >= nextFireTime)
            {
                Shoot();
                // fireRate di SO mendefinisikan jeda antar tembakan (misal 1.5 detik)
                nextFireTime = Time.time + (data.fireRate > 0 ? data.fireRate : 1.5f);
            }
        }

        private void Shoot()
        {
            if (data == null || string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;

            // Panggil peluru dari PoolManager
            PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, firePoint.rotation);
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
            // Jika Player menabrak badan Turret secara langsung
            if (other.CompareTag("Player"))
            {
                IDamageable damageable = other.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(10f);
                }
            }
        }
    }
}
