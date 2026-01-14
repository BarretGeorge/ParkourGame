using UnityEngine;

/// <summary>
/// 障碍物基类 - 所有障碍物的父类
/// </summary>
public abstract class Obstacle : MonoBehaviour
{
    [Header("障碍物设置")]
    [SerializeField] protected ObstacleType obstacleType = ObstacleType.Static;

    [SerializeField] protected int damage = 1;

    [SerializeField] protected bool canBeDestroyed = false;

    [SerializeField] protected int health = 1;

    [Header("碰撞设置")]
    [SerializeField] protected bool useTrigger = false;

    [SerializeField] protected float collisionCooldown = 0.5f;

    [SerializeField] protected LayerMask playerLayer = 1;

    [Header("视觉效果")]
    [SerializeField] protected bool enableHighlight = true;

    [SerializeField] protected Color highlightColor = Color.red;

    [SerializeField] protected float flashDuration = 0.2f;

    // 状态
    protected bool isDestroyed = false;
    protected float lastCollisionTime = -999f;
    protected Renderer obstacleRenderer;
    protected Color originalColor;

    // 事件
    public System.Action<Obstacle> OnObstacleHit;
    public System.Action<Obstacle> OnObstacleDestroyed;

    // 属性
    public ObstacleType Type => obstacleType;
    public bool IsDestroyed => isDestroyed;
    public int Damage => damage;
    public bool CanBeDestroyed => canBeDestroyed;

    protected virtual void Awake()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        // 获取Renderer
        obstacleRenderer = GetComponent<Renderer>();
        if (obstacleRenderer != null)
        {
            originalColor = obstacleRenderer.material.color;
        }

        // 如果没有Collider，添加一个
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = useTrigger;
        }

        // 设置标签
        gameObject.tag = "Obstacle";
    }

    protected virtual void Update()
    {
        // 每个障碍物可以有自己的更新逻辑
        OnUpdate();
    }

    /// <summary>
    /// 子类重写的Update方法
    /// </summary>
    protected virtual void OnUpdate()
    {
        // 移动障碍物等逻辑在这里实现
    }

    /// <summary>
    /// 处理玩家碰撞
    /// </summary>
    public virtual void HandlePlayerCollision(PlayerController player)
    {
        // 检查冷却时间
        if (Time.time - lastCollisionTime < collisionCooldown)
        {
            return;
        }

        lastCollisionTime = Time.time;

        // 触发碰撞事件
        OnObstacleHit?.Invoke(this);

        // 处理碰撞
        if (canBeDestroyed)
        {
            TakeDamage(1);
        }
        else
        {
            // 不能破坏的障碍物直接杀死玩家
            player?.Die();
        }

        // 视觉反馈
        Flash();
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(int damageAmount)
    {
        if (!canBeDestroyed || isDestroyed) return;

        health -= damageAmount;

        if (health <= 0)
        {
            DestroyObstacle();
        }
    }

    /// <summary>
    /// 销毁障碍物
    /// </summary>
    public virtual void DestroyObstacle()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        // 触发事件
        OnObstacleDestroyed?.Invoke(this);

        // 播放销毁效果（Phase 9实现粒子特效）
        PlayDestroyEffect();

        // 销毁对象
        Destroy(gameObject);
    }

    /// <summary>
    /// 闪烁效果
    /// </summary>
    protected virtual void Flash()
    {
        if (!enableHighlight || obstacleRenderer == null) return;

        StartCoroutine(FlashCoroutine());
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        if (obstacleRenderer != null)
        {
            obstacleRenderer.material.color = highlightColor;
            yield return new WaitForSeconds(flashDuration);
            obstacleRenderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// 播放销毁特效
    /// </summary>
    protected virtual void PlayDestroyEffect()
    {
        // TODO: Phase 9 实现粒子特效
        Debug.Log($"[Obstacle] {obstacleType} destroyed!");
    }

    /// <summary>
    /// 重置障碍物状态（用于对象池）
    /// </summary>
    public virtual void ResetObstacle()
    {
        isDestroyed = false;
        health = 1;
        lastCollisionTime = -999f;

        if (obstacleRenderer != null)
        {
            obstacleRenderer.material.color = originalColor;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (IsPlayerInLayer(other))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            HandlePlayerCollision(player);
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (IsPlayerInLayer(collision.collider))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            HandlePlayerCollision(player);
        }
    }

    /// <summary>
    /// 检查是否是玩家
    /// </summary>
    protected bool IsPlayerInLayer(Collider collider)
    {
        return ((1 << collider.gameObject.layer) & playerLayer) != 0;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在Scene视图中可视化障碍物
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(collider.center, collider.size);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
#endif
}

/// <summary>
/// 障碍物类型
/// </summary>
public enum ObstacleType
{
    Static,       // 静态障碍物
    Moving,       // 移动障碍物
    Rotating,     // 旋转障碍物
    Falling,      // 掉落障碍物
    Breakable,    // 可破坏障碍物
    Low,          // 低矮障碍物（需滑铲）
    High,         // 高处障碍物（需下蹲）
    Wide,         // 宽障碍物（占据多车道）
    Narrow,       // 窄障碍物
    Special       // 特殊障碍物
}
