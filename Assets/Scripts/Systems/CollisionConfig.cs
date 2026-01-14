using UnityEngine;

/// <summary>
/// 碰撞配置 - 定义所有碰撞检测相关参数
/// </summary>
[CreateAssetMenu(fileName = "CollisionConfig", menuName = "Parkour/Collision Config")]
public class CollisionConfig : ScriptableObject
{
    [Header("检测精度")]
    [Tooltip("多点检测开关")]
    public bool enableMultiPointDetection = true;

    [Tooltip("检测点数量")]
    [Range(3, 12)]
    public int detectionPoints = 6;

    [Tooltip("检测半径")]
    [Range(0.1f, 0.5f)]
    public float detectionRadius = 0.3f;

    [Header("甘地判定")]
    [Tooltip("启用甘地判定")]
    public bool enableGracePeriod = true;

    [Tooltip("水平容错距离")]
    [Range(0f, 0.5f)]
    public float horizontalGraceDistance = 0.2f;

    [Tooltip("垂直容错时间")]
    [Range(0.05f, 0.3f)]
    public float verticalGraceTime = 0.1f;

    [Tooltip("容错冷却时间")]
    [Range(0.1f, 1f)]
    public float graceCooldown = 0.5f;

    [Tooltip("甘地判定次数限制（0=无限）")]
    [Range(0, 10)]
    public int maxGracePeriodsPerRun = 3;

    [Header("碰撞层")]
    [Tooltip("障碍物层")]
    public LayerMask obstacleLayer = 1;

    [Tooltip("收集品层")]
    public LayerMask collectibleLayer = 1;

    [Tooltip("地面层")]
    public LayerMask groundLayer = 1;

    [Header("特殊碰撞")]
    [Tooltip("滑铲碰撞高度修正")]
    [Range(-1f, 1f)]
    public float slideHeightAdjustment = -0.5f;

    [Tooltip("下蹲碰撞高度修正")]
    [Range(-1f, 1f)]
    public float crouchHeightAdjustment = -0.3f;

    [Tooltip("攀爬碰撞范围")]
    [Range(0.5f, 3f)]
    public float climbCheckDistance = 2f;

    [Header("优化")]
    [Tooltip("启用空间划分")]
    public bool enableSpatialPartitioning = true;

    [Tooltip("空间网格大小")]
    [Range(5f, 20f)]
    public float gridSize = 10f;

    [Tooltip("启用碰撞剔除")]
    public bool enableCollisionCulling = true;

    [Tooltip("剔除距离")]
    [Range(20f, 100f)]
    public float cullingDistance = 50f;

    [Header("反馈")]
    [Tooltip("启用碰撞反馈")]
    public bool enableCollisionFeedback = true;

    [Tooltip("碰撞震动强度")]
    [Range(0.1f, 1f)]
    public float collisionShakeIntensity = 0.5f;

    [Tooltip("碰撞震动持续时间")]
    [Range(0.1f, 0.5f)]
    public float collisionShakeDuration = 0.3f;

    /// <summary>
    /// 验证配置
    /// </summary>
    public bool ValidateConfig()
    {
        bool isValid = true;

        if (detectionPoints < 3)
        {
            Debug.LogError("[CollisionConfig] Detection points must be at least 3!");
            isValid = false;
        }

        if (enableGracePeriod && verticalGraceTime <= 0f)
        {
            Debug.LogError("[CollisionConfig] Grace time must be positive!");
            isValid = false;
        }

        return isValid;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中预览配置
    /// </summary>
    public void PreviewConfig()
    {
        Debug.Log("=== Collision Config Preview ===");
        Debug.Log($"Multi-Point Detection: {enableMultiPointDetection}");
        Debug.Log($"Detection Points: {detectionPoints}");
        Debug.Log($"Detection Radius: {detectionRadius}m");
        Debug.Log($"Grace Period: {enableGracePeriod}");
        Debug.Log($"Horizontal Grace: {horizontalGraceDistance}m");
        Debug.Log($"Vertical Grace: {verticalGraceTime}s");
        Debug.Log($"Max Grace Periods: {maxGracePeriodsPerRun}");
        Debug.Log($"Spatial Partitioning: {enableSpatialPartitioning}");
        Debug.Log($"Grid Size: {gridSize}m");
        Debug.Log($"Collision Culling: {enableCollisionCulling}");
        Debug.Log($"Culling Distance: {cullingDistance}m");
    }
#endif
}
