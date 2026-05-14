using UnityEngine;
using System.Collections;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyShooter : EnemyBase
    {
        [SerializeField] private Transform firePoint;

        private Rigidbody2D rb;
        private Transform playerTransform;
        private float nextFireTime;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Cari player di scene
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        void FixedUpdate()
        {
            if (playerTransform == null || data == null) return;

            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * data.moveSpeed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }

        void Update()
        {
            if (playerTransform == null || data == null) return;

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + (data.fireRate > 0 ? data.fireRate : 1.5f);
            }
        }

        private void Shoot()
        {
            if (data == null || string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;
            PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, firePoint.rotation);
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
