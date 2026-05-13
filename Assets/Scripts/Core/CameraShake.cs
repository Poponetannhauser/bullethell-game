using UnityEngine;
using System.Collections;

namespace BulletHell.Core
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                originalPosition = transform.localPosition;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Memicu guncangan kamera secara global dari skrip mana pun.
        /// </summary>
        public static void TriggerShake(float duration = 0.2f, float intensity = 0.3f)
        {
            if (Instance != null)
            {
                Instance.StartShake(duration, intensity);
            }
        }

        public void StartShake(float duration, float intensity)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalPosition;
            }
            shakeCoroutine = StartCoroutine(ShakeRoutine(duration, intensity));
        }

        private IEnumerator ShakeRoutine(float duration, float intensity)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                transform.localPosition = originalPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null; // Tunggu ke frame berikutnya
            }

            transform.localPosition = originalPosition;
            shakeCoroutine = null;
        }
    }
}
