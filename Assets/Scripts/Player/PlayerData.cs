using UnityEngine;

/// <summary>
/// 玩家配置数据 - 用于平衡性调整
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "Parkour/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("基础移动")]
    [Tooltip("基础移动速度")]
    [Range(5f, 20f)] public float baseSpeed = 10f;

    [Tooltip("最大移动速度")]
    [Range(15f, 50f)] public float maxSpeed = 30f;

    [Tooltip("速度递增率（每秒增加的速度）")]
    [Range(0.1f, 2f)] public float speedIncreaseRate = 0.5f;

    [Header("变道系统")]
    [Tooltip("变道数量（通常为3或5）")]
    [Range(1, 7)] public int laneCount = 3;

    [Tooltip("车道宽度")]
    [Range(1f, 5f)] public float laneWidth = 3f;

    [Tooltip("变道速度")]
    [Range(5f, 20f)] public float laneChangeSpeed = 10f;

    [Tooltip("变道平滑度")]
    [Range(0.1f, 1f)] public float laneChangeSmoothness = 0.5f;

    [Header("跳跃系统")]
    [Tooltip("跳跃高度")]
    [Range(3f, 10f)] public float jumpHeight = 6f;

    [Tooltip("二段跳高度")]
    [Range(2f, 8f)] public float doubleJumpHeight = 5f;

    [Tooltip("重力倍率")]
    [Range(1f, 5f)] public float gravityMultiplier = 2f;

    [Tooltip("落地检测距离")]
    [Range(0.1f, 1f)] public float groundCheckDistance = 0.3f;

    [Tooltip("地面检测层级")]
    public LayerMask groundLayer = 1;

    [Tooltip("是否启用二段跳")]
    public bool enableDoubleJump = true;

    [Header("甘地判定（容错）")]
    [Tooltip("左右容错距离")]
    [Range(0f, 0.5f)] public float horizontalGraceDistance = 0.2f;

    [Tooltip("上下容错时间（秒）")]
    [Range(0f, 0.5f)] public float verticalGraceTime = 0.1f;

    [Header("无敌时间")]
    [Tooltip("受伤后的无敌时间（秒）")]
    [Range(0.5f, 3f)] public float invincibilityTime = 1f;

    [Header("动画")]
    [Tooltip("动画混合参数平滑度")]
    [Range(1f, 20f)] public float animationBlendSpeed = 10f;

    [Tooltip("倾斜角度（变道时）")]
    [Range(10f, 45f)] public float tiltAngle = 20f;

    [Header("滑铲设置")]
    [Tooltip("滑铲时的角色高度")]
    [Range(0.5f, 1.5f)] public float slideHeight = 1f;

    [Tooltip("滑铲持续时间（秒）")]
    [Range(0.3f, 1.5f)] public float slideDuration = 0.8f;

    [Tooltip("滑铲速度倍率")]
    [Range(1.2f, 2f)] public float slideSpeedMultiplier = 1.5f;

    [Tooltip("滑铲冷却时间")]
    [Range(0.1f, 1f)] public float slideCooldown = 0.3f;

    [Header("下蹲设置")]
    [Tooltip("下蹲时的角色高度")]
    [Range(0.5f, 1.5f)] public float crouchHeight = 1.2f;

    [Tooltip("下蹲过渡速度")]
    [Range(5f, 20f)] public float crouchTransitionSpeed = 10f;

    [Tooltip("是否保持下蹲状态")]
    public bool holdToCrouch = true;

    [Header("蹬墙跑设置")]
    [Tooltip("蹬墙跑时的速度倍率")]
    [Range(1.1f, 1.5f)] public float wallRunSpeedMultiplier = 1.2f;

    [Tooltip("蹬墙跑最大持续时间（秒）")]
    [Range(1f, 5f)] public float maxWallRunDuration = 3f;

    [Tooltip("蹬墙跑后可跳跃的额外高度")]
    [Range(5f, 15f)] public float wallRunJumpHeight = 10f;

    [Tooltip("墙壁检测距离")]
    [Range(0.5f, 2f)] public float wallCheckDistance = 1f;

    [Tooltip("最大蹬墙跑角度（与墙壁法线的夹角）")]
    [Range(10f, 60f)] public float maxWallAngle = 45f;

    [Header("攀爬设置")]
    [Tooltip("攀爬速度")]
    [Range(3f, 10f)] public float climbSpeed = 5f;

    [Tooltip("最大攀爬高度")]
    [Range(2f, 6f)] public float maxClimbHeight = 4f;

    [Tooltip("攀爬检测距离")]
    [Range(1f, 3f)] public float climbCheckDistance = 2f;
}
