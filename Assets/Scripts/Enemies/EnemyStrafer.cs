using System.Collections;
using UnityEngine;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
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

            entryDirection = (((Vector2)Vector3.zero) - (Vector2)transform.position).normalized;
            if (entryDirection == Vector2.zero) entryDirection = Vector2.down;
        }

        void FixedUpdate()
        {
            if (data == null) return;

            Vector2 perpendicularDir = new Vector2(-entryDirection.y, entryDirection.x);
            Vector2 advanceVelocity = entryDirection * (data.moveSpeed * advanceSpeedMultiplier);
            Vector2 strafeVelocity = perpendicularDir * Mathf.Sin(Time.time * strafeFrequency) * strafeAmplitude;
            
            rb.linearVelocity = advanceVelocity + strafeVelocity;

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
                if (data != null && !string.IsNullOrEmpty(data.bulletPoolKey) && firePoint != null)
                {
                    PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, firePoint.rotation);
                }
                yield return new WaitForSeconds(burstInterval);
            }

            isShootingBurst = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandlePlayerCollision(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandlePlayerCollision(collision.gameObject);
        }
    }
}
