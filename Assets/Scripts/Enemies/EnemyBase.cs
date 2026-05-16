using UnityEngine;
using System.Collections;
using BulletHell.Data;
using BulletHell.Core;
using BulletHell.Managers;

namespace BulletHell.Enemies
{
    // Base class untuk logika HP, hit-flash, die, dan collision
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Enemy Data")]
        [SerializeField] protected EnemyDataSO data;
        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected float currentHealth;
        protected Color initialColor;

        protected virtual void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
                initialColor = spriteRenderer.color;
        }

        protected virtual void OnEnable()
        {
            if (data != null)
                currentHealth = data.maxHealth;
            else
                Debug.LogWarning($"EnemyDataSO not assigned on {gameObject.name}!");

            if (spriteRenderer != null)
                spriteRenderer.color = GetCurrentBaseColor();
        }

        public virtual void TakeDamage(float amount)
        {
            currentHealth -= amount;

            if (gameObject.activeInHierarchy)
                StartCoroutine(HitFlashRoutine());

            if (currentHealth <= 0)
                Die();
        }

        protected virtual IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.color = data.hitFlashColor;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = GetCurrentBaseColor();
            }
        }

        // Override warna untuk boss fase 2
        protected virtual Color GetCurrentBaseColor()
        {
            return initialColor;
        }

        protected virtual void Die()
        {
            if (GameManager.Instance != null && data != null)
                GameManager.Instance.AddScore(data.scoreValue, transform.position);

            AudioManager.PlaySFX(AudioManager.Instance?.enemyDeath, 0.6f);
            SpawnShatterEffect();
            PoolManager.Instance.ReturnToPool(gameObject);
        }

        protected virtual void SpawnShatterEffect()
        {
            if (data == null) return;

            GameObject explosion = PoolManager.Instance.GetPooledObject("ShatterEffect", transform.position, Quaternion.identity);
            if (explosion != null)
            {
                var shatter = explosion.GetComponent<BulletHell.VFX.ShatterExplosion>();
                if (shatter != null) shatter.Setup(data.shatterColor);
            }
        }

        // Collision handler
        protected virtual void HandlePlayerCollision(GameObject other)
        {
            if (other.CompareTag("Player"))
            {
                IDamageable player = other.GetComponentInParent<IDamageable>();
                if (player != null && data != null)
                    player.TakeDamage(data.contactDamage);

                Die();
            }
        }
    }
}
