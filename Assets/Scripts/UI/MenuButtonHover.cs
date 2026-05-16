using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using BulletHell.Managers;

namespace BulletHell.UI
{
    // Hover effects
    public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private MainMenuUI mainMenu;

        [Header("Description")]
        [TextArea(2, 4)]
        [SerializeField] private string hoverDescription;

        [Header("Visual")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private Color hoverTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private Vector3 originalScale;
        private TextMeshProUGUI buttonText;
        private Color originalTextColor;

        void Awake()
        {
            originalScale = transform.localScale;
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) originalTextColor = buttonText.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = originalScale * hoverScale;
            if (buttonText != null) buttonText.color = hoverTextColor;
            
            AudioManager.PlayUIHover();

            if (mainMenu != null && !string.IsNullOrEmpty(hoverDescription))
                mainMenu.ShowDescription(hoverDescription);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = originalScale;
            if (buttonText != null) buttonText.color = originalTextColor;

            if (mainMenu != null)
                mainMenu.HideDescription();
        }
    }
}
