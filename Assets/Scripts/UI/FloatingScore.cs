using UnityEngine;
using TMPro; // Pastikan TextMeshPro sudah terinstal di project-mu

namespace BulletHell.UI
{
    public class FloatingScore : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float fadeDuration = 1f;

        private Color originalColor;
        private float timer;

        void Awake()
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
            originalColor = textMesh.color;
        }

        public void Setup(int scoreValue)
        {
            textMesh.text = "+" + scoreValue;
            textMesh.color = originalColor;
            timer = 0;
        }

        void Update()
        {
            // Melayang ke atas
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

            // Efek menghilang (Fade out)
            timer += Time.deltaTime;
            float alpha = 1 - (timer / fadeDuration);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            if (timer >= fadeDuration)
            {
                // Kembali ke pool (Sistem kita pakai PooledObject)
                gameObject.SetActive(false);
                Managers.PoolManager.Instance.ReturnToPool(gameObject);
            }
        }
    }
}
