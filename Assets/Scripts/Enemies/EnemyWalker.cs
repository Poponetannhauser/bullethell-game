using UnityEngine;
using System.Collections;
using BulletHell.Managers;

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

        private void OnCollisionEnter2D(Collision2D col) => HandlePlayerCollision(col.gameObject);
        private void OnTriggerEnter2D(Collider2D col) => HandlePlayerCollision(col.gameObject);
    }
}
