using UnityEngine;

/// <summary>
/// 玩家滑铲系统 - 快速通过低矮障碍物
/// </summary>
public class PlayerSlide : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerData playerData;

    [Header("滑铲设置")]
    [Tooltip("滑铲时的角色高度")]
    [Range(0.5f, 1.5f)] public float slideHeight = 1f;

    [Tooltip("滑铲持续时间（秒）")]
    [Range(0.3f, 1.5f)] public float slideDuration = 0.8f;

    [Tooltip("滑铲速度倍率")]
    [Range(1.2f, 2f)] public float slideSpeedMultiplier = 1.5f;

    [Tooltip("滑铲冷却时间")]
    [Range(0.1f, 1f)] public float slideCooldown = 0.3f;

    [Header("组件引用")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerModel;

    // 滑铲状态
    private bool isSliding;
    private bool canSlide = true;
    private float slideTimer;
    private float cooldownTimer;
    private float originalHeight;
    private float originalCenterY;

    // 碰撞检测
    private RaycastHit slideHit;
    private bool hasLowObstacle;

    // 状态属性
    public bool IsSliding => isSliding;
    public bool CanSlide => canSlide;
    public float SlideProgress => isSliding ? slideTimer / slideDuration : 0f;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (playerModel == null)
        {
            // 查找玩家模型（第一个子对象）
            if (transform.childCount > 0)
            {
                playerModel = transform.GetChild(0);
            }
        }

        // 保存原始值
        originalHeight = characterController.height;
        originalCenterY = characterController.center.y;
    }

    private void Update()
    {
        UpdateCooldown();
        UpdateSlide();
        CheckForLowObstacles();
    }

    /// <summary>
    /// 尝试开始滑铲
    /// </summary>
    public bool TrySlide()
    {
        if (!canSlide || isSliding) return false;

        StartSlide();
        return true;
    }

    /// <summary>
    /// 开始滑铲
    /// </summary>
    private void StartSlide()
    {
        isSliding = true;
        slideTimer = 0f;
        canSlide = false;

        // 调整CharacterController高度
        AdjustControllerHeight(slideHeight);

        // 触发滑铲事件（用于动画和特效）
        // GameManager.Instance.Events.OnPlayerSlideStart?.Invoke();

        Debug.Log("Slide started!");
    }

    /// <summary>
    /// 更新滑铲状态
    /// </summary>
    private void UpdateSlide()
    {
        if (!isSliding) return;

        slideTimer += Time.deltaTime;

        // 检查滑铲是否结束
        if (slideTimer >= slideDuration)
        {
            EndSlide();
        }
        // 或者如果前方没有低矮障碍物，提前结束
        else if (!hasLowObstacle && slideTimer > slideDuration * 0.5f)
        {
            EndSlide();
        }
    }

    /// <summary>
    /// 结束滑铲
    /// </summary>
    private void EndSlide()
    {
        isSliding = false;
        cooldownTimer = slideCooldown;

        // 恢复CharacterController高度
        AdjustControllerHeight(originalHeight);

        // 触发滑铲结束事件
        // GameManager.Instance.Events.OnPlayerSlideEnd?.Invoke();

        Debug.Log("Slide ended!");
    }

    /// <summary>
    /// 更新冷却
    /// </summary>
    private void UpdateCooldown()
    {
        if (!canSlide)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canSlide = true;
            }
        }
    }

    /// <summary>
    /// 检测前方是否有低矮障碍物
    /// </summary>
    private void CheckForLowObstacles()
    {
        // 向前发射射线检测低矮障碍物
        Vector3 origin = transform.position + Vector3.up * slideHeight * 0.5f;
        hasLowObstacle = Physics.Raycast(
            origin,
            transform.forward,
            out slideHit,
            3f,
            playerData.groundLayer
        );
    }

    /// <summary>
    /// 调整控制器高度
    /// </summary>
    private void AdjustControllerHeight(float newHeight)
    {
        // 平滑过渡高度
        characterController.height = Mathf.Lerp(
            characterController.height,
            newHeight,
            10f * Time.deltaTime
        );

        // 调整中心点以保持底部在地面
        Vector3 newCenter = characterController.center;
        newCenter.y = newHeight * 0.5f;
        characterController.center = newCenter;
    }

    /// <summary>
    /// 获取滑铲速度倍率
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return isSliding ? slideSpeedMultiplier : 1f;
    }

    /// <summary>
    /// 强制结束滑铲（用于特殊情况）
    /// </summary>
    public void ForceEndSlide()
    {
        if (isSliding)
        {
            EndSlide();
        }
    }

    /// <summary>
    /// 重置滑铲状态
    /// </summary>
    public void ResetSlide()
    {
        isSliding = false;
        canSlide = true;
        slideTimer = 0f;
        cooldownTimer = 0f;
        AdjustControllerHeight(originalHeight);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 可视化滑铲检测
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!isSliding) return;

        // 绘制滑铲高度
        Gizmos.color = Color.yellow;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(
            pos + Vector3.up * 0.1f,
            pos + Vector3.up * slideHeight
        );

        // 绘制低矮障碍物检测射线
        Gizmos.color = hasLowObstacle ? Color.red : Color.green;
        Vector3 origin = transform.position + Vector3.up * slideHeight * 0.5f;
        Gizmos.DrawRay(origin, transform.forward * 3f);
    }
#endif
}
