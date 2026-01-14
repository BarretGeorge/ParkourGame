using UnityEngine;

/// <summary>
/// 玩家攀爬系统 - 自动攀爬障碍物
/// </summary>
public class PlayerClimb : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerData playerData;

    [Header("攀爬设置")]
    [Tooltip("攀爬速度")]
    [Range(3f, 10f)] public float climbSpeed = 5f;

    [Tooltip("最大攀爬高度")]
    [Range(2f, 6f)] public float maxClimbHeight = 4f;

    [Tooltip("攀爬检测距离")]
    [Range(1f, 3f)] public float climbCheckDistance = 2f;

    [Tooltip("攀爬后跳跃高度")]
    [Range(3f, 10f)] public float climbJumpHeight = 6f;

    [Tooltip("可攀爬物体的层")]
    public LayerMask climbableLayer = 1;

    [Header("动画设置")]
    [Tooltip("攀爬动画时长")]
    [Range(0.3f, 1f)] public float climbAnimationDuration = 0.5f;

    [Header("组件引用")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerModel;

    // 攀爬状态
    private bool isClimbing;
    private bool canClimb;
    private Vector3 climbTargetPosition;
    private GameObject currentClimbableObject;
    private float climbTimer;
    private Vector3 climbStartPos;

    // 检测
    private RaycastHit climbHit;

    // 状态属性
    public bool IsClimbing => isClimbing;
    public bool CanClimb => canClimb;
    public float ClimbProgress => isClimbing ? climbTimer / climbAnimationDuration : 0f;

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

        if (playerModel == null && transform.childCount > 0)
        {
            playerModel = transform.GetChild(0);
        }
    }

    private void Update()
    {
        CheckForClimbable();
        UpdateClimb();
    }

    /// <summary>
    /// 检测前方是否有可攀爬物体
    /// </summary>
    private void CheckForClimbable()
    {
        if (isClimbing) return;

        // 从玩家中心向前发射射线
        Vector3 origin = transform.position + Vector3.up * 1f;
        canClimb = Physics.Raycast(
            origin,
            transform.forward,
            out climbHit,
            climbCheckDistance,
            climbableLayer
        );

        if (canClimb)
        {
            // 计算攀爬目标位置
            CalculateClimbTarget();
            currentClimbableObject = climbHit.collider.gameObject;
        }
        else
        {
            currentClimbableObject = null;
        }
    }

    /// <summary>
    /// 计算攀爬目标位置
    /// </summary>
    private void CalculateClimbTarget()
    {
        // 目标位置在障碍物顶部
        Vector3 targetPos = climbHit.point;

        // 向上找到障碍物顶部
        RaycastHit topHit;
        if (Physics.Raycast(
            climbHit.point + Vector3.up * 0.1f,
            Vector3.up,
            out topHit,
            maxClimbHeight,
            climbableLayer))
        {
            // 找到了顶部
            targetPos.y = topHit.point.y;
        }
        else
        {
            // 没有找到顶部，使用最大高度
            targetPos.y = climbHit.point.y + maxClimbHeight;
        }

        // 确保目标位置在玩家前方一点
        targetPos += transform.forward * 0.5f;

        climbTargetPosition = targetPos;
    }

    /// <summary>
    /// 尝试开始攀爬
    /// </summary>
    public bool TryClimb()
    {
        if (!canClimb || isClimbing) return false;

        StartClimb();
        return true;
    }

    /// <summary>
    /// 开始攀爬
    /// </summary>
    private void StartClimb()
    {
        isClimbing = true;
        climbTimer = 0f;
        climbStartPos = transform.position;

        // 禁用CharacterController以便手动控制位置
        characterController.enabled = false;

        // 触发事件
        // GameManager.Instance.Events.OnPlayerClimbStart?.Invoke();

        Debug.Log("Climb started!");
    }

    /// <summary>
    /// 更新攀爬动画
    /// </summary>
    private void UpdateClimb()
    {
        if (!isClimbing) return;

        climbTimer += Time.deltaTime;
        float progress = climbTimer / climbAnimationDuration;

        if (progress >= 1f)
        {
            // 攀爬完成
            FinishClimb();
        }
        else
        {
            // 平滑移动到目标位置
            transform.position = Vector3.Lerp(
                climbStartPos,
                climbTargetPosition,
                progress
            );

            // 简单的抛物线高度变化
            float arcHeight = 1f;
            Vector3 pos = transform.position;
            pos.y += Mathf.Sin(progress * Mathf.PI) * arcHeight * Time.deltaTime;
            transform.position = pos;
        }
    }

    /// <summary>
    /// 完成攀爬
    /// </summary>
    private void FinishClimb()
    {
        transform.position = climbTargetPosition;

        // 重新启用CharacterController
        characterController.enabled = true;

        isClimbing = false;

        // 触发事件
        // GameManager.Instance.Events.OnPlayerClimbEnd?.Invoke();

        Debug.Log("Climb finished!");
    }

    /// <summary>
    /// 强制结束攀爬（用于被打断的情况）
    /// </summary>
    public void ForceEndClimb()
    {
        if (!isClimbing) return;

        // 重新启用CharacterController
        characterController.enabled = true;

        isClimbing = false;
        canClimb = false;

        Debug.Log("Climb force ended!");
    }

    /// <summary>
    /// 重置攀爬状态
    /// </summary>
    public void ResetClimb()
    {
        if (isClimbing)
        {
            characterController.enabled = true;
        }

        isClimbing = false;
        canClimb = false;
        climbTimer = 0f;
        currentClimbableObject = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 绘制攀爬检测射线
        Gizmos.color = canClimb ? Color.green : Color.white;
        Vector3 origin = transform.position + Vector3.up * 1f;
        Gizmos.DrawRay(origin, transform.forward * climbCheckDistance);

        // 绘制目标位置
        if (canClimb)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(climbTargetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, climbTargetPosition);
        }

        // 绘制最大攀爬高度
        if (canClimb)
        {
            Gizmos.color = Color.blue;
            Vector3 topPos = climbHit.point + Vector3.up * maxClimbHeight;
            Gizmos.DrawLine(climbHit.point, topPos);
        }
    }
#endif
}
