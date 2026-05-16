using UnityEngine;
using BulletHell.Managers;

namespace BulletHell.Enemies
{
    // Behavior: Shoots bullets at the player
    
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
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        void FixedUpdate()
        {
            if (playerTransform == null || data == null) return;

            Vector2 dir = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = dir * data.moveSpeed;
            rb.rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
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
            if (string.IsNullOrEmpty(data.bulletPoolKey) || firePoint == null) return;
            PoolManager.Instance.GetPooledObject(data.bulletPoolKey, firePoint.position, firePoint.rotation);
        }

        private void OnCollisionEnter2D(Collision2D col) => HandlePlayerCollision(col.gameObject);
        private void OnTriggerEnter2D(Collider2D col) => HandlePlayerCollision(col.gameObject);
    }
}
