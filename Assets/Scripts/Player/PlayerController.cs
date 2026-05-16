using UnityEngine;
using UnityEngine.InputSystem;
using BulletHell.Data;
using BulletHell.Managers;
using BulletHell.Core;
using System;
using System.Collections;

namespace BulletHell.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField] private WeaponDataSO weaponData;
        [SerializeField] private Transform firePoint;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invincibilityDuration = 0.5f;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 7f;

        private float currentHealth;
        private bool isInvincible;
        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 minBounds, maxBounds;

        private float nextFireTime;
        private float currentHeat;
        private bool isOverheated;
        private bool isFiring;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnHeatChanged;
        public event Action OnPlayerDeath;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void Start()
        {
            CalculateCameraBounds();
            currentHealth = maxHealth;
            currentHeat = 0f;
            isOverheated = false;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnHeatChanged?.Invoke(currentHeat, 100f);
        }

        void Update()
        {
            HandleShooting();
            HandleOverheatCooling();
            RotateTowardsMouse();
        }

        void FixedUpdate()
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        // Clamp posisi player agar tidak keluar dari batas layar kamera
        void LateUpdate()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
            transform.position = pos;
        }

        private void HandleShooting()
        {
            bool fireRequested = isFiring ||
                (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
                (Keyboard.current != null && Keyboard.current.spaceKey.isPressed);

            if (fireRequested && Time.time >= nextFireTime && !isOverheated)
            {
                Shoot();
                nextFireTime = Time.time + (weaponData != null ? weaponData.fireRate : 0.5f);
            }
        }

        // Overheat passive cooling
        private void HandleOverheatCooling()
        {
            if (GameSettings.SelectedMode != GameMode.Overheat || weaponData == null) return;
            if (isOverheated || currentHeat <= 0) return;

            currentHeat -= weaponData.coolDownRate * Time.deltaTime;
            currentHeat = Mathf.Max(0, currentHeat);
            OnHeatChanged?.Invoke(currentHeat, 100f);
        }

        private void Shoot()
        {
            if (weaponData == null || firePoint == null) return;

            PoolManager.Instance.GetPooledObject(weaponData.bulletPoolKey, firePoint.position, firePoint.rotation);

            // Overheat accumulation
            if (GameSettings.SelectedMode == GameMode.Overheat)
            {
                currentHeat += weaponData.heatPerShot;
                OnHeatChanged?.Invoke(currentHeat, 100f);

                if (currentHeat >= 100f)
                {
                    AudioManager.PlaySFX(AudioManager.Instance?.overheatAlarm);
                    StartCoroutine(OverheatRoutine());
                }
            }

            AudioManager.PlaySFX(AudioManager.Instance?.playerShoot, 0.4f);
        }

        // Overheat cooldown
        private IEnumerator OverheatRoutine()
        {
            isOverheated = true;
            if (spriteRenderer != null) spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(weaponData.overheatCooldownDuration);

            currentHeat = 0f;
            isOverheated = false;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            OnHeatChanged?.Invoke(currentHeat, 100f);
        }

        private void RotateTowardsMouse()
        {
            if (Mouse.current == null) return;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 lookDir = (Vector2)mousePos - (Vector2)transform.position;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }

        private void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
        private void OnFire(InputValue value) => isFiring = value.isPressed;

        private void CalculateCameraBounds()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            minBounds = cam.ViewportToWorldPoint(new Vector3(0.05f, 0.05f, 0));
            maxBounds = cam.ViewportToWorldPoint(new Vector3(0.95f, 0.95f, 0));
        }

        public void TakeDamage(float amount)
        {
            if (isInvincible || currentHealth <= 0) return;

            currentHealth -= amount;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            CameraShake.TriggerShake(0.2f, 0.35f);
            CameraShake.TriggerHitStop(0.06f);
            AudioManager.PlaySFX(AudioManager.Instance?.playerHit);

            if (currentHealth <= 0)
                Die();
            else
                StartCoroutine(InvincibilityRoutine());
        }

        // I-frame blink
        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;
            float blinkDuration = invincibilityDuration / 10f;

            for (int i = 0; i < 5; i++)
            {
                if (spriteRenderer != null) spriteRenderer.color = new Color(1, 0, 0, 0.5f);
                yield return new WaitForSeconds(blinkDuration);
                if (spriteRenderer != null) spriteRenderer.color = Color.gray;
                yield return new WaitForSeconds(blinkDuration);
            }

            isInvincible = false;
        }

        public void ResetPlayer()
        {
            currentHealth = maxHealth;
            currentHeat = 0f;
            isOverheated = false;
            isInvincible = false;
            transform.position = Vector3.zero;
            gameObject.SetActive(true);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnHeatChanged?.Invoke(currentHeat, 100f);
        }

        private void Die()
        {
            AudioManager.PlaySFX(AudioManager.Instance?.playerDeath);
            OnPlayerDeath?.Invoke();
            gameObject.SetActive(false);
        }
    }
}