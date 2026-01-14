using UnityEngine;

/// <summary>
/// 速度提升道具 - 临时提升移动速度
/// </summary>
public class SpeedBoostPowerUp : PowerUp
{
    [Header("速度提升设置")]
    [Tooltip("速度提升倍率")]
    [SerializeField] private float speedMultiplier = 1.5f;

    [Tooltip("是否影响重力")]
    [SerializeField] private bool reduceGravity = true;

    [Tooltip("重力倍率")]
    [SerializeField] private float gravityMultiplier = 0.7f;

    protected override void Initialize()
    {
        base.Initialize();
        collectibleType = CollectibleType.SpeedBoost;

        // 设置颜色（速度是绿色）
        if (collectibleRenderer != null)
        {
            collectibleRenderer.material.color = Color.green;
        }

        if (glowLight != null)
        {
            glowLight.color = Color.green;
        }
    }

    protected override void ActivatePowerUp(PlayerController player)
    {
        if (player == null) return;

        isActive = true;
        remainingTime = duration;

        // 提升速度
        float currentSpeed = player.CurrentSpeed;
        player.SetSpeed(currentSpeed * speedMultiplier);

        // TODO: 如果需要影响重力，修改PlayerJump的重力倍率
        // if (reduceGravity) { ... }

        // 创建视觉特效
        CreateActiveEffect(player.transform);

        Debug.Log($"[SpeedBoost] Speed boosted by {speedMultiplier}x for {duration}s");
    }

    protected override void DeactivatePowerUp(PlayerController player)
    {
        if (player == null) return;

        isActive = false;

        // 恢复正常速度
        PlayerData playerData = player.PlayerData;
        player.SetSpeed(playerData.baseSpeed);

        // 移除视觉特效
        RemoveActiveEffect();

        Debug.Log("[SpeedBoost] Speed restored to normal");
    }

    /// <summary>
    /// 获取速度倍率
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }

    private void Update()
    {
        base.Update();

        if (isActive)
        {
            // 更新速度提升效果
            UpdateSpeedBoostEffect();
        }
    }

    /// <summary>
    /// 更新速度提升特效
    /// </summary>
    private void UpdateSpeedBoostEffect()
    {
        // TODO: Phase 9 添加速度线特效
        // 这里可以添加尾迹、速度线等视觉反馈
    }
}
