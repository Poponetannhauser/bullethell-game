using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace BulletHell.Managers
{
    // Central manager for playing SFX and looping BGM with smooth crossfade
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSourceA;
        [SerializeField] private AudioSource musicSourceB;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Settings")]
        [SerializeField] private float crossfadeDuration = 1.5f;

        [Header("Background Music")]
        public AudioClip mainMenuBGM;
        public AudioClip gameplayBGM;

        [Header("SFX Clips")]
        public AudioClip playerShoot;
        public AudioClip playerHit;
        public AudioClip playerDeath;
        public AudioClip enemyDeath;
        public AudioClip bossExplosion;
        public AudioClip overheatAlarm;
        public AudioClip uiClick;
        public AudioClip uiHover;

        // Tracks which AudioSource is currently the "active" one
        private AudioSource activeSource;
        private Coroutine crossfadeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                activeSource = musicSourceA;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AudioClip targetClip = scene.name == "MainMenu" ? mainMenuBGM : gameplayBGM;
            CrossfadeTo(targetClip);
        }

        // Smoothly transitions from current music to a new clip
        public void CrossfadeTo(AudioClip newClip)
        {
            if (newClip == null)
            {
                StopMusic();
                return;
            }

            // If the new clip is already playing, do nothing
            if (activeSource.clip == newClip && activeSource.isPlaying) return;

            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(newClip));
        }

        private IEnumerator CrossfadeRoutine(AudioClip newClip)
        {
            // Pick the inactive source to play the new music
            AudioSource nextSource = (activeSource == musicSourceA) ? musicSourceB : musicSourceA;

            nextSource.clip = newClip;
            nextSource.loop = true;
            nextSource.volume = 0f;
            nextSource.Play();

            float elapsed = 0f;
            float startVolume = activeSource.volume;

            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / crossfadeDuration;
                activeSource.volume = Mathf.Lerp(startVolume, 0f, t);
                nextSource.volume = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            activeSource.Stop();
            activeSource.volume = 1f;
            activeSource = nextSource;
        }

        public void StopMusic()
        {
            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            musicSourceA.Stop();
            musicSourceB.Stop();
        }

        public static void PlaySFX(AudioClip clip, float volume = 1f, float pitchRange = 0.1f)
        {
            if (Instance == null || clip == null) return;

            // Slight pitch randomization keeps repetitive sounds from sounding robotic
            Instance.sfxSource.pitch = 1f + Random.Range(-pitchRange, pitchRange);
            Instance.sfxSource.PlayOneShot(clip, volume);
        }

        public static void PlayUIHover() => PlaySFX(Instance?.uiHover, 0.5f, 0.05f);
        public static void PlayUIClick() => PlaySFX(Instance?.uiClick, 0.7f, 0.05f);
    }
}
