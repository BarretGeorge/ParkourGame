using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 音频管理器 - 统一管理所有游戏音频
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private int sfxSourcePoolSize = 10;

    [Header("音量设置")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.8f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("淡入淡出设置")]
    [SerializeField] private float fadeDuration = 1f;

    // 音频源池
    private List<AudioSource> sfxSourcePool = new List<AudioSource>();

    // 音频库
    private AudioLibrary audioLibrary;

    // 单例
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;

    // 属性
    public float MasterVolume
    {
        get => masterVolume;
        set => masterVolume = Mathf.Clamp01(value);
    }

    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = Mathf.Clamp01(value);
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set => sfxVolume = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            LoadAudioLibrary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        // 创建SFX对象池
        for (int i = 0; i < sfxSourcePoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            sfxSourcePool.Add(source);
        }

        UpdateVolumes();
    }

    private void LoadAudioLibrary()
    {
        audioLibrary = FindObjectOfType<AudioLibrary>();
        if (audioLibrary == null)
        {
            Debug.LogWarning("AudioLibrary not found in scene. Sound effects may not work.");
        }
    }

    private void UpdateVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }

        foreach (AudioSource source in sfxSourcePool)
        {
            source.volume = masterVolume * sfxVolume;
        }
    }

    #region 背景音乐控制

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StartCoroutine(CrossFadeMusic(clip));
    }

    public void PlayMusic(string musicName)
    {
        if (audioLibrary != null)
        {
            AudioClip clip = audioLibrary.GetMusic(musicName);
            PlayMusic(clip);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeOutMusic());
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        if (musicSource == null) yield break;

        // 淡出当前音乐
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < fadeDuration / 2 && musicSource.isPlaying)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / (fadeDuration / 2));
            yield return null;
        }

        // 切换音乐
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // 淡入新音乐
        timer = 0f;
        while (timer < fadeDuration / 2)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, masterVolume * musicVolume, timer / (fadeDuration / 2));
            yield return null;
        }

        musicSource.volume = masterVolume * musicVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < fadeDuration && musicSource.isPlaying)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = masterVolume * musicVolume;
    }

    #endregion

    #region 音效播放

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource availableSource = GetAvailableSFXSource();
        if (availableSource != null)
        {
            availableSource.clip = clip;
            availableSource.Play();
        }
    }

    public void PlaySFX(string sfxName)
    {
        if (audioLibrary != null)
        {
            AudioClip clip = audioLibrary.GetSFX(sfxName);
            PlaySFX(clip);
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;

        AudioSource availableSource = GetAvailableSFXSource();
        if (availableSource != null)
        {
            availableSource.clip = clip;
            availableSource.volume = masterVolume * sfxVolume * volumeScale;
            availableSource.Play();
        }
    }

    public void PlaySFX(string sfxName, float volumeScale)
    {
        if (audioLibrary != null)
        {
            AudioClip clip = audioLibrary.GetSFX(sfxName);
            PlaySFX(clip, volumeScale);
        }
    }

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        AudioSource availableSource = GetAvailableSFXSource();
        if (availableSource != null)
        {
            availableSource.transform.position = position;
            availableSource.spatialBlend = 1f;
            availableSource.clip = clip;
            availableSource.Play();
        }
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayOneShot(AudioClip clip, float volumeScale)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        // 查找未播放的音频源
        foreach (AudioSource source in sfxSourcePool)
        {
            if (!source.isPlaying)
            {
                source.volume = masterVolume * sfxVolume;
                source.spatialBlend = 0f;
                return source;
            }
        }

        // 如果都在播放，返回第一个（会打断当前播放）
        if (sfxSourcePool.Count > 0)
        {
            AudioSource firstSource = sfxSourcePool[0];
            firstSource.volume = masterVolume * sfxVolume;
            firstSource.spatialBlend = 0f;
            return firstSource;
        }

        return null;
    }

    #endregion

    #region 游戏音效快捷方法

    public void PlayFootstepSound()
    {
        PlaySFX("Footstep");
    }

    public void PlayJumpSound()
    {
        PlaySFX("Jump");
    }

    public void PlayLandSound()
    {
        PlaySFX("Land");
    }

    public void PlayCollectCoinSound(CoinType coinType)
    {
        string soundName = "CollectCoin";
        switch (coinType)
        {
            case CoinType.Bronze:
                soundName = "CollectBronze";
                break;
            case CoinType.Silver:
                soundName = "CollectSilver";
                break;
            case CoinType.Gold:
                soundName = "CollectGold";
                break;
            case CoinType.Platinum:
                soundName = "CollectPlatinum";
                break;
            case CoinType.Diamond:
                soundName = "CollectDiamond";
                break;
        }
        PlaySFX(soundName);
    }

    public void PlayPowerUpSound(PowerUpType powerUpType)
    {
        string soundName = "CollectPowerUp";
        switch (powerUpType)
        {
            case PowerUpType.Magnet:
                soundName = "CollectMagnet";
                break;
            case PowerUpType.Shield:
                soundName = "CollectShield";
                break;
            case PowerUpType.SpeedBoost:
                soundName = "CollectSpeedBoost";
                break;
            case PowerUpType.Invulnerability:
                soundName = "CollectInvulnerability";
                break;
        }
        PlaySFX(soundName);
    }

    public void PlayCollisionSound()
    {
        PlaySFX("Collision");
    }

    public void PlayDeathSound()
    {
        PlaySFX("Death");
    }

    public void PlayUIButtonSound()
    {
        PlaySFX("UIButton");
    }

    public void PlayUICancelSound()
    {
        PlaySFX("UICancel");
    }

    public void PlayUIConfirmSound()
    {
        PlaySFX("UIConfirm");
    }

    #endregion

    #region 音量控制

    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
        UpdateVolumes();
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;
        UpdateVolumes();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        UpdateVolumes();
        SaveVolumeSettings();
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume");
        }
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume");
        }
        UpdateVolumes();
    }

    #endregion

    #region 工具方法

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    public AudioClip GetCurrentMusic()
    {
        return musicSource != null ? musicSource.clip : null;
    }

    public void StopAllSFX()
    {
        foreach (AudioSource source in sfxSourcePool)
        {
            if (source != null)
            {
                source.Stop();
            }
        }

        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    #endregion
}
