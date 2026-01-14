using UnityEngine;

/// <summary>
/// 高级碰撞检测器 - 提供精准的碰撞判定和甘地判定
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class AdvancedCollisionDetector : MonoBehaviour
{
    [Header("玩家引用")]
    [SerializeField] private PlayerController playerController;

    [Header("碰撞检测设置")]
    [Tooltip("启用多点碰撞检测")]
    [SerializeField] private bool enableMultiPointDetection = true;

    [Tooltip("检测点数量")]
    [Range(3, 12)]
    [SerializeField] private int detectionPoints = 6;

    [Tooltip("检测半径")]
    [SerializeField] private float detectionRadius = 0.3f;

    [Header("甘地判定（容错）")]
    [Tooltip("启用甘地判定")]
    [SerializeField] private bool enableGracePeriod = true;

    [Tooltip("水平容错距离")]
    [SerializeField] private float horizontalGraceDistance = 0.2f;

    [Tooltip("垂直容错时间（秒）")]
    [SerializeField] private float verticalGraceTime = 0.1f;

    [Tooltip("容错冷却时间")]
    [SerializeField] private float graceCooldown = 0.5f;

    [Header("碰撞响应")]
    [Tooltip("碰撞层")]
    [SerializeField] private LayerMask collisionLayers = 1;

    [Tooltip("忽略触发器")]
    [SerializeField] private bool ignoreTriggers = true;

    // 组件
    private CharacterController characterController;

    // 检测点
    private Vector3[] detectionPointPositions;

    // 甘地判定状态
    private float lastCollisionTime = -999f;
    private float graceTimer = 0f;
    private bool isInGracePeriod = false;

    // 上次碰撞的对象
    private GameObject lastCollidedObject;

    // 事件
    public System.Action<Collider> OnCollisionDetected;
    public System.Action<Collider> OnGracePeriodUsed;
    public System.Action<PlayerController> OnPlayerDeath;

    // 属性
    public bool IsInGracePeriod => isInGracePeriod;
    public float GraceTimer => graceTimer;
    public bool HasGraceCooldown => Time.time - lastCollisionTime < graceCooldown;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 获取组件
        characterController = GetComponent<CharacterController>();

        // 自动获取PlayerController
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        // 计算检测点位置
        CalculateDetectionPoints();
    }

    /// <summary>
    /// 计算检测点位置（圆形排列）
    /// </summary>
    private void CalculateDetectionPoints()
    {
        detectionPointPositions = new Vector3[detectionPoints];

        for (int i = 0; i < detectionPoints; i++)
        {
            float angle = (i / (float)detectionPoints) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * detectionRadius;
            float z = Mathf.Sin(angle) * detectionRadius;
            detectionPointPositions[i] = new Vector3(x, 0, z);
        }
    }

    private void Update()
    {
        // 更新甘地判定
        UpdateGracePeriod();

        // 执行碰撞检测
        if (enableMultiPointDetection)
        {
            MultiPointCollisionDetection();
        }
        else
        {
            SimpleCollisionDetection();
        }
    }

    /// <summary>
    /// 多点碰撞检测（更精准）
    /// </summary>
    private void MultiPointCollisionDetection()
    {
        Vector3 playerPos = transform.position;

        for (int i = 0; i < detectionPoints; i++)
        {
            // 计算检测点世界位置
            Vector3 detectionPoint = playerPos + detectionPointPositions[i];

            // 执行检测
            Collider[] hits = Physics.OverlapSphere(
                detectionPoint,
                detectionRadius * 0.5f,
                collisionLayers,
                QueryTriggerInteraction.Ignore
            );

            foreach (var hit in hits)
            {
                if (ShouldProcessCollision(hit))
                {
                    ProcessCollision(hit);
                }
            }
        }
    }

    /// <summary>
    /// 简单碰撞检测（单一检测点）
    /// </summary>
    private void SimpleCollisionDetection()
    {
        Vector3 center = transform.position + characterController.center;

        Collider[] hits = Physics.OverlapSphere(
            center,
            characterController.radius * 0.8f,
            collisionLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            if (ShouldProcessCollision(hit))
            {
                ProcessCollision(hit);
            }
        }
    }

    /// <summary>
    /// 检查是否应该处理碰撞
    /// </summary>
    private bool ShouldProcessCollision(Collider collider)
    {
        // 忽略触发器
        if (ignoreTriggers && collider.isTrigger) return false;

        // 忽略自己
        if (collider.transform == transform) return false;

        // 忽略最近碰撞的对象（防止连续判定）
        if (collider.gameObject == lastCollidedObject &&
            Time.time - lastCollisionTime < 0.1f)
        {
            return false;
        }

        // 检查是否在碰撞层中
        if (((1 << collider.gameObject.layer) & collisionLayers) == 0) return false;

        return true;
    }

    /// <summary>
    /// 处理碰撞
    /// </summary>
    private void ProcessCollision(Collider collider)
    {
        // 检查是否是障碍物
        if (collider.CompareTag("Obstacle"))
        {
            HandleObstacleCollision(collider);
        }
        // 检查是否是收集品
        else if (collider.CompareTag("Collectible"))
        {
            HandleCollectibleCollision(collider);
        }
    }

    /// <summary>
    /// 处理障碍物碰撞
    /// </summary>
    private void HandleObstacleCollision(Collider obstacleCollider)
    {
        // 检查是否有护盾
        CollectibleManager collectibleManager = FindObjectOfType<CollectibleManager>();
        if (collectibleManager != null && collectibleManager.HasShield)
        {
            ShieldPowerUp shield = FindObjectOfType<ShieldPowerUp>();
            if (shield != null && shield.TryAbsorbDamage())
            {
                // 护盾抵挡了伤害
                Debug.Log("[AdvancedCollisionDetector] Shield absorbed damage!");
                lastCollidedObject = obstacleCollider.gameObject;
                lastCollisionTime = Time.time;
                return;
            }
        }

        // 检查是否是可破坏障碍物
        BreakableObstacle breakable = obstacleCollider.GetComponent<BreakableObstacle>();
        if (breakable != null)
        {
            // 尝试用特殊动作破坏
            breakable.HandleSpecialBreak(playerController);
            lastCollidedObject = obstacleCollider.gameObject;
            lastCollisionTime = Time.time;
            return;
        }

        // 检查甘地判定
        if (enableGracePeriod && CanUseGracePeriod())
        {
            // 使用甘地判定，避免碰撞
            TriggerGracePeriod(obstacleCollider);
            Debug.Log("[AdvancedCollisionDetector] Grace period used!");
            return;
        }

        // 正常碰撞，玩家死亡
        TriggerCollisionDetected(obstacleCollider);

        // 玩家死亡
        if (playerController != null)
        {
            playerController.Die();
            OnPlayerDeath?.Invoke(playerController);
        }

        lastCollidedObject = obstacleCollider.gameObject;
        lastCollisionTime = Time.time;
    }

    /// <summary>
    /// 处理收集品碰撞
    /// </summary>
    private void HandleCollectibleCollision(Collider collectibleCollider)
    {
        Collectible collectible = collectibleCollider.GetComponent<Collectible>();
        if (collectible != null && !collectible.IsCollected)
        {
            collectible.Collect(playerController);
        }

        lastCollidedObject = collectibleCollider.gameObject;
        lastCollisionTime = Time.time;
    }

    /// <summary>
    /// 检查是否可以使用甘地判定
    /// </summary>
    private bool CanUseGracePeriod()
    {
        // 检查冷却
        if (HasGraceCooldown) return false;

        // 检查是否已经在甘地判定中
        if (isInGracePeriod) return false;

        // 检查玩家动作
        bool isDodging = playerController != null && (
            playerController.IsSliding ||
            playerController.IsCrouching ||
            playerController.IsJumping
        );

        // 只有在执行动作时才能使用甘地判定
        return isDodging;
    }

    /// <summary>
    /// 触发甘地判定
    /// </summary>
    private void TriggerGracePeriod(Collider obstacle)
    {
        isInGracePeriod = true;
        graceTimer = verticalGraceTime;

        OnGracePeriodUsed?.Invoke(obstacle);

        // 触发碰撞事件但不杀死玩家
        OnCollisionDetected?.Invoke(obstacle);
    }

    /// <summary>
    /// 更新甘地判定
    /// </summary>
    private void UpdateGracePeriod()
    {
        if (!isInGracePeriod) return;

        graceTimer -= Time.deltaTime;

        if (graceTimer <= 0f)
        {
            isInGracePeriod = false;
            graceTimer = 0f;
        }
    }

    /// <summary>
    /// 触发碰撞检测事件
    /// </summary>
    private void TriggerCollisionDetected(Collider collider)
    {
        OnCollisionDetected?.Invoke(collider);
    }

    /// <summary>
    /// 执行精确的射线检测
    /// </summary>
    public RaycastHit[] CastDetectionRays(Vector3 direction, float maxDistance)
    {
        Vector3 center = transform.position + characterController.center;

        // 从多个点发射射线
        RaycastHit[] allHits = new RaycastHit[detectionPoints];

        for (int i = 0; i < detectionPoints; i++)
        {
            Vector3 origin = center + detectionPointPositions[i];

            Physics.Raycast(
                origin,
                direction,
                out allHits[i],
                maxDistance,
                collisionLayers
            );
        }

        return allHits;
    }

    /// <summary>
    /// 检查前方是否有障碍物（用于攀爬等）
    /// </summary>
    public bool CheckForObstacleAhead(float distance, out RaycastHit hitInfo)
    {
        Vector3 center = transform.position + characterController.center;
        Vector3 direction = transform.forward;

        bool hit = Physics.SphereCast(
            center,
            characterController.radius * 0.5f,
            direction,
            out hitInfo,
            distance,
            collisionLayers
        );

        return hit;
    }

    /// <summary>
    /// 检查上方是否有障碍物（用于下蹲判断）
    /// </summary>
    public bool CheckForObstacleAbove(float height)
    {
        Vector3 center = transform.position + characterController.center;
        Vector3 origin = center + Vector3.up * (characterController.height * 0.5f);

        return Physics.CheckSphere(
            origin,
            characterController.radius * 0.5f,
            collisionLayers
        );
    }

    /// <summary>
    /// 检查下方是否有地面
    /// </summary>
    public bool CheckForGroundBelow(float distance, out RaycastHit hitInfo)
    {
        Vector3 center = transform.position + characterController.center;
        Vector3 origin = center + Vector3.down * (characterController.height * 0.5f);

        bool hit = Physics.SphereCast(
            origin,
            characterController.radius * 0.5f,
            Vector3.down,
            out hitInfo,
            distance,
            playerController?.PlayerData.groundLayer ?? 1
        );

        return hit;
    }

    /// <summary>
    /// 重置碰撞检测器
    /// </summary>
    public void ResetCollisionDetector()
    {
        lastCollisionTime = -999f;
        isInGracePeriod = false;
        graceTimer = 0f;
        lastCollidedObject = null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制检测点
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 playerPos = transform.position;

        // 绘制检测点
        Gizmos.color = isInGracePeriod ? Color.green : Color.cyan;

        for (int i = 0; i < detectionPoints; i++)
        {
            if (detectionPointPositions != null && i < detectionPointPositions.Length)
            {
                Vector3 point = playerPos + detectionPointPositions[i];
                Gizmos.DrawWireSphere(point, detectionRadius * 0.5f);
            }
        }

        // 绘制碰撞范围
        if (characterController != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = playerPos + characterController.center;
            Gizmos.DrawWireSphere(center, characterController.radius * 0.8f);
        }
    }

    /// <summary>
    /// 调试信息
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 1350, 300, 150));
        GUILayout.Box("Collision Detector Debug");

        GUILayout.Label($"Multi-Point Detection: {enableMultiPointDetection}");
        GUILayout.Label($"Detection Points: {detectionPoints}");
        GUILayout.Label($"Grace Period: {isInGracePeriod}");
        GUILayout.Label($"Grace Timer: {graceTimer:F2}s");
        GUILayout.Label($"Last Collision: {Time.time - lastCollisionTime:F2}s ago");
        GUILayout.Label($"Has Cooldown: {HasGraceCooldown}");

        GUILayout.EndArea();
    }
#endif
}
