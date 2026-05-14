using System.Collections;
using UnityEngine;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBoss : EnemyBase
    {
        [Header("Boss References")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer outlineRenderer;

        [Header("Movement Settings")]
        [SerializeField] private float entrySpeed = 2f;
        [SerializeField] private float targetScreenYOffset = 2.5f; 
        [SerializeField] private float bounceSpeed = 4f; 
        [SerializeField] private float phase2SpeedMultiplier = 1.4f; 

        [Header("Phase 1: Radial Ring Shot")]
        [SerializeField] private int radialBulletCount = 12;
        [SerializeField] private float phase1FireRate = 2.5f;

        [Header("Phase 2: Spiral Rotating Shot")]
        [SerializeField] private float phase2FireRate = 0.15f;
        [SerializeField] private float spiralAngleIncrement = 20f;
        [SerializeField] private Color phase2Color = new Color(1f, 0.4f, 0f); 

        private Rigidbody2D rb;
        private Transform playerTransform;
        private bool isEntering;
        private bool isPhase2;
        private float nextFireTime;
        private float currentSpiralAngle;
        private float targetY;

        private Vector2 moveDirection;
        private Vector2 minCamBounds;
        private Vector2 maxCamBounds;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            isEntering = true;
            isPhase2 = false;
            currentSpiralAngle = 0f;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                targetY = mainCam.orthographicSize - targetScreenYOffset;
                minCamBounds = mainCam.ViewportToWorldPoint(new Vector3(0.1f, 0.4f, 0)); 
                maxCamBounds = mainCam.ViewportToWorldPoint(new Vector3(0.9f, 0.9f, 0));
            }
        }

        void FixedUpdate()
        {
            if (data == null) return;

            if (isEntering)
            {
                if (transform.position.y > targetY)
                {
                    rb.linearVelocity = Vector2.down * entrySpeed;
                }
                else
                {
                    isEntering = false;
                    nextFireTime = Time.time + 1f; 
                    
                    float randomAngle = Random.Range(20f, 160f); 
                    if (Random.value > 0.5f) randomAngle += 180f;
                    moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
                }
            }
            else
            {
                float currentSpeed = isPhase2 ? bounceSpeed * phase2SpeedMultiplier : bounceSpeed;
                rb.linearVelocity = moveDirection * currentSpeed;

                Vector3 pos = transform.position;
                bool bounced = false;

                if (pos.x <= minCamBounds.x && moveDirection.x < 0) { moveDirection.x = -moveDirection.x; bounced = true; }
                else if (pos.x >= maxCamBounds.x && moveDirection.x > 0) { moveDirection.x = -moveDirection.x; bounced = true; }

                if (pos.y <= minCamBounds.y && moveDirection.y < 0) { moveDirection.y = -moveDirection.y; bounced = true; }
                else if (pos.y >= maxCamBounds.y && moveDirection.y > 0) { moveDirection.y = -moveDirection.y; bounced = true; }

                if (bounced)
                {
                    moveDirection = Quaternion.Euler(0, 0, Random.Range(-15f, 15f)) * moveDirection;
                    moveDirection.Normalize();
                }
            }
        }

        void Update()
        {
            if (isEntering || data == null) return;

            if (!isPhase2 && currentHealth <= data.maxHealth * 0.5f) EnterPhase2();

            if (Time.time >= nextFireTime)
            {
                if (!isPhase2) { ExecutePhase1Attack(); nextFireTime = Time.time + phase1FireRate; }
                else { ExecutePhase2Attack(); nextFireTime = Time.time + phase2FireRate; }
            }
        }

        private void ExecutePhase1Attack()
        {
            if (string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;
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
            Quaternion bulletRotation = Quaternion.Euler(0, 0, currentSpiralAngle);
            PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, bulletRotation);
            currentSpiralAngle += spiralAngleIncrement;
            if (currentSpiralAngle >= 360f) currentSpiralAngle -= 360f;
        }

        private void EnterPhase2()
        {
            isPhase2 = true;
            if (spriteRenderer != null) spriteRenderer.color = phase2Color;
        }

        protected override Color GetCurrentBaseColor()
        {
            return isPhase2 ? phase2Color : initialColor;
        }

        protected override void Die()
        {
            // Juice masif untuk boss
            CameraShake.TriggerShake(0.8f, 0.6f);
            CameraShake.TriggerHitStop(0.15f);

            // Efek pecah berantai
            Color shatterColor = isPhase2 ? phase2Color : data.shatterColor;
            SpawnShatter(transform.position, shatterColor);
            SpawnShatter(transform.position + Vector3.left * 1.5f, shatterColor);
            SpawnShatter(transform.position + Vector3.right * 1.5f, shatterColor);
            SpawnShatter(transform.position + Vector3.up * 1.5f, shatterColor);

            if (GameManager.Instance != null && data != null)
            {
                GameManager.Instance.AddScore(data.scoreValue, transform.position);
                GameManager.Instance.HandleVictory();
            }

            PoolManager.Instance.ReturnToPool(gameObject);
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var player = collision.gameObject.GetComponentInParent<IDamageable>();
                if (player != null) player.TakeDamage(25f); 
            }
        }
    }
}
