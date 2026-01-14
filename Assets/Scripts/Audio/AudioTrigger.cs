using UnityEngine;

/// <summary>
/// 音频触发器 - 进入区域时播放音效
/// </summary>
public class AudioTrigger : MonoBehaviour
{
    [Header("音效设置")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private string sfxName;
    [SerializeField] private bool playOnEnter = true;
    [SerializeField] private bool playOnExit = false;
    [SerializeField] private bool playOnStay = false;
    [SerializeField] private float stayInterval = 1f;

    [Header("触发设置")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float volumeScale = 1f;

    [Header("3D音效")]
    [SerializeField] private bool use3DSound = false;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;

    private bool hasTriggered = false;
    private float lastStayTime;

    private void OnTriggerEnter(Collider other)
    {
        if (!playOnEnter) return;
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnce && hasTriggered) return;

        PlayAudio();
        hasTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playOnExit) return;
        if (!other.CompareTag(playerTag)) return;

        PlayAudio();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!playOnStay) return;
        if (!other.CompareTag(playerTag)) return;
        if (Time.time - lastStayTime < stayInterval) return;

        PlayAudio();
        lastStayTime = Time.time;
    }

    private void PlayAudio()
    {
        if (AudioManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(sfxName))
            {
                if (use3DSound)
                {
                    AudioManager.Instance.PlaySFX(sfxName, transform.position);
                }
                else
                {
                    AudioManager.Instance.PlaySFX(sfxName, volumeScale);
                }
            }
            else if (audioClip != null)
            {
                if (use3DSound)
                {
                    AudioManager.Instance.PlaySFX(audioClip, transform.position);
                }
                else
                {
                    AudioManager.Instance.PlaySFX(audioClip, volumeScale);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (use3DSound)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, minDistance);

            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }

        if (GetComponent<Collider>() != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
