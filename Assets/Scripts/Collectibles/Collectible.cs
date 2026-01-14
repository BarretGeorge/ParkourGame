using UnityEngine;

/// <summary>
/// 收集品基类 - 所有可收集物品的父类
/// </summary>
public abstract class Collectible : MonoBehaviour
{
    [Header("收集品设置")]
    [SerializeField] protected CollectibleType collectibleType = CollectibleType.Coin;

    [SerializeField] protected int value = 1;

    [SerializeField] protected bool autoCollect = false;

    [Header("视觉效果")]
    [SerializeField] protected bool enableRotation = true;

    [SerializeField] protected float rotationSpeed = 90f;

    [SerializeField] protected bool enableBobbing = true;

    [SerializeField] protected float bobbingSpeed = 2f;

    [SerializeField] protected float bobbingAmount = 0.2f;

    [SerializeField] protected bool enableGlow = true;

    [SerializeField] protected Color glowColor = Color.yellow;

    [Header("磁铁设置")]
    [SerializeField] protected bool affectedByMagnet = true;

    [SerializeField] protected float magnetSpeed = 15f;

    [SerializeField] protected float magnetRange = 10f;

    // 状态
    protected bool isCollected = false;
    protected bool isBeingMagnetized = false;
    protected Vector3 basePosition;
    protected Quaternion baseRotation;

    // 组件
    protected Renderer collectibleRenderer;
    protected Light glowLight;

    // 引用
    protected Transform playerTransform;

    // 事件
    public System.Action<Collectible> OnCollected;
    public System.Action<Collectible> OnSpawned;

    // 属性
    public CollectibleType Type => collectibleType;
    public int Value => value;
    public bool IsCollected => isCollected;
    public bool IsBeingMagnetized => isBeingMagnetized;

    protected virtual void Awake()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        // 保存初始状态
        basePosition = transform.position;
        baseRotation = transform.rotation;

        // 获取Renderer
        collectibleRenderer = GetComponent<Renderer>();
        if (collectibleRenderer == null)
        {
            // 如果没有Renderer，添加一个
            GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visuals.transform.SetParent(transform);
            visuals.transform.localPosition = Vector3.zero;
            visuals.transform.localScale = Vector3.one * 0.5f;
            visuals.name = "Visuals";
            collectibleRenderer = visuals.GetComponent<Renderer>();

            // 移除Collider
            Destroy(visuals.GetComponent<Collider>());
        }

        // 添加发光效果
        if (enableGlow)
        {
            glowLight = gameObject.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = glowColor;
            glowLight.range = 3f;
            glowLight.intensity = 0.5f;
            glowLight.shadows = LightShadows.None;
        }

        // 添加Collider
        if (GetComponent<Collider>() == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }

        // 设置标签
        gameObject.tag = "Collectible";

        // 设置层
        gameObject.layer = LayerMask.NameToLayer("Collectible");
    }

    protected virtual void Start()
    {
        // 查找玩家
        FindPlayer();
    }

    protected virtual void Update()
    {
        if (isCollected) return;

        UpdateVisuals();
        UpdateMagnetEffect();
    }

    /// <summary>
    /// 更新视觉效果
    /// </summary>
    protected virtual void UpdateVisuals()
    {
        // 旋转
        if (enableRotation)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        // 上下浮动
        if (enableBobbing && !isBeingMagnetized)
        {
            float bobOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            transform.position = basePosition + Vector3.up * bobOffset;
        }
    }

    /// <summary>
    /// 更新磁铁效果
    /// </summary>
    protected virtual void UpdateMagnetEffect()
    {
        if (!affectedByMagnet || playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 检查是否在磁铁范围内
        if (distance <= magnetRange)
        {
            isBeingMagnetized = true;

            // 向玩家移动
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float speed = magnetSpeed * (1f - distance / magnetRange); // 越近越快
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            isBeingMagnetized = false;
        }
    }

    /// <summary>
    /// 收集物品
    /// </summary>
    public virtual void Collect(PlayerController player)
    {
        if (isCollected) return;

        isCollected = true;

        // 触发事件
        OnCollected?.Invoke(this);

        // 给予奖励
        GrantReward(player);

        // 播放收集特效
        PlayCollectEffect();

        // 销毁对象
        Destroy(gameObject);
    }

    /// <summary>
    /// 给予奖励（子类实现）
    /// </summary>
    protected abstract void GrantReward(PlayerController player);

    /// <summary>
    /// 播放收集特效
    /// </summary>
    protected virtual void PlayCollectEffect()
    {
        // TODO: Phase 9 实现粒子特效
        Debug.Log($"[Collectible] {collectibleType} collected!");
    }

    /// <summary>
    /// 查找玩家
    /// </summary>
    protected virtual void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

    /// <summary>
    /// 设置磁铁状态
    /// </summary>
    public void SetMagnetized(bool magnetized)
    {
        isBeingMagnetized = magnetized;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    public void SetValue(int newValue)
    {
        value = newValue;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            Collect(player);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制磁铁范围
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        if (!affectedByMagnet) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
#endif
}

/// <summary>
/// 收集品类型
/// </summary>
public enum CollectibleType
{
    Coin,              // 金币
    SpecialCoin,       // 特殊金币（高价值）
    Magnet,            // 磁铁
    Shield,            // 护盾
    SpeedBoost,        // 速度提升
    ScoreMultiplier,   // 分数倍率
    Invincibility,     // 无敌
    DoublePoints,      // 双倍分数
    Life,              // 生命
    Key,               // 钥匙
    Chest,             // 宝箱
    Special            // 特殊道具
}
