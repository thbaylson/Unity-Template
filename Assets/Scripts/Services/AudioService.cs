using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public interface IAudioService
{
    float MusicVolumeNormalized { get; }
    float SfxVolumeNormalized { get; }

    event Action<float> MusicVolumeChanged;
    event Action<float> SfxVolumeChanged;

    void SetMusicVolume(float normalized01);
    void SetSfxVolume(float normalized01);

    AudioClip CurrentMusicClip { get; }

    void PlayMusic(AudioClip clip, float crossfadeSeconds = -1f, bool loop = true);
    void StopMusic(float fadeOutSeconds = -1f);

    void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f);
    void PlaySfxAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f, float pitch = 1f, float minDistance = 1f, float maxDistance = 20f
);

}

/// <summary>
/// This service manages music and sound effects, including scene-based music management, crossfading, and volume control via an AudioMixer.
/// On scene load, it looks for a MusicConfig component to determine what to do with the music: override to a new song, 
/// keep the same song playing, or stop the music for the current scene.
/// </summary>
public class AudioService : MonoBehaviour, IAudioService
{
    private const float MinDb = -80f;
    private const float MaxDb = 0f;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SfxVolume";

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Music")]
    [SerializeField, Min(0f)] private float crossfadeSeconds = 1.0f;
    [SerializeField] private bool useUnscaledTimeForFades = true;

    [Header("SFX")]
    [SerializeField, Min(1)] private int sfxPoolSize = 8;

    [Header("3D SFX")]
    [SerializeField, Min(1)] private int sfx3dPoolSize = 8;

    public float MusicVolumeNormalized { get; private set; } = 1f;
    public float SfxVolumeNormalized { get; private set; } = 1f;

    public event Action<float> MusicVolumeChanged;
    public event Action<float> SfxVolumeChanged;

    public AudioClip CurrentMusicClip => _activeMusic != null ? _activeMusic.clip : null;

    private AudioSource _musicA;
    private AudioSource _musicB;
    private AudioSource _activeMusic;
    private AudioSource _inactiveMusic;

    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private int _sfxPoolIndex;
    
    private List<AudioSource> _sfx3dPool = new List<AudioSource>();
    private int _sfx3dPoolIndex;

    private Coroutine _musicFadeRoutine;

    private void Awake()
    {
        if (Services.AudioService != null)
        {
            Destroy(gameObject);
            return;
        }

        Services.AudioService = this;

        CreateMusicSources();
        CreateSfxPool();
        CreateSfx3dPool();
        LoadVolumes();
        ApplyMixerVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplySceneMusic(SceneManager.GetActiveScene());
    }

    private void CreateMusicSources()
    {
        _musicA = CreateChildAudioSource("MusicSource_A", musicGroup);
        _musicB = CreateChildAudioSource("MusicSource_B", musicGroup);

        _musicA.loop = true;
        _musicB.loop = true;

        _musicA.playOnAwake = false;
        _musicB.playOnAwake = false;

        _activeMusic = _musicA;
        _inactiveMusic = _musicB;
    }

    private void CreateSfxPool()
    {
        _sfxPool.Clear();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var src = CreateChildAudioSource($"SfxSource_{i}", sfxGroup);
            src.playOnAwake = false;
            _sfxPool.Add(src);
        }
        _sfxPoolIndex = 0;
    }

    private void CreateSfx3dPool()
    {
        _sfx3dPool.Clear();

        for (int i = 0; i < sfx3dPoolSize; i++)
        {
            var src = CreateChildAudioSource($"Sfx3DSource_{i}", sfxGroup);
            src.playOnAwake = false;

            // Defaults for 3D positional one-shots
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.dopplerLevel = 0f;

            _sfx3dPool.Add(src);
        }

        _sfx3dPoolIndex = 0;
    }

    private AudioSource CreateChildAudioSource(string name, AudioMixerGroup group)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = group;
        return src;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneMusic(scene);
    }

    private void ApplySceneMusic(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded) return;

        var settings = FindMusicConfig(scene);

        // If a scene doesn't opt-in with MusicConfig, default is: do nothing (keep current).
        if (settings == null) return;

        switch (settings.mode)
        {
            case MusicConfig.MusicMode.KeepCurrent:
                return;

            case MusicConfig.MusicMode.Stop:
                StopMusic(crossfadeSeconds);
                return;

            case MusicConfig.MusicMode.Override:
                PlayMusic(settings.musicClip, crossfadeSeconds, settings.loop);
                return;
        }
    }

    private static MusicConfig FindMusicConfig(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            // Root check
            if (root.TryGetComponent(out MusicConfig s)) return s;

            // Children (including inactive)
            s = root.GetComponentInChildren<MusicConfig>(true);
            if (s != null) return s;
        }

        return null;
    }

    public void PlayMusic(AudioClip clip, float crossfadeSeconds = -1f, bool loop = true)
    {
        if (clip == null)
        {
            StopMusic(crossfadeSeconds);
            return;
        }

        // If already playing this clip, do nothing.
        if (_activeMusic.isPlaying && _activeMusic.clip == clip) return;

        float fade = (crossfadeSeconds >= 0f) ? crossfadeSeconds : this.crossfadeSeconds;

        _inactiveMusic.clip = clip;
        _inactiveMusic.loop = loop;
        _inactiveMusic.volume = 0f;
        _inactiveMusic.Play();

        StartMusicFade(_activeMusic, _inactiveMusic, fade);

        // Swap roles
        (_activeMusic, _inactiveMusic) = (_inactiveMusic, _activeMusic);
    }

    public void StopMusic(float fadeOutSeconds = -1f)
    {
        if (!_activeMusic.isPlaying && !_inactiveMusic.isPlaying) return;

        float fade = (fadeOutSeconds >= 0f) ? fadeOutSeconds : crossfadeSeconds;

        // Fade active to 0; ensure inactive is stopped too.
        if (_inactiveMusic.isPlaying) _inactiveMusic.Stop();

        StartMusicFade(_activeMusic, null, fade);
    }

    private void StartMusicFade(AudioSource fadeOut, AudioSource fadeIn, float seconds)
    {
        if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
        _musicFadeRoutine = StartCoroutine(FadeRoutine(fadeOut, fadeIn, seconds));
    }

    private System.Collections.IEnumerator FadeRoutine(AudioSource fadeOut, AudioSource fadeIn, float seconds)
    {
        float t = 0f;
        float outStart = fadeOut != null ? fadeOut.volume : 0f;
        float inStart = fadeIn != null ? fadeIn.volume : 0f;

        if (seconds <= 0f)
        {
            if (fadeOut != null)
            {
                fadeOut.volume = 0f;
                fadeOut.Stop();
                fadeOut.clip = null;
            }

            if (fadeIn != null) fadeIn.volume = 1f;

            _musicFadeRoutine = null;
            yield break;
        }

        while (t < seconds)
        {
            float dt = useUnscaledTimeForFades ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;

            float a = Mathf.Clamp01(t / seconds);

            if (fadeOut != null) fadeOut.volume = Mathf.Lerp(outStart, 0f, a);
            if (fadeIn != null) fadeIn.volume = Mathf.Lerp(inStart, 1f, a);

            yield return null;
        }

        if (fadeOut != null)
        {
            fadeOut.volume = 0f;
            fadeOut.Stop();
            fadeOut.clip = null;
        }

        if (fadeIn != null) fadeIn.volume = 1f;

        _musicFadeRoutine = null;
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        if (_sfxPool.Count == 0) return;

        var src = _sfxPool[_sfxPoolIndex];
        _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Count;

        src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        src.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlaySfxAtPoint(
        AudioClip clip,
        Vector3 position,
        float volumeScale = 1f,
        float pitch = 1f,
        float minDistance = 1f,
        float maxDistance = 20f)
    {
        if (clip == null) return;
        if (_sfx3dPool.Count == 0) return;

        var src = _sfx3dPool[_sfx3dPoolIndex];
        _sfx3dPoolIndex = (_sfx3dPoolIndex + 1) % _sfx3dPool.Count;

        src.transform.position = position;

        // Set per-play parameters safely (we selected a free or stolen source).
        src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        src.minDistance = Mathf.Max(0.01f, minDistance);
        src.maxDistance = Mathf.Max(src.minDistance, maxDistance);

        src.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void SetMusicVolume(float normalized01)
    {
        MusicVolumeNormalized = Mathf.Clamp01(normalized01);
        ApplyMixerVolumes();

        MusicVolumeChanged?.Invoke(MusicVolumeNormalized);

        SaveVolumes();
    }

    public void SetSfxVolume(float normalized01)
    {
        SfxVolumeNormalized = Mathf.Clamp01(normalized01);
        ApplyMixerVolumes();

        SfxVolumeChanged?.Invoke(SfxVolumeNormalized);

        SaveVolumes();
    }

    private void LoadVolumes()
    {
        // TODO: Implement later.
    }

    private void SaveVolumes()
    {
        // TODO: Implement later.
    }

    private void ApplyMixerVolumes()
    {
        if (audioMixer == null) return;

        audioMixer.SetFloat(musicVolumeParameter, NormalizedToDb(MusicVolumeNormalized));
        audioMixer.SetFloat(sfxVolumeParameter, NormalizedToDb(SfxVolumeNormalized));
    }

    private static float NormalizedToDb(float normalized01)
    {
        float t = Mathf.Clamp01(normalized01);

        // Log-ish curve: 0..1 mapped to 0..1 in a more perceptual way
        // curve = log10(1 + 9t) -> 0..1
        float curve = Mathf.Log10(1f + 9f * t);

        return Mathf.Lerp(MinDb, MaxDb, curve);
    }
}