using UnityEngine;
using UnityEngine.UI;
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
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                player.OnHealthChanged += UpdateUI;
            }
        }

        void OnDisable()
        {
            if (player != null) player.OnHealthChanged -= UpdateUI;
        }

        private void UpdateUI(float currentHealth, float maxHealth)
        {
            if (healthSlider == null) return;

            float healthPercent = currentHealth / maxHealth;
            healthSlider.value = healthPercent;

            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercent);
            }
        }
    }
}
