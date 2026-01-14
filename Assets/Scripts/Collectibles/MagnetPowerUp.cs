using UnityEngine;

/// <summary>
/// 磁铁道具 - 自动吸引附近的金币
/// </summary>
public class MagnetPowerUp : PowerUp
{
    [Header("磁铁设置")]
    [Tooltip("磁铁吸引范围")]
    [SerializeField] private float magnetRange = 15f;

    [Tooltip("磁铁吸引速度")]
    [SerializeField] private float magnetSpeed = 20f;

    // 单例实例（当前活动的磁铁效果）
    private static MagnetPowerUp activeInstance;

    protected override void Initialize()
    {
        base.Initialize();
        collectibleType = CollectibleType.Magnet;

        // 设置颜色（磁铁是红色）
        if (collectibleRenderer != null)
        {
            collectibleRenderer.material.color = Color.red;
        }
    }

    protected override void ActivatePowerUp(PlayerController player)
    {
        // 如果已有活动的磁铁且不可堆叠，延长持续时间
        if (activeInstance != null && activeInstance != this)
        {
            if (!stackable)
            {
                activeInstance.remainingTime += duration;
                Destroy(gameObject);
                return;
            }
        }

        isActive = true;
        remainingTime = duration;
        activeInstance = this;

        // 创建视觉特效
        CreateActiveEffect(player.transform);

        // 增强所有收集品的磁铁效果
        EnhanceCollectiblesMagnet();

        Debug.Log("[MagnetPowerUp] Magnet activated!");
    }

    protected override void DeactivatePowerUp(PlayerController player)
    {
        isActive = false;

        if (activeInstance == this)
        {
            activeInstance = null;
        }

        // 移除视觉特效
        RemoveActiveEffect();

        // 恢复收集品磁铁效果
        RestoreCollectiblesMagnet();

        Debug.Log("[MagnetPowerUp] Magnet deactivated");
    }

    /// <summary>
    /// 增强收集品的磁铁效果
    /// </summary>
    private void EnhanceCollectiblesMagnet()
    {
        // 查找所有收集品并增强磁铁范围
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (var collectible in collectibles)
        {
            if (collectible != null && !collectible.IsCollected)
            {
                // 临时增大磁铁范围
                collectible.SetMagnetized(true);
            }
        }
    }

    /// <summary>
    /// 恢复收集品磁铁效果
    /// </summary>
    private void RestoreCollectiblesMagnet()
    {
        // 这会在道具过期时自动恢复
        // 因为收集品的磁铁状态会在Update中重新检查
    }

    /// <summary>
    /// 获取磁铁范围
    /// </summary>
    public float GetMagnetRange()
    {
        return magnetRange;
    }

    /// <summary>
    /// 获取磁铁速度
    /// </summary>
    public float GetMagnetSpeed()
    {
        return magnetSpeed;
    }

    private void Update()
    {
        base.Update();

        if (isActive)
        {
            // 持续吸引附近的收集品
            AttractNearbyCollectibles();
        }
    }

    /// <summary>
    /// 吸引附近的收集品
    /// </summary>
    private void AttractNearbyCollectibles()
    {
        if (playerTransform == null) return;

        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (var collectible in collectibles)
        {
            if (collectible == null || collectible.IsCollected) continue;

            float distance = Vector3.Distance(playerTransform.position, collectible.transform.position);

            if (distance <= magnetRange)
            {
                // 计算吸引方向
                Vector3 direction = (playerTransform.position - collectible.transform.position).normalized;
                float speed = magnetSpeed * (1f - distance / magnetRange);

                // 移动收集品（通过设置位置）
                collectible.transform.position += direction * speed * Time.deltaTime;
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制磁铁范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (!isActive) return;

        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, magnetRange);
        }
    }
#endif
}
