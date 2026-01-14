using UnityEngine;

/// <summary>
/// 金币收集品
/// </summary>
public class Coin : Collectible
{
    [Header("金币设置")]
    [Tooltip("金币类型")]
    [SerializeField] private CoinType coinType = CoinType.Normal;

    [Tooltip("金币颜色")]
    [SerializeField] private Color coinColor = Color.yellow;

    [Header("音效")]
    [SerializeField] private AudioClip collectSound;

    // 状态
    private AudioSource audioSource;

    protected override void Initialize()
    {
        base.Initialize();

        collectibleType = CollectibleType.Coin;

        // 设置颜色
        if (collectibleRenderer != null)
        {
            collectibleRenderer.material.color = coinColor;
        }

        // 设置发光颜色
        if (glowLight != null)
        {
            glowLight.color = coinColor;
        }

        // 添加AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D音效

        // 根据类型设置值
        SetCoinValue();
    }

    protected override void GrantReward(PlayerController player)
    {
        if (player == null) return;

        // 添加金币
        player.AddCoin(value);

        Debug.Log($"[Coin] Collected {coinType} coin! Value: {value}");
    }

    protected override void PlayCollectEffect()
    {
        base.PlayCollectEffect();

        // 播放音效
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // TODO: Phase 9 添加粒子特效
        // 播放金币收集特效（闪光、粒子等）
    }

    /// <summary>
    /// 根据类型设置值
    /// </summary>
    private void SetCoinValue()
    {
        switch (coinType)
        {
            case CoinType.Normal:
                value = 1;
                coinColor = Color.yellow;
                break;

            case CoinType.Silver:
                value = 5;
                coinColor = new Color(0.75f, 0.75f, 0.75f); // 银色
                break;

            case CoinType.Gold:
                value = 10;
                coinColor = Color.yellow;
                break;

            case CoinType.Diamond:
                value = 50;
                coinColor = new Color(0.5f, 0.8f, 1f); // 钻石蓝
                break;

            case CoinType.Rainbow:
                value = 100;
                coinColor = Color.white;
                break;
        }

        // 更新材质颜色
        if (collectibleRenderer != null)
        {
            collectibleRenderer.material.color = coinColor;
        }
    }

    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();

        // 彩虹金币特殊效果
        if (coinType == CoinType.Rainbow && collectibleRenderer != null)
        {
            float hue = (Time.time * 0.5f) % 1f;
            collectibleRenderer.material.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }
}

/// <summary>
/// 金币类型
/// </summary>
public enum CoinType
{
    Normal,  // 普通金币（1分）
    Silver,  // 银币（5分）
    Gold,    // 金币（10分）
    Diamond, // 钻石（50分）
    Rainbow  // 彩金币（100分）
}
