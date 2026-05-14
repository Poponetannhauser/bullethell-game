using UnityEngine;
using UnityEngine.InputSystem;
using BulletHell.Data;
using BulletHell.Managers;
using BulletHell.Core; // Tambahkan ini
using System;
using System.Collections;

namespace BulletHell.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))] // Menjamin komponen Player Input ada
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField] private WeaponDataSO weaponData;
        [SerializeField] private Transform firePoint;

        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invincibilityDuration = 0.5f;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 7f;

        private float currentHealth;
        private bool isInvincible;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 minBounds;
        private Vector2 maxBounds;

        private float nextFireTime;
        private float currentHeat = 0f;
        private bool isOverheated = false;

        // C# Events untuk UI
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

        private bool isFiring;

        void Update()
        {
            HandleShooting();
            HandleOverheatLogic();
            RotateTowardsMouse();
        }

        private void HandleShooting()
        {
            bool fireRequested = isFiring || 
                                 (Mouse.current != null && Mouse.current.leftButton.isPressed) || 
                                 (Keyboard.current != null && Keyboard.current.spaceKey.isPressed);

            // Bisa menembak jika: Ada request tembak, cooldown peluru selesai, DAN tidak sedang overheat
            if (fireRequested && Time.time >= nextFireTime && !isOverheated)
            {
                Shoot();
                nextFireTime = Time.time + (weaponData != null ? weaponData.fireRate : 0.5f);
            }
        }

        private void HandleOverheatLogic()
        {
            if (GameSettings.SelectedMode != GameMode.Overheat || weaponData == null) return;

            if (isOverheated) return;

            if (currentHeat > 0)
            {
                currentHeat -= weaponData.coolDownRate * Time.deltaTime;
                currentHeat = Mathf.Max(0, currentHeat);
                OnHeatChanged?.Invoke(currentHeat, 100f);
            }
        }

        private void RotateTowardsMouse()
        {
            if (Mouse.current == null) return;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 lookDir = (Vector2)mousePos - (Vector2)transform.position;
            
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }

        private void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        private void OnFire(InputValue value)
        {
            isFiring = value.isPressed;
        }

        private void Shoot()
        {
            if (weaponData == null || firePoint == null) return;

            // Spawn peluru
            PoolManager.Instance.GetPooledObject(weaponData.bulletPoolKey, firePoint.position, firePoint.rotation);

            // Tambah panas (Hanya di mode Overheat)
            if (GameSettings.SelectedMode == GameMode.Overheat)
            {
                currentHeat += weaponData.heatPerShot;
                OnHeatChanged?.Invoke(currentHeat, 100f);

                if (currentHeat >= 100f)
                {
                    StartCoroutine(OverheatRoutine());
                }
            }
        }

        private IEnumerator OverheatRoutine()
        {
            isOverheated = true;
            Debug.Log("<color=red>SENJATA OVERHEAT! MENUNGGU PENDINGINAN...</color>");

            // Opsional: Ubah warna sprite jadi merah membara saat overheat
            if (spriteRenderer != null) spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(weaponData.overheatCooldownDuration);

            currentHeat = 0f;
            isOverheated = false;
            
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            OnHeatChanged?.Invoke(currentHeat, 100f);
            Debug.Log("<color=green>SENJATA SIAP DIGUNAKAN KEMBALI.</color>");
        }

        void FixedUpdate()
        {
            // Menggunakan linearVelocity (Sesuai Unity 2023+)
            rb.linearVelocity = moveInput * moveSpeed;
        }

        void LateUpdate()
        {
            // Screen Clamping (tetap sama)
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
            transform.position = clampedPosition;
        }

        private void CalculateCameraBounds()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;
            minBounds = mainCam.ViewportToWorldPoint(new Vector3(0.05f, 0.05f, 0)); 
            maxBounds = mainCam.ViewportToWorldPoint(new Vector3(0.95f, 0.95f, 0));
        }

        // IMPLEMENTASI INTERFACE IDamageable
        public void TakeDamage(float amount)
        {
            // Jangan terima damage kalau sedang i-frames atau sudah mati
            if (isInvincible || currentHealth <= 0) return;

            currentHealth -= amount;
            
            // Beritahu UI kalau HP berubah
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Memicu Screen Shake saat terkena hit (Polish & Juice Plus Point)
            CameraShake.TriggerShake(0.2f, 0.35f);
            // Memicu Hit-Stop (Freeze Frame) untuk bobot impact berat
            CameraShake.TriggerHitStop(0.06f);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Mulai efek I-frames (Berdip & Kebal)
                StartCoroutine(InvincibilityRoutine());
            }
        }

        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;

            // Logika kedap-kedip (Blinking)
            int blinks = 5; // Jumlah kedipan
            float blinkDuration = invincibilityDuration / (blinks * 2);

            for (int i = 0; i < blinks; i++)
            {
                if (spriteRenderer != null) spriteRenderer.color = new Color(1, 0, 0, 0.5f); // Merah Transparan
                yield return new WaitForSeconds(blinkDuration);
                if (spriteRenderer != null) spriteRenderer.color = Color.gray; // Normal
                yield return new WaitForSeconds(blinkDuration);
            }

            isInvincible = false;
        }

        public void ResetPlayer()
        {
            currentHealth = maxHealth;
            transform.position = Vector3.zero;
            gameObject.SetActive(true);
            isInvincible = false;
            
            // Update UI
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            Debug.Log("Player Mati! Game Over.");
            
            // Beritahu semua sistem kalau Player mati
            OnPlayerDeath?.Invoke();

            // Untuk sementara, kita matikan objeknya
            gameObject.SetActive(false);
        }
    }
}