using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace BulletHell.UI
{
    /// <summary>
    /// Komponen untuk tombol menu individual.
    /// Menampilkan deskripsi saat di-hover dan memberikan efek visual ringan.
    /// Pasang di setiap Button yang butuh hover description.
    /// </summary>
    public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Referensi")]
        [SerializeField] private MainMenuUI mainMenu;

        [Header("Deskripsi (hanya untuk tombol mode)")]
        [TextArea(2, 4)]
        [SerializeField] private string hoverDescription;

        [Header("Visual Hover")]
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
            // Efek visual: sedikit membesar
            transform.localScale = originalScale * hoverScale;

            // Efek visual: ubah warna teks
            if (buttonText != null) buttonText.color = hoverTextColor;

            // Tampilkan deskripsi jika ada
            if (mainMenu != null && !string.IsNullOrEmpty(hoverDescription))
            {
                mainMenu.ShowDescription(hoverDescription);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Kembalikan ke ukuran normal
            transform.localScale = originalScale;

            // Kembalikan warna teks
            if (buttonText != null) buttonText.color = originalTextColor;

            // Sembunyikan deskripsi
            if (mainMenu != null)
            {
                mainMenu.HideDescription();
            }
        }
    }
}
