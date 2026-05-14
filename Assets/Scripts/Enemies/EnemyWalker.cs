using UnityEngine;
using System.Collections;
using BulletHell.Managers;
using BulletHell.Data;
using BulletHell.Core;

namespace BulletHell.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyWalker : EnemyBase
    {
        private Rigidbody2D rb;
        private Transform playerTransform;

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
