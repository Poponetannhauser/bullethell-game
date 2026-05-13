using System.Collections;
using UnityEngine;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBoss : MonoBehaviour, IDamageable
    {
        [Header("Data & References")]
        [SerializeField] private EnemyDataSO data;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Movement Settings")]
        [SerializeField] private float entrySpeed = 2f;
        [SerializeField] private float targetScreenYOffset = 2.5f; // Jarak diam dari atas layar
        [SerializeField] private float bounceSpeed = 4f; // Kecepatan memantul
        [SerializeField] private float phase2SpeedMultiplier = 1.4f; // Akselerasi saat marah

        [Header("Phase 1: Radial Ring Shot")]
        [SerializeField] private int radialBulletCount = 12;
        [SerializeField] private float phase1FireRate = 2.5f;

        [Header("Phase 2: Spiral Rotating Shot")]
        [SerializeField] private float phase2FireRate = 0.15f;
        [SerializeField] private float spiralAngleIncrement = 20f;
        [SerializeField] private Color phase2Color = new Color(1f, 0.4f, 0f); // Warna oranye kemarahan

        private Rigidbody2D rb;
        private Transform playerTransform;
        private float currentHealth;
        private bool isEntering;
        private bool isPhase2;
        private float nextFireTime;
        private float currentSpiralAngle;
        private float targetY;

        private Color initialColor;
        private Vector2 moveDirection;
        private Vector2 minCamBounds;
        private Vector2 maxCamBounds;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void OnEnable()
        {
            if (spriteRenderer != null) initialColor = spriteRenderer.color;
            if (data != null) currentHealth = data.maxHealth;
            isEntering = true;
            isPhase2 = false;
            currentSpiralAngle = 0f;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;

            // Hitung posisi Y target tempat Boss akan berhenti masuk panggung
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                targetY = mainCam.orthographicSize - targetScreenYOffset;
                
                // Batas kiri, kanan, atas, dan area tengah bawah untuk memantul
                minCamBounds = mainCam.ViewportToWorldPoint(new Vector3(0.1f, 0.4f, 0)); // Dibatasi agar tidak turun terlalu dekat ke pemain
                maxCamBounds = mainCam.ViewportToWorldPoint(new Vector3(0.9f, 0.9f, 0));
            }
            else
            {
                targetY = 3f;
                minCamBounds = new Vector2(-7f, 0f);
                maxCamBounds = new Vector2(7f, 4f);
            }
        }

        void FixedUpdate()
        {
            if (data == null) return;

            // GERAKAN MASUK: Bergerak turun perlahan sampai mencapai posisi targetY
            if (isEntering)
            {
                if (transform.position.y > targetY)
                {
                    rb.linearVelocity = Vector2.down * entrySpeed;
                }
                else
                {
                    // Selesai masuk panggung, pilih arah pantulan awal secara acak
                    isEntering = false;
                    nextFireTime = Time.time + 1f; // Jeda sebelum menembak
                    
                    float randomAngle = Random.Range(20f, 160f); // Sudut mengarah ke atas/samping
                    if (Random.value > 0.5f) randomAngle += 180f;
                    moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
                }
            }
            else
            {
                // Kecepatan gerak meningkat jika masuk Fase 2
                float currentSpeed = isPhase2 ? bounceSpeed * phase2SpeedMultiplier : bounceSpeed;
                rb.linearVelocity = moveDirection * currentSpeed;

                // Logika Memantul (Bouncing) jika menabrak dinding imajiner kamera
                Vector3 pos = transform.position;
                bool bounced = false;

                if (pos.x <= minCamBounds.x && moveDirection.x < 0)
                {
                    moveDirection.x = -moveDirection.x;
                    bounced = true;
                }
                else if (pos.x >= maxCamBounds.x && moveDirection.x > 0)
                {
                    moveDirection.x = -moveDirection.x;
                    bounced = true;
                }

                if (pos.y <= minCamBounds.y && moveDirection.y < 0)
                {
                    moveDirection.y = -moveDirection.y;
                    bounced = true;
                }
                else if (pos.y >= maxCamBounds.y && moveDirection.y > 0)
                {
                    moveDirection.y = -moveDirection.y;
                    bounced = true;
                }

                // Tambahkan sedikit rotasi acak pada vektor pantulan agar jalurnya tidak monoton/berulang
                if (bounced)
                {
                    moveDirection = Quaternion.Euler(0, 0, Random.Range(-15f, 15f)) * moveDirection;
                    moveDirection.Normalize();
                }
            }
        }

        void Update()
        {
            // Jangan menembak selama proses masuk panggung
            if (isEntering || data == null) return;

            // CEK TRANSISI FASE 2: Jika HP di bawah 50%
            if (!isPhase2 && currentHealth <= data.maxHealth * 0.5f)
            {
                EnterPhase2();
            }

            // LOGIKA SERANGAN BOSS BERDASARKAN FASE
            if (Time.time >= nextFireTime)
            {
                if (!isPhase2)
                {
                    ExecutePhase1Attack();
                    nextFireTime = Time.time + phase1FireRate;
                }
                else
                {
                    ExecutePhase2Attack();
                    nextFireTime = Time.time + phase2FireRate;
                }
            }
        }

        private void ExecutePhase1Attack()
        {
            if (string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;

            // Serangan Cincin / Ring Shot 360 Derajat
            float angleStep = 360f / radialBulletCount;
            float currentAngle = 0f;

            for (int i = 0; i < radialBulletCount; i++)
            {
                Quaternion bulletRotation = Quaternion.Euler(0, 0, currentAngle);
                PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, bulletRotation);
                currentAngle += angleStep;
            }
        }

        private void ExecutePhase2Attack()
        {
            if (string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;

            // Serangan Spiral Berputar Cepat
            Quaternion bulletRotation = Quaternion.Euler(0, 0, currentSpiralAngle);
            PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, bulletRotation);
            
            // Tambahkan rotasi untuk peluru berikutnya
            currentSpiralAngle += spiralAngleIncrement;
            if (currentSpiralAngle >= 360f) currentSpiralAngle -= 360f;
        }

        private void EnterPhase2()
        {
            isPhase2 = true;
            Debug.Log("BOSS MASUK FASE 2: MODE SPIRAL AKTIF!");
            if (spriteRenderer != null) spriteRenderer.color = phase2Color;
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            // Visual feedback berkedip saat kena hit
            StartCoroutine(HitFlashRoutine());

            if (currentHealth <= 0) Die();
        }

        private IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer != null && data != null)
            {
                Color baseColor = isPhase2 ? phase2Color : initialColor;
                spriteRenderer.color = data.hitFlashColor;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = baseColor;
            }
        }

        private void Die()
        {
            Debug.Log("BOSS DIKALAHKAN! PEMAIN MENANG!");
            
            // Guncangan layar masif saat klimaks (Polish & Juice Plus Point)
            CameraShake.TriggerShake(0.8f, 0.6f);

            // Munculkan efek pecah masif di beberapa titik sekitar Boss
            Color shatterColor = isPhase2 ? phase2Color : initialColor;
            
            SpawnShatter(transform.position, shatterColor);
            SpawnShatter(transform.position + Vector3.left * 1.5f, shatterColor);
            SpawnShatter(transform.position + Vector3.right * 1.5f, shatterColor);
            SpawnShatter(transform.position + Vector3.up * 1.5f, shatterColor);

            // Tambahkan skor besar
            if (GameManager.Instance != null && data != null)
            {
                GameManager.Instance.AddScore(data.scoreValue, transform.position);
                GameManager.Instance.HandleVictory();
            }

            // Kembalikan ke pool
            PoolManager.Instance.ReturnToPool(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var damageable = collision.gameObject.GetComponentInParent<IDamageable>();
                if (damageable != null) damageable.TakeDamage(25f); // Hit boss sangat mematikan
            }
        }
        private void SpawnShatter(Vector3 position, Color color)
        {
            GameObject explosion = PoolManager.Instance.GetPooledObject("ShatterEffect", position, Quaternion.identity);
            if (explosion != null)
            {
                var shatter = explosion.GetComponent<BulletHell.VFX.ShatterExplosion>();
                if (shatter != null) shatter.Setup(color);
            }
        }
    }
}
