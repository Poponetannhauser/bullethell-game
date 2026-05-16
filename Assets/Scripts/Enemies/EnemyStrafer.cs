using System.Collections;
using UnityEngine;
using BulletHell.Managers;

namespace BulletHell.Enemies
{
    // Behavior:
    // Bergerak dengan pola gelombang sinus sambil mendekati tengah layar
    // Menembak dalam burst pendek ke arah player

    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyStrafer : EnemyBase
    {
        [Header("Strafe Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float strafeFrequency = 2f;
        [SerializeField] private float strafeAmplitude = 3f;
        [SerializeField] private float advanceSpeedMultiplier = 0.5f;

        [Header("Burst Settings")]
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstInterval = 0.1f;

        private Rigidbody2D rb;
        private Transform playerTransform;
        private bool isShootingBurst;
        private float nextFireTime;
        private Vector2 entryDirection;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            isShootingBurst = false;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;

            entryDirection = ((Vector2)Vector3.zero - (Vector2)transform.position).normalized;
            if (entryDirection == Vector2.zero) entryDirection = Vector2.down;
        }

        void FixedUpdate()
        {
            if (data == null) return;

            Vector2 perp = new Vector2(-entryDirection.y, entryDirection.x);
            Vector2 advance = entryDirection * (data.moveSpeed * advanceSpeedMultiplier);
            Vector2 strafe = perp * Mathf.Sin(Time.time * strafeFrequency) * strafeAmplitude;
            rb.linearVelocity = advance + strafe;

            if (playerTransform != null)
            {
                Vector2 aim = (playerTransform.position - transform.position).normalized;
                rb.rotation = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg - 90f;
            }
        }

        void Update()
        {
            if (playerTransform == null || data == null || isShootingBurst) return;

            if (Time.time >= nextFireTime)
            {
                StartCoroutine(ShootBurstRoutine());
                nextFireTime = Time.time + (data.fireRate > 0 ? data.fireRate : 2f);
            }
        }

        private IEnumerator ShootBurstRoutine()
        {
            isShootingBurst = true;
            for (int i = 0; i < burstCount; i++)
            {
                if (!string.IsNullOrEmpty(data.bulletPoolKey) && firePoint != null)
                    PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, firePoint.rotation);

                yield return new WaitForSeconds(burstInterval);
            }
            isShootingBurst = false;
        }

        private void OnCollisionEnter2D(Collision2D col) => HandlePlayerCollision(col.gameObject);
        private void OnTriggerEnter2D(Collider2D col) => HandlePlayerCollision(col.gameObject);
    }
}
