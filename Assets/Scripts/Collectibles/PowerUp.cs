using UnityEngine;

/// <summary>
/// 能量道具基类 - 提供临时能力提升
/// </summary>
public abstract class PowerUp : Collectible
{
    [Header("能量道具设置")]
    [SerializeField] protected float duration = 10f;

    [SerializeField] protected bool stackable = false;

    [SerializeField] protected AudioClip activateSound;

    [SerializeField] protected GameObject powerUpEffect;

    // 状态
    protected float remainingTime;
    protected bool isActive = false;
    protected GameObject activeEffect;

    // 组件
    protected AudioSource audioSource;

    protected override void Initialize()
    {
        base.Initialize();

        // 添加AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    protected override void GrantReward(PlayerController player)
    {
        if (player == null) return;

        // 激活道具效果
        ActivatePowerUp(player);

        // 播放音效
        if (activateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activateSound);
        }

        Debug.Log($"[PowerUp] {collectibleType} activated for {duration} seconds!");
    }

    /// <summary>
    /// 激活道具效果（子类实现）
    /// </summary>
    protected abstract void ActivatePowerUp(PlayerController player);

    /// <summary>
    /// 停用道具效果（子类实现）
    /// </summary>
    protected abstract void DeactivatePowerUp(PlayerController player);

    /// <summary>
    /// 更新道具效果
    /// </summary>
    protected virtual void UpdatePowerUp(PlayerController player)
    {
        if (!isActive) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            DeactivatePowerUp(player);
        }
    }

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    public float GetRemainingTime()
    {
        return remainingTime;
    }

    /// <summary>
    /// 获取激活进度（0-1）
    /// </summary>
    public float GetActivationProgress()
    {
        if (!isActive) return 0f;
        return remainingTime / duration;
    }

    /// <summary>
    /// 是否可堆叠
    /// </summary>
    public bool IsStackable()
    {
        return stackable;
    }

    /// <summary>
    /// 创建激活特效
    /// </summary>
    protected void CreateActiveEffect(Transform parent)
    {
        if (powerUpEffect != null)
        {
            activeEffect = Instantiate(powerUpEffect, parent);
            // TODO: Phase 9 实现完整特效系统
        }
    }

    /// <summary>
    /// 移除激活特效
    /// </summary>
    protected void RemoveActiveEffect()
    {
        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }
    }
}
