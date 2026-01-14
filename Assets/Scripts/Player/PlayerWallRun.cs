using UnityEngine;

/// <summary>
/// 玩家蹬墙跑系统 - 沿墙壁奔跑获得额外速度
/// </summary>
public class PlayerWallRun : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerData playerData;

    [Header("蹬墙跑设置")]
    [Tooltip("蹬墙跑时的速度倍率")]
    [Range(1.1f, 1.5f)] public float wallRunSpeedMultiplier = 1.2f;

    [Tooltip("蹬墙跑最大持续时间（秒）")]
    [Range(1f, 5f)] public float maxWallRunDuration = 3f;

    [Tooltip("蹬墙跑后可跳跃的额外高度")]
    [Range(5f, 15f)] public float wallRunJumpHeight = 10f;

    [Tooltip("蹬墙跑后水平速度倍率")]
    [Range(1.2f, 2f)] public float wallRunJumpSpeedMultiplier = 1.5f;

    [Tooltip("离开墙壁后的冷却时间")]
    [Range(0.5f, 2f)] public float wallRunCooldown = 1f;

    [Header("检测设置")]
    [Tooltip("墙壁检测距离")]
    [Range(0.5f, 2f)] public float wallCheckDistance = 1f;

    [Tooltip("墙壁检测层")]
    public LayerMask wallLayer = 1;

    [Tooltip("最大蹬墙跑角度（与墙壁法线的夹角）")]
    [Range(10f, 60f)] public float maxWallAngle = 45f;

    [Header("视觉效果")]
    [Tooltip("蹬墙跑时的相机倾斜角度")]
    [Range(5f, 20f)] public float cameraTiltAngle = 10f;

    [Tooltip("相机倾斜平滑速度")]
    [Range(5f, 15f)] public float cameraTiltSpeed = 10f;

    [Header("组件引用")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerModel;

    // 蹬墙跑状态
    private bool isWallRunning;
    private bool isOnLeftWall;
    private bool isOnRightWall;
    private float wallRunTimer;
    private float cooldownTimer;
    private Vector3 wallNormal;
    private RaycastHit wallHit;

    // 统计
    private float totalWallRunDistance;

    // 状态属性
    public bool IsWallRunning => isWallRunning;
    public bool IsOnLeftWall => isOnLeftWall;
    public bool IsOnRightWall => isOnRightWall;
    public float WallRunTimer => wallRunTimer;
    public float WallRunProgress => maxWallRunDuration > 0 ? wallRunTimer / maxWallRunDuration : 0f;
    public bool CanWallRun => cooldownTimer <= 0f;

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
        UpdateCooldown();
        CheckForWalls();
        UpdateWallRun();
    }

    /// <summary>
    /// 检测左右墙壁
    /// </summary>
    private void CheckForWalls()
    {
        if (!playerJump.IsGrounded && cooldownTimer <= 0f)
        {
            // 检测左侧墙壁
            isOnLeftWall = CheckWall(Vector3.left, out wallHit);
            if (isOnLeftWall)
            {
                wallNormal = wallHit.normal;
            }

            // 检测右侧墙壁
            if (!isOnLeftWall)
            {
                isOnRightWall = CheckWall(Vector3.right, out wallHit);
                if (isOnRightWall)
                {
                    wallNormal = wallHit.normal;
                }
            }
        }
        else
        {
            isOnLeftWall = false;
            isOnRightWall = false;
        }
    }

    /// <summary>
    /// 检测指定方向是否有墙壁
    /// </summary>
    private bool CheckWall(Vector3 direction, out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, direction, out hit, wallCheckDistance, wallLayer))
        {
            // 检查角度是否合适
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle < maxWallAngle || angle > (180f - maxWallAngle))
            {
                return false; // 太垂直的墙壁不能蹬墙跑
            }

            // 检查移动方向是否与墙壁平行
            float moveAngle = Vector3.Angle(transform.forward, -hit.normal);
            if (moveAngle > 90f)
            {
                return false; // 移动方向不对
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 更新蹬墙跑状态
    /// </summary>
    private void UpdateWallRun()
    {
        if ((isOnLeftWall || isOnRightWall) && !isWallRunning)
        {
            // 开始蹬墙跑
            StartWallRun();
        }
        else if (!(isOnLeftWall || isOnRightWall) && isWallRunning)
        {
            // 结束蹬墙跑
            EndWallRun();
        }
        else if (isWallRunning)
        {
            // 更新蹬墙跑计时器
            wallRunTimer += Time.deltaTime;

            // 检查是否超时
            if (wallRunTimer >= maxWallRunDuration)
            {
                EndWallRun();
            }

            // 应用轻微的重力（比正常慢）
            // PlayerJump组件会处理这个
        }
    }

    /// <summary>
    /// 开始蹬墙跑
    /// </summary>
    private void StartWallRun()
    {
        isWallRunning = true;
        wallRunTimer = 0f;

        // 触发事件
        // GameManager.Instance.Events.OnWallRunStart?.Invoke(isOnLeftWall);

        Debug.Log($"Wall run started on {(isOnLeftWall ? "left" : "right")} wall");
    }

    /// <summary>
    /// 结束蹬墙跑
    /// </summary>
    private void EndWallRun()
    {
        if (!isWallRunning) return;

        isWallRunning = false;
        cooldownTimer = wallRunCooldown;

        // 触发事件
        // GameManager.Instance.Events.OnWallRunEnd?.Invoke();

        Debug.Log("Wall run ended");
    }

    /// <summary>
    /// 获取蹬墙跑速度倍率
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return isWallRunning ? wallRunSpeedMultiplier : 1f;
    }

    /// <summary>
    /// 获取蹬墙跑时的重力倍率（空中时重力较轻）
    /// </summary>
    public float GetGravityMultiplier()
    {
        if (isWallRunning)
        {
            return 0.3f; // 蹬墙跑时重力较轻
        }
        return 1f;
    }

    /// <summary>
    /// 获取相机倾斜角度
    /// </summary>
    public float GetCameraTilt()
    {
        if (!isWallRunning) return 0f;

        float targetTilt = isOnLeftWall ? -cameraTiltAngle : cameraTiltAngle;
        return targetTilt;
    }

    /// <summary>
    /// 执行蹬墙跑跳跃
    /// </summary>
    public bool TryWallRunJump()
    {
        if (!isWallRunning) return false;

        // 计算跳跃方向（远离墙壁）
        Vector3 jumpDirection = transform.forward + wallNormal * 0.5f;
        jumpDirection = jumpDirection.normalized;

        // 应用跳跃力
        // 这需要PlayerJump组件配合

        // 结束蹬墙跑
        EndWallRun();

        // 触发事件
        // GameManager.Instance.Events.OnWallRunJump?.Invoke(jumpDirection);

        return true;
    }

    /// <summary>
    /// 更新冷却
    /// </summary>
    private void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 重置蹬墙跑状态
    /// </summary>
    public void ResetWallRun()
    {
        isWallRunning = false;
        isOnLeftWall = false;
        isOnRightWall = false;
        wallRunTimer = 0f;
        cooldownTimer = 0f;
    }

    // 需要访问PlayerJump组件
    private PlayerJump playerJump;

    private void Start()
    {
        playerJump = GetComponent<PlayerJump>();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 绘制墙壁检测射线
        Gizmos.color = isOnLeftWall ? Color.red : Color.white;
        Vector3 leftOrigin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(leftOrigin, Vector3.left * wallCheckDistance);

        Gizmos.color = isOnRightWall ? Color.red : Color.white;
        Vector3 rightOrigin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(rightOrigin, Vector3.right * wallCheckDistance);

        // 绘制墙壁法线
        if (isWallRunning)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(wallHit.point, wallNormal * 2f);
        }
    }
#endif
}
