using System.Collections;
using UnityEngine;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyStrafer : MonoBehaviour, IDamageable
    {
        [Header("Data & References")]
        [SerializeField] private EnemyDataSO data;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Strafe Settings")]
        [SerializeField] private float strafeFrequency = 2f; // Kecepatan bolak-balik menyamping
        [SerializeField] private float strafeAmplitude = 3f; // Lebar gerakan menyamping
        [SerializeField] private float advanceSpeedMultiplier = 0.5f; // Skala kecepatan maju ke tengah layar

        [Header("Burst Settings")]
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstInterval = 0.1f;

        private Rigidbody2D rb;
        private Transform playerTransform;
        private float currentHealth;
        private bool isShootingBurst;
        private float nextFireTime;
        private Vector2 entryDirection;
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
            isShootingBurst = false;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;

            // Menghitung arah masuk dasar menuju tengah layar (0,0) saat pertama kali spawn
            entryDirection = (((Vector2)Vector3.zero) - (Vector2)transform.position).normalized;
            // Jika kebetulan pas di tengah, beri default arah ke bawah
            if (entryDirection == Vector2.zero) entryDirection = Vector2.down;
        }

        void FixedUpdate()
        {
            if (data == null) return;

            // 1. PERGERAKAN: Kombinasi maju pelan ke arah tengah + manuver menyamping (Sine Wave)
            // Menghitung vektor tegak lurus (perpendicular) dari arah masuk untuk gerakan menyamping
            Vector2 perpendicularDir = new Vector2(-entryDirection.y, entryDirection.x);
            
            // Menghitung kecepatan: maju perlahan + ayunan menyamping berbasis waktu
            Vector2 advanceVelocity = entryDirection * (data.moveSpeed * advanceSpeedMultiplier);
            Vector2 strafeVelocity = perpendicularDir * Mathf.Sin(Time.time * strafeFrequency) * strafeAmplitude;
            
            rb.linearVelocity = advanceVelocity + strafeVelocity;

            // 2. ROTASI: Selalu membidik ke arah Player agar burst fire akurat
            if (playerTransform != null)
            {
                Vector2 aimDir = (playerTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = angle;
            }
        }

        void Update()
        {
            if (playerTransform == null || data == null || isShootingBurst) return;

            // LOGIKA COOLDOWN BURST FIRE
            if (Time.time >= nextFireTime)
            {
                StartCoroutine(ShootBurstRoutine());
                // Mengatur waktu tembakan berikutnya setelah keseluruhan burst selesai
                nextFireTime = Time.time + (data.fireRate > 0 ? data.fireRate : 2f);
            }
        }

        private IEnumerator ShootBurstRoutine()
        {
            isShootingBurst = true;

            for (int i = 0; i < burstCount; i++)
            {
                if (data != null && !string.IsNullOrEmpty(data.bulletPoolKey) && firePoint != null)
                {
                    PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, firePoint.rotation);
                }
                // Jeda sangat cepat antar peluru dalam satu burst
                yield return new WaitForSeconds(burstInterval);
            }

            isShootingBurst = false;
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
