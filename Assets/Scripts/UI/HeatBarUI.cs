using UnityEngine;
using UnityEngine.UI;
using BulletHell.Player;
using BulletHell.Core;

namespace BulletHell.UI
{
    public class HeatBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider heatSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.red;

        private PlayerController player;

        void Awake()
        {
            if (GameSettings.SelectedMode != GameMode.Overheat)
            {
                gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                player.OnHeatChanged += UpdateUI;
            }
        }

        void OnDisable()
        {
            if (player != null) player.OnHeatChanged -= UpdateUI;
        }

        private void UpdateUI(float current, float max)
        {
            if (heatSlider == null) return;

            heatSlider.maxValue = max;
            heatSlider.value = current;

            if (fillImage != null)
            {
                float ratio = current / max;
                fillImage.color = Color.Lerp(normalColor, warningColor, ratio);
            }
        }
    }
}
