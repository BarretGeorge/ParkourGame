using UnityEngine;
using System.Collections;

/// <summary>
/// 音频淡入淡出控制器
/// </summary>
public class AudioFader : MonoBehaviour
{
    [Header("音频源")]
    [SerializeField] private AudioSource targetAudioSource;

    [Header("淡入设置")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeInTargetVolume = 1f;

    [Header("淡出设置")]
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeOutTargetVolume = 0f;

    public void FadeIn()
    {
        FadeIn(fadeInDuration, fadeInTargetVolume);
    }

    public void FadeIn(float duration, float targetVolume)
    {
        if (targetAudioSource == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeInCoroutine(duration, targetVolume));
    }

    public void FadeOut()
    {
        FadeOut(fadeOutDuration, fadeOutTargetVolume);
    }

    public void FadeOut(float duration, float targetVolume)
    {
        if (targetAudioSource == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeOutCoroutine(duration, targetVolume));
    }

    public void CrossFade(AudioSource newSource, float duration)
    {
        if (targetAudioSource == null || newSource == null) return;

        StartCoroutine(CrossFadeCoroutine(newSource, duration));
    }

    private IEnumerator FadeInCoroutine(float duration, float targetVolume)
    {
        if (targetAudioSource == null) yield break;

        float startVolume = targetAudioSource.volume;
        float timer = 0f;

        if (!targetAudioSource.isPlaying)
        {
            targetAudioSource.volume = 0f;
            targetAudioSource.Play();
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            targetAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        targetAudioSource.volume = targetVolume;
    }

    private IEnumerator FadeOutCoroutine(float duration, float targetVolume)
    {
        if (targetAudioSource == null) yield break;

        float startVolume = targetAudioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            targetAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        targetAudioSource.volume = targetVolume;

        if (targetVolume <= 0f)
        {
            targetAudioSource.Stop();
        }
    }

    private IEnumerator CrossFadeCoroutine(AudioSource newSource, float duration)
    {
        if (targetAudioSource == null || newSource == null) yield break;

        float halfDuration = duration / 2f;

        // 淡出旧音频
        float oldStartVolume = targetAudioSource.volume;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            targetAudioSource.volume = Mathf.Lerp(oldStartVolume, 0f, timer / halfDuration);
            yield return null;
        }

        targetAudioSource.Stop();
        targetAudioSource.volume = oldStartVolume;

        // 淡入新音频
        newSource.volume = 0f;
        newSource.Play();

        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            newSource.volume = Mathf.Lerp(0f, fadeInTargetVolume, timer / halfDuration);
            yield return null;
        }

        newSource.volume = fadeInTargetVolume;
        targetAudioSource = newSource;
    }

    public void SetAudioSource(AudioSource source)
    {
        targetAudioSource = source;
    }
}
