using UnityEngine;
using System.Collections;
using BulletHell.Managers;

namespace BulletHell.VFX
{
    public class ShatterExplosion : MonoBehaviour
    {
        [Header("Shatter Settings")]
        [SerializeField] private int pieceCount = 10;
        [SerializeField] private float minSpeed = 4f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float lifetime = 0.5f;
        [SerializeField] private Color defaultColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private Transform[] pieces;
        private Vector2[] velocities;
        private SpriteRenderer[] renderers;
        private bool isInitialized = false;
        private Color currentColor;

        private static Sprite sharedSquareSprite;

        private void Awake()
        {
            Initialize();
            currentColor = defaultColor;
        }

        public void Setup(Color color)
        {
            currentColor = color;
            // Update warna segera jika sudah enable
            if (renderers != null)
            {
                foreach (var sr in renderers) sr.color = color;
            }
        }

        private void Initialize()
        {
            if (isInitialized) return;

            // Optimasi memori: Bagikan 1 tekstur putih murni untuk semua pecahan
            if (sharedSquareSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                sharedSquareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }

            pieces = new Transform[pieceCount];
            velocities = new Vector2[pieceCount];
            renderers = new SpriteRenderer[pieceCount];

            for (int i = 0; i < pieceCount; i++)
            {
                GameObject pieceObj = new GameObject($"ShatterPiece_{i}");
                pieceObj.transform.SetParent(transform);
                pieceObj.transform.localPosition = Vector3.zero;

                SpriteRenderer sr = pieceObj.AddComponent<SpriteRenderer>();
                sr.sprite = sharedSquareSprite;
                sr.color = defaultColor;

                pieces[i] = pieceObj.transform;
                renderers[i] = sr;
            }

            isInitialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            // Atur ulang posisi ke tengah dan tembakkan dengan arah/kecepatan acak
            for (int i = 0; i < pieceCount; i++)
            {
                pieces[i].localPosition = Vector3.zero;
                // Skala acak memberikan kesan pecahan bervariasi
                pieces[i].localScale = Vector3.one * Random.Range(0.15f, 0.4f);
                renderers[i].color = currentColor;

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float speed = Random.Range(minSpeed, maxSpeed);
                velocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            }

            StartCoroutine(AnimateShatterRoutine());
        }

        private IEnumerator AnimateShatterRoutine()
        {
            float elapsed = 0f;
            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / lifetime;
                float alpha = 1f - progress; // Fade out mulus

                for (int i = 0; i < pieceCount; i++)
                {
                    // Terbang menyebar
                    pieces[i].localPosition += (Vector3)velocities[i] * Time.deltaTime;
                    // Berputar liar
                    pieces[i].Rotate(0, 0, Random.Range(-400f, 400f) * Time.deltaTime);

                    Color c = renderers[i].color;
                    c.a = alpha;
                    renderers[i].color = c;
                }

                yield return null;
            }

            // Selesai memudar, kembalikan ke gudang Pool
            PoolManager.Instance.ReturnToPool(gameObject);
        }
    }
}
