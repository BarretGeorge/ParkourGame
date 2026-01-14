using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 碰撞管理器 - 统一管理游戏中的所有碰撞检测
/// </summary>
public class CollisionManager : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerController playerController;

    [SerializeField] private AdvancedCollisionDetector collisionDetector;

    [Header("碰撞设置")]
    [Tooltip("启用碰撞反馈")]
    [SerializeField] private bool enableCollisionFeedback = true;

    [Tooltip("碰撞震动强度")]
    [SerializeField] private float collisionShakeIntensity = 0.5f;

    [Tooltip("碰撞震动持续时间")]
    [SerializeField] private float collisionShakeDuration = 0.3f;

    [Header("甘地判定配置")]
    [Tooltip("全局甘地判定开关")]
    [SerializeField] private bool globalGracePeriodEnabled = true;

    [Tooltip("甘地判定次数限制（0=无限）")]
    [SerializeField] private int maxGracePeriodsPerRun = 3;

    // 统计
    public int TotalCollisions { get; private set; }
    public int GracePeriodsUsed { get; private set; }
    public int ObstaclesHit { get; private set; }
    public int CollectiblesCollected { get; private set; }

    // 状态
    private bool hasShield = false;
    private List<GameObject> recentlyCollidedObjects = new List<GameObject>();

    // 事件
    public System.Action<CollisionData> OnCollision;
    public System.Action<CollisionData> OnGracePeriodUsed;
    public System.Action<PlayerController> OnPlayerDeath;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 自动获取组件
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (collisionDetector == null)
        {
            collisionDetector = GetComponent<AdvancedCollisionDetector>();
            if (collisionDetector == null)
            {
                collisionDetector = gameObject.AddComponent<AdvancedCollisionDetector>();
            }
        }

        // 订阅碰撞检测器事件
        collisionDetector.OnCollisionDetected += HandleCollisionDetected;
        collisionDetector.OnGracePeriodUsed += HandleGracePeriodUsed;
        collisionDetector.OnPlayerDeath += HandlePlayerDeath;

        // 查找CollectibleManager
        CollectibleManager collectibleManager = FindObjectOfType<CollectibleManager>();
        if (collectibleManager != null)
        {
            collectibleManager.OnPowerUpActivated += HandlePowerUpActivated;
            collectibleManager.OnPowerUpDeactivated += HandlePowerUpDeactivated;
        }
    }

    /// <summary>
    /// 处理碰撞检测事件
    /// </summary>
    private void HandleCollisionDetected(Collider collider)
    {
        // 检查是否最近已碰撞
        if (recentlyCollidedObjects.Contains(collider.gameObject))
        {
            return;
        }

        // 创建碰撞数据
        CollisionData collisionData = new CollisionData
        {
            collider = collider,
            collisionPoint = collider.ClosestPointOnBounds(transform.position),
            collisionTime = Time.time,
            collisionType = DetermineCollisionType(collider)
        };

        // 触发事件
        OnCollision?.Invoke(collisionData);

        // 根据类型处理
        switch (collisionData.collisionType)
        {
            case CollisionType.Obstacle:
                HandleObstacleCollision(collisionData);
                break;

            case CollisionType.Collectible:
                HandleCollectibleCollision(collisionData);
                break;

            case CollisionType.PowerUp:
                HandlePowerUpCollision(collisionData);
                break;
        }

        // 记录碰撞对象
        recentlyCollidedObjects.Add(collider.gameObject);
        StartCoroutine(RemoveFromRecentCollisions(collider.gameObject, 0.5f));

        // 统计
        TotalCollisions++;
        if (collisionData.collisionType == CollisionType.Obstacle)
        {
            ObstaclesHit++;
        }
    }

    /// <summary>
    /// 处理甘地判定事件
    /// </summary>
    private void HandleGracePeriodUsed(Collider collider)
    {
        if (!globalGracePeriodEnabled) return;

        // 检查次数限制
        if (maxGracePeriodsPerRun > 0 && GracePeriodsUsed >= maxGracePeriodsPerRun)
        {
            return;
        }

        CollisionData collisionData = new CollisionData
        {
            collider = collider,
            collisionPoint = collider.ClosestPointOnBounds(transform.position),
            collisionTime = Time.time,
            collisionType = CollisionType.GracePeriod
        };

        OnGracePeriodUsed?.Invoke(collisionData);
        GracePeriodsUsed++;

        // 视觉反馈
        ShowGracePeriodFeedback();

        Debug.Log($"[CollisionManager] Grace period used! ({GracePeriodsUsed}/{maxGracePeriodsPerRun})");
    }

    /// <summary>
    /// 处理玩家死亡
    /// </summary>
    private void HandlePlayerDeath(PlayerController player)
    {
        OnPlayerDeath?.Invoke(player);

        // 碰撞反馈
        if (enableCollisionFeedback)
        {
            Camera.main?.GetComponent<CameraController>()?.ShakeCamera(
                collisionShakeIntensity,
                collisionShakeDuration
            );
        }

        Debug.Log("[CollisionManager] Player died!");
    }

    /// <summary>
    /// 确定碰撞类型
    /// </summary>
    private CollisionType DetermineCollisionType(Collider collider)
    {
        if (collider.CompareTag("Obstacle"))
        {
            return CollisionType.Obstacle;
        }
        else if (collider.CompareTag("Collectible"))
        {
            Collectible collectible = collider.GetComponent<Collectible>();
            if (collectible != null)
            {
                switch (collectible.Type)
                {
                    case CollectibleType.Magnet:
                    case CollectibleType.Shield:
                    case CollectibleType.SpeedBoost:
                        return CollisionType.PowerUp;
                    default:
                        return CollisionType.Collectible;
                }
            }
        }

        return CollisionType.Unknown;
    }

    /// <summary>
    /// 处理障碍物碰撞
    /// </summary>
    private void HandleObstacleCollision(CollisionData collisionData)
    {
        // 玩家死亡已在AdvancedCollisionDetector中处理
        Debug.Log($"[CollisionManager] Obstacle collision: {collisionData.collider.name}");
    }

    /// <summary>
    /// 处理收集品碰撞
    /// </summary>
    private void HandleCollectibleCollision(CollisionData collisionData)
    {
        CollectiblesCollected++;
        Debug.Log($"[CollisionManager] Collectible collected: {collisionData.collider.name}");
    }

    /// <summary>
    /// 处理能量道具碰撞
    /// </summary>
    private void HandlePowerUpCollision(CollisionData collisionData)
    {
        Debug.Log($"[CollisionManager] Power-up collected: {collisionData.collider.name}");
    }

    /// <summary>
    /// 处理能量道具激活
    /// </summary>
    private void HandlePowerUpActivated(PowerUp powerUp)
    {
        if (powerUp is ShieldPowerUp)
        {
            hasShield = true;
            Debug.Log("[CollisionManager] Shield activated!");
        }
    }

    /// <summary>
    /// 处理能量道具停用
    /// </summary>
    private void HandlePowerUpDeactivated(PowerUp powerUp)
    {
        if (powerUp is ShieldPowerUp)
        {
            hasShield = false;
            Debug.Log("[CollisionManager] Shield deactivated!");
        }
    }

    /// <summary>
    /// 显示甘地判定反馈
    /// </summary>
    private void ShowGracePeriodFeedback()
    {
        // TODO: Phase 9 实现粒子特效
        // 这里可以添加闪烁特效、提示文字等
        Debug.Log("[CollisionManager] Grace period visual feedback!");
    }

    /// <summary>
    /// 从最近碰撞列表中移除
    /// </summary>
    private System.Collections.IEnumerator RemoveFromRecentCollisions(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        recentlyCollidedObjects.Remove(obj);
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(PlayerController player)
    {
        playerController = player;
        if (collisionDetector != null)
        {
            // 碰撞检测器需要更新引用
        }
    }

    /// <summary>
    /// 重置碰撞管理器
    /// </summary>
    public void ResetCollisionManager()
    {
        TotalCollisions = 0;
        GracePeriodsUsed = 0;
        ObstaclesHit = 0;
        CollectiblesCollected = 0;
        hasShield = false;
        recentlyCollidedObjects.Clear();

        if (collisionDetector != null)
        {
            collisionDetector.ResetCollisionDetector();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 调试信息
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 1500, 300, 250));
        GUILayout.Box("Collision Manager Debug");

        GUILayout.Label($"Total Collisions: {TotalCollisions}");
        GUILayout.Label($"Obstacles Hit: {ObstaclesHit}");
        GUILayout.Label($"Collectibles: {CollectiblesCollected}");
        GUILayout.Label($"Grace Periods Used: {GracePeriodsUsed}/{maxGracePeriodsPerRun}");
        GUILayout.Label($"Has Shield: {hasShield}");
        GUILayout.Label($"Recently Collided: {recentlyCollidedObjects.Count}");

        GUILayout.EndArea();
    }
#endif
}

/// <summary>
/// 碰撞数据
/// </summary>
public class CollisionData
{
    public Collider collider;
    public Vector3 collisionPoint;
    public float collisionTime;
    public CollisionType collisionType;
}

/// <summary>
/// 碰撞类型
/// </summary>
public enum CollisionType
{
    Unknown,
    Obstacle,
    Collectible,
    PowerUp,
    GracePeriod
}
