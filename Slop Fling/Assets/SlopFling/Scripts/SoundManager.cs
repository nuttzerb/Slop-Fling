using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private SoundLibrary library;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private const string PREF_BGM_MUTE = "SF_BGM_MUTE";
    private const string PREF_SFX_MUTE = "SF_SFX_MUTE";

    public bool IsBgmMuted { get; private set; }
    public bool IsSfxMuted { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!bgmSource)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }

        LoadMuteState();
    }

    private void LoadMuteState()
    {
        IsBgmMuted = PlayerPrefs.GetInt(PREF_BGM_MUTE, 0) == 1;
        IsSfxMuted = PlayerPrefs.GetInt(PREF_SFX_MUTE, 0) == 1;

        if (bgmSource) bgmSource.mute = IsBgmMuted;
        if (sfxSource) sfxSource.mute = IsSfxMuted;
    }

    public void SetBgmMute(bool mute)
    {
        IsBgmMuted = mute;
        if (bgmSource) bgmSource.mute = mute;
        PlayerPrefs.SetInt(PREF_BGM_MUTE, mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSfxMute(bool mute)
    {
        IsSfxMuted = mute;
        if (sfxSource) sfxSource.mute = mute;
        PlayerPrefs.SetInt(PREF_SFX_MUTE, mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ===== PLAY BGM =====

    public void PlayBgm(SoundId id, bool loop = true)
    {
        if (!library || !bgmSource) return;

        var clip = library.GetClip(id, out float vol);
        if (!clip) return;

        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.volume = vol;
        if (!IsBgmMuted)
            bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource) bgmSource.Stop();
    }

    // ===== PLAY SFX =====

    public void PlaySfx(SoundId id)
    {
        if (!library || !sfxSource) return;
        if (IsSfxMuted) return;

        var clip = library.GetClip(id, out float vol);
        if (!clip) return;

        sfxSource.PlayOneShot(clip, vol);
    }

    public void PlaySfxAt(SoundId id, Vector3 position)
    {
        if (!library || IsSfxMuted) return;

        var clip = library.GetClip(id, out float vol);
        if (!clip) return;

        AudioSource.PlayClipAtPoint(clip, position, vol);
    }
}
