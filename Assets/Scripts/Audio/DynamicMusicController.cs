using UnityEngine;
using System.Collections;

/// <summary>
/// 动态音乐控制器 - 根据游戏状态调整音乐
/// </summary>
public class DynamicMusicController : MonoBehaviour
{
    [Header("音乐设置")]
    [SerializeField] private float normalSpeed = 10f;
    [SerializeField] private float highSpeed = 20f;
    [SerializeField] private float dangerVolume = 0.3f;
    [SerializeField] private float excitementVolume = 1f;

    [Header("音频源")]
    [SerializeField] private AudioSource baseMusicSource;
    [SerializeField] private AudioSource intensityMusicSource;
    [SerializeField] private AudioSource percussionSource;

    private PlayerController playerController;
    private CollectibleManager collectibleManager;
    private float currentIntensity = 0f;

    private void Start()
    {
        FindManagers();
        SetupAudioSources();
    }

    private void FindManagers()
    {
        playerController = FindObjectOfType<PlayerController>();
        collectibleManager = FindObjectOfType<CollectibleManager>();
    }

    private void SetupAudioSources()
    {
        if (baseMusicSource != null)
        {
            baseMusicSource.loop = true;
        }

        if (intensityMusicSource != null)
        {
            intensityMusicSource.loop = true;
            intensityMusicSource.volume = 0f;
        }

        if (percussionSource != null)
        {
            percussionSource.loop = true;
            percussionSource.volume = 0f;
        }
    }

    private void Update()
    {
        if (playerController == null) return;

        CalculateIntensity();
        UpdateMusicLayers();
    }

    private void CalculateIntensity()
    {
        float speedIntensity = 0f;
        float powerUpIntensity = 0f;

        // 基于速度的强度
        if (playerController != null)
        {
            float currentSpeed = playerController.CurrentSpeed;
            speedIntensity = Mathf.InverseLerp(normalSpeed, highSpeed, currentSpeed);
        }

        // 基于道具的强度
        if (collectibleManager != null)
        {
            if (collectibleManager.HasShield || collectibleManager.HasMagnet)
            {
                powerUpIntensity += 0.2f;
            }

            SpeedBoostPowerUp speedBoost = FindObjectOfType<SpeedBoostPowerUp>();
            if (speedBoost != null && speedBoost.isActive)
            {
                powerUpIntensity += 0.3f;
            }
        }

        // 组合强度
        currentIntensity = Mathf.Clamp01(speedIntensity + powerUpIntensity);
    }

    private void UpdateMusicLayers()
    {
        // 基础音乐始终播放
        if (baseMusicSource != null && !baseMusicSource.isPlaying)
        {
            baseMusicSource.Play();
        }

        // 根据强度调整音乐层
        if (intensityMusicSource != null)
        {
            float targetVolume = currentIntensity * excitementVolume;
            intensityMusicSource.volume = Mathf.Lerp(intensityMusicSource.volume, targetVolume, Time.deltaTime * 2f);

            if (currentIntensity > 0.3f && !intensityMusicSource.isPlaying)
            {
                intensityMusicSource.Play();
            }
            else if (currentIntensity <= 0.1f && intensityMusicSource.isPlaying)
            {
                intensityMusicSource.Stop();
            }
        }

        if (percussionSource != null)
        {
            float targetVolume = Mathf.Clamp01((currentIntensity - 0.5f) * 2f) * excitementVolume;
            percussionSource.volume = Mathf.Lerp(percussionSource.volume, targetVolume, Time.deltaTime * 2f);

            if (currentIntensity > 0.6f && !percussionSource.isPlaying)
            {
                percussionSource.Play();
            }
            else if (currentIntensity <= 0.4f && percussionSource.isPlaying)
            {
                percussionSource.Stop();
            }
        }
    }

    public void SetIntensity(float intensity)
    {
        currentIntensity = Mathf.Clamp01(intensity);
    }

    public void PauseAllLayers()
    {
        if (baseMusicSource != null) baseMusicSource.Pause();
        if (intensityMusicSource != null) intensityMusicSource.Pause();
        if (percussionSource != null) percussionSource.Pause();
    }

    public void ResumeAllLayers()
    {
        if (baseMusicSource != null) baseMusicSource.UnPause();
        if (intensityMusicSource != null && currentIntensity > 0.3f) intensityMusicSource.UnPause();
        if (percussionSource != null && currentIntensity > 0.6f) percussionSource.UnPause();
    }

    public void StopAllLayers()
    {
        if (baseMusicSource != null) baseMusicSource.Stop();
        if (intensityMusicSource != null) intensityMusicSource.Stop();
        if (percussionSource != null) percussionSource.Stop();
    }
}
