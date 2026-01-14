using UnityEngine;

/// <summary>
/// 声音发射器 - 持续发射声音（如环境音）
/// </summary>
public class SoundEmitter : MonoBehaviour
{
    [Header("音效设置")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnAwake = false;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitch = 1f;
    [SerializeField] private float spatialBlend = 1f;

    [Header("3D声音设置")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    private AudioSource audioSource;

    private void Awake()
    {
        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.loop = loop;
        audioSource.playOnAwake = playOnAwake;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.spatialBlend = spatialBlend;

        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = rolloffMode;

        if (playOnAwake)
        {
            audioSource.Play();
        }
    }

    public void Play()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void Pause()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    public void Resume()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    public void SetVolume(float newVolume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(newVolume);
        }
    }

    public void SetPitch(float newPitch)
    {
        if (audioSource != null)
        {
            audioSource.pitch = Mathf.Clamp(newPitch, -3f, 3f);
        }
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    private void OnDrawGizmos()
    {
        // 显示音频范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
