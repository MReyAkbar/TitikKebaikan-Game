using System.Collections;
using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    [Header("Game Background Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip gameBackgroundMusic;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.22f;
    [SerializeField] private float musicFadeInDuration = 2f;

    [Header("Loop Ambience")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip ambienceLoop;
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.2f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float fadeInDuration = 1.5f;

    [Header("Random Traffic SFX")]
    [SerializeField] private AudioSource trafficSource;
    [SerializeField] private AudioClip[] randomTrafficClips;
    [SerializeField] private Vector2 randomDelayRange = new Vector2(6f, 14f);
    [SerializeField, Range(0f, 1f)] private float randomTrafficVolume = 0.35f;
    [SerializeField] private Vector2 randomPitchRange = new Vector2(0.95f, 1.05f);

    [Header("Pause")]
    [SerializeField] private bool lowerVolumeWhenPaused = true;
    [SerializeField, Range(0f, 1f)] private float pausedVolumeMultiplier = 0.35f;

    private Coroutine randomTrafficRoutine;
    private Coroutine ambienceFadeRoutine;
    private Coroutine musicFadeRoutine;
    private bool isPaused;

    private void Awake()
    {
        EnsureAudioSources();
        SetupMusicSource();
        SetupAmbienceSource();
        SetupTrafficSource();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
    }

    private void Start()
    {
        if (playOnStart)
            PlayAmbience();
    }

    public void PlayAmbience()
    {
        if (musicSource != null && gameBackgroundMusic != null)
        {
            musicSource.clip = gameBackgroundMusic;
            musicSource.loop = true;
            musicSource.Play();

            StartMusicFade(GetTargetMusicVolume());
        }

        if (ambienceSource != null && ambienceLoop != null)
        {
            ambienceSource.clip = ambienceLoop;
            ambienceSource.loop = true;
            ambienceSource.Play();

            StartAmbienceFade(GetTargetAmbienceVolume());
        }

        if (randomTrafficRoutine == null && randomTrafficClips != null && randomTrafficClips.Length > 0)
            randomTrafficRoutine = StartCoroutine(RandomTrafficLoop());
    }

    public void StopAmbience()
    {
        if (musicSource != null)
            musicSource.Stop();

        if (ambienceSource != null)
            ambienceSource.Stop();

        if (randomTrafficRoutine != null)
        {
            StopCoroutine(randomTrafficRoutine);
            randomTrafficRoutine = null;
        }
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            GameObject sourceObject = new GameObject("MusicSource");
            sourceObject.transform.SetParent(transform);
            musicSource = sourceObject.AddComponent<AudioSource>();
        }

        if (ambienceSource == null)
        {
            GameObject sourceObject = new GameObject("AmbienceSource");
            sourceObject.transform.SetParent(transform);
            ambienceSource = sourceObject.AddComponent<AudioSource>();
        }

        if (trafficSource == null)
        {
            GameObject sourceObject = new GameObject("RandomTrafficSource");
            sourceObject.transform.SetParent(transform);
            trafficSource = sourceObject.AddComponent<AudioSource>();
        }
    }

    private void SetupMusicSource()
    {
        if (musicSource == null) return;

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0f;
    }

    private void SetupAmbienceSource()
    {
        if (ambienceSource == null) return;

        ambienceSource.playOnAwake = false;
        ambienceSource.loop = true;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.volume = 0f;
    }

    private void SetupTrafficSource()
    {
        if (trafficSource == null) return;

        trafficSource.playOnAwake = false;
        trafficSource.loop = false;
        trafficSource.spatialBlend = 0f;
    }

    private IEnumerator RandomTrafficLoop()
    {
        while (true)
        {
            float delay = Random.Range(randomDelayRange.x, randomDelayRange.y);
            yield return new WaitForSecondsRealtime(delay);

            if (isPaused)
                continue;

            if (trafficSource == null || randomTrafficClips == null || randomTrafficClips.Length == 0)
                continue;

            AudioClip clip = randomTrafficClips[Random.Range(0, randomTrafficClips.Length)];
            if (clip == null) continue;

            trafficSource.pitch = Random.Range(randomPitchRange.x, randomPitchRange.y);
            trafficSource.PlayOneShot(clip, randomTrafficVolume);
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        isPaused = state == GameManager.GameState.Paused;
        StartMusicFade(GetTargetMusicVolume());
        StartAmbienceFade(GetTargetAmbienceVolume());
    }

    private float GetTargetMusicVolume()
    {
        if (lowerVolumeWhenPaused && isPaused)
            return musicVolume * pausedVolumeMultiplier;

        return musicVolume;
    }

    private float GetTargetAmbienceVolume()
    {
        if (lowerVolumeWhenPaused && isPaused)
            return ambienceVolume * pausedVolumeMultiplier;

        return ambienceVolume;
    }

    private void StartMusicFade(float targetVolume)
    {
        if (musicSource == null) return;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeSource(musicSource, targetVolume, musicFadeInDuration, true));
    }

    private void StartAmbienceFade(float targetVolume)
    {
        if (ambienceSource == null) return;

        if (ambienceFadeRoutine != null)
            StopCoroutine(ambienceFadeRoutine);

        ambienceFadeRoutine = StartCoroutine(FadeSource(ambienceSource, targetVolume, fadeInDuration, false));
    }

    private IEnumerator FadeSource(AudioSource source, float targetVolume, float fadeDuration, bool isMusic)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;

        if (isMusic)
            musicFadeRoutine = null;
        else
            ambienceFadeRoutine = null;
    }
}
