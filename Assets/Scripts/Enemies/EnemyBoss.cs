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

        [Header("Phase 2 Visuals")]
        [SerializeField] private Color phase2Color = new Color(1f, 0.4f, 0f);

        private Rigidbody2D rb;
        private Transform playerTransform;
        private bool isEntering;
        private bool isPhase2;
        private bool isDead;
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
            isPhase2 = false;
            isDead = false;

            base.OnEnable();

            isEntering = true;
            currentSpiralAngle = 0f;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;

            Camera mainCam = Camera.main;
            if (mainCam != null && data != null)
            {
                targetY = mainCam.orthographicSize - data.targetScreenYOffset;
                minCamBounds = mainCam.ViewportToWorldPoint(new Vector3(0.1f, 0.4f, 0));
                maxCamBounds = mainCam.ViewportToWorldPoint(new Vector3(0.9f, 0.9f, 0));
            }
        }

        void FixedUpdate()
        {
            if (data == null || isDead) return;

            if (isEntering)
            {
                if (transform.position.y > targetY)
                {
                    rb.linearVelocity = Vector2.down * data.entrySpeed;
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
                float speed = isPhase2 ? data.bounceSpeed * data.phase2SpeedMultiplier : data.bounceSpeed;
                rb.linearVelocity = moveDirection * speed;

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
            if (isEntering || data == null || isDead) return;

            if (!isPhase2 && currentHealth <= data.maxHealth * 0.5f)
                EnterPhase2();

            if (Time.time >= nextFireTime)
            {
                if (!isPhase2) { ExecutePhase1Attack(); nextFireTime = Time.time + data.phase1FireRate; }
                else { ExecutePhase2Attack(); nextFireTime = Time.time + data.phase2FireRate; }
            }
        }

        private void ExecutePhase1Attack()
        {
            if (string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;
            float angleStep = 360f / data.radialBulletCount;
            float angle = 0f;
            for (int i = 0; i < data.radialBulletCount; i++)
            {
                PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, Quaternion.Euler(0, 0, angle));
                angle += angleStep;
            }
        }

        private void ExecutePhase2Attack()
        {
            if (string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;
            PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, Quaternion.Euler(0, 0, currentSpiralAngle));
            currentSpiralAngle = (currentSpiralAngle + data.spiralAngleIncrement) % 360f;
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
            if (isDead) return;
            StartCoroutine(DeathSequenceRoutine());
        }

        private IEnumerator DeathSequenceRoutine()
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            nextFireTime = Time.time + 100f;

            Color shatterColor = isPhase2 ? phase2Color : data.shatterColor;
            float duration = 2.0f;
            float timer = 0f;

            while (timer < duration)
            {
                Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
                SpawnShatter(transform.position + offset, shatterColor);
                CameraShake.TriggerShake(0.1f, 0.2f);
                AudioManager.PlaySFX(AudioManager.Instance?.bossExplosion, 0.5f);

                if (spriteRenderer != null)
                    spriteRenderer.color = (timer % 0.2f > 0.1f) ? Color.white : GetCurrentBaseColor();

                float wait = Random.Range(0.05f, 0.15f);
                yield return new WaitForSeconds(wait);
                timer += wait;
            }

            CameraShake.TriggerShake(1.0f, 0.8f);
            CameraShake.TriggerHitStop(0.2f);

            for (int i = 0; i < 8; i++)
            {
                Vector3 finalOffset = Random.insideUnitCircle * 2f;
                SpawnShatter(transform.position + finalOffset, shatterColor);
            }

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
                if (player != null && data != null) 
                    player.TakeDamage(data.contactDamage);
            }
        }
    }
}
