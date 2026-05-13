using UnityEngine;
using System.Collections.Generic;

namespace BulletHell.VFX
{
    public class CyberGridBackground : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float gridSize = 2f;
        [SerializeField] private float lineThickness = 0.03f;
        [SerializeField] private Color lineColor = new Color(0.4f, 0.4f, 0.4f, 0.35f);
        [SerializeField] private float scrollSpeed = 1.2f;

        private List<Transform> horizontalLines = new List<Transform>();
        private float minY;
        private float maxY;
        private static Sprite sharedSprite;

        private void Start()
        {
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Dapatkan batas dunia layar secara dinamis
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

            // Beri kelonggaran padding agar wrapping garis mulus di luar batas layar
            float minX = bottomLeft.x - gridSize * 2f;
            float maxX = topRight.x + gridSize * 2f;
            minY = bottomLeft.y - gridSize * 2f;
            maxY = topRight.y + gridSize * 2f;

            float width = maxX - minX;
            float height = maxY - minY;

            // Berbagi tekstur 1x1 putih murni untuk efisiensi RAM mutlak
            if (sharedSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                sharedSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }

            // 1. Buat Pilar Garis Vertikal Statis (Membentuk jalur grid presisi)
            for (float x = Mathf.Floor(minX / gridSize) * gridSize; x <= maxX; x += gridSize)
            {
                CreateLinePiece(new Vector3(x, (minY + maxY) * 0.5f, 10f), new Vector3(lineThickness, height, 1f), false);
            }

            // 2. Buat Garis Horizontal Dinamis (Terus bergerak ke bawah seperti NieR:Automata UI)
            for (float y = Mathf.Floor(minY / gridSize) * gridSize; y <= maxY; y += gridSize)
            {
                Transform hLine = CreateLinePiece(new Vector3((minX + maxX) * 0.5f, y, 10f), new Vector3(width, lineThickness, 1f), true);
                horizontalLines.Add(hLine);
            }
        }

        private Transform CreateLinePiece(Vector3 position, Vector3 scale, bool isHorizontal)
        {
            GameObject lineObj = new GameObject(isHorizontal ? "CyberGrid_H" : "CyberGrid_V");
            lineObj.transform.SetParent(transform);
            lineObj.transform.position = position;
            lineObj.transform.localScale = scale;

            SpriteRenderer sr = lineObj.AddComponent<SpriteRenderer>();
            sr.sprite = sharedSprite;
            sr.color = lineColor;
            // Taruh di urutan paling belakang agar tidak menutupi peluru dan pemain
            sr.sortingOrder = -200;

            return lineObj.transform;
        }

        private void Update()
        {
            float moveDist = scrollSpeed * Time.deltaTime;
            float totalHeight = maxY - minY;

            // Gulir setiap garis horizontal ke bawah
            foreach (Transform line in horizontalLines)
            {
                line.position += Vector3.down * moveDist;

                // Ketika melewati batas bawah layar, bungkus ulang (wrap around) ke atas
                if (line.position.y < minY)
                {
                    line.position += Vector3.up * totalHeight;
                }
            }
        }
    }
}
