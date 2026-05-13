using UnityEngine;
using UnityEngine.UI;
using BulletHell.Gameplay;
using BulletHell.Player;

namespace BulletHell.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Color highHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        private PlayerController player;

        void OnEnable()
        {
            // Cari player di scene
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                
                // SUBSCRIBE (Berlangganan) ke event
                // Jadi setiap kali HP Player berubah, fungsi UpdateUI akan terpanggil otomatis
                player.OnHealthChanged += UpdateUI;
            }
        }

        void OnDisable()
        {
            // UNSUBSCRIBE (Penting! Biar nggak bocor memorinya)
            if (player != null)
            {
                player.OnHealthChanged -= UpdateUI;
            }
        }

        private void UpdateUI(float currentHealth, float maxHealth)
        {
            if (healthSlider == null) return;

            // Hitung persentase (0 sampai 1)
            float healthPercent = currentHealth / maxHealth;
            healthSlider.value = healthPercent;

            // Opsional: Ganti warna bar kalau darah mau habis
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercent);
            }
        }
    }
}
