using UnityEngine;

/// <summary>
/// 玩家跳跃系统 - 处理跳跃、二段跳和重力
/// </summary>
public class PlayerJump : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerData playerData;

    [Header("组件引用")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform groundCheck;

    // 跳跃状态
    private bool isGrounded;
    private bool canDoubleJump;
    private float verticalVelocity;
    private bool isJumping;
    private float lastGroundedTime;

    // 状态属性
    public bool IsGrounded => isGrounded;
    public bool IsJumping => isJumping;
    public bool CanDoubleJump => canDoubleJump && playerData.enableDoubleJump;
    public float VerticalVelocity => verticalVelocity;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 自动获取 CharacterController
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        // 创建地面检测点
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = Vector3.down * 0.1f;
            groundCheck = groundCheckObj.transform;
        }

        ResetState();
    }

    private void ResetState()
    {
        isGrounded = true;
        canDoubleJump = false;
        verticalVelocity = 0f;
        isJumping = false;
    }

    /// <summary>
    /// 执行普通跳跃
    /// </summary>
    public void Jump()
    {
        if (isGrounded)
        {
            PerformJump(playerData.jumpHeight);
            canDoubleJump = true;
        }
        else if (canDoubleJump)
        {
            PerformDoubleJump();
        }
    }

    /// <summary>
    /// 执行跳跃
    /// </summary>
    private void PerformJump(float height)
    {
        // 计算跳跃初速度: v = sqrt(2 * g * h)
        float gravity = Physics.gravity.y * playerData.gravityMultiplier;
        verticalVelocity = Mathf.Sqrt(2 * -gravity * height);
        isJumping = true;
    }

    /// <summary>
    /// 执行二段跳
    /// </summary>
    private void PerformDoubleJump()
    {
        PerformJump(playerData.doubleJumpHeight);
        canDoubleJump = false;

        // 触发二段跳特效（后续实现）
        // GameManager.Instance.EffectsManager.PlayDoubleJumpEffect(transform.position);
    }

    /// <summary>
    /// 更新跳跃物理
    /// </summary>
    public void UpdateJumpPhysics(float deltaTime)
    {
        // 检测是否在地面
        CheckGrounded();

        // 应用重力
        float gravity = Physics.gravity.y * playerData.gravityMultiplier;

        if (!isGrounded)
        {
            // 在空中时应用重力
            verticalVelocity += gravity * deltaTime;
        }
        else
        {
            // 在地面时重置垂直速度
            if (verticalVelocity < 0)
            {
                verticalVelocity = -1f; // 轻微向下压力，保持贴地
                isJumping = false;
            }
        }

        // 甘地判定 - 离地后短暂时间内仍可跳跃
        if (!isGrounded)
        {
            lastGroundedTime += deltaTime;
        }
        else
        {
            lastGroundedTime = 0f;
        }
    }

    /// <summary>
    /// 检测是否在地面
    /// </summary>
    private void CheckGrounded()
    {
        // 使用 CharacterController 的地面检测
        isGrounded = characterController.isGrounded;

        // 额外的射线检测（更精确）
        if (!isGrounded)
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                0.1f,
                playerData.groundLayer
            );
        }

        // 落地时重置二段跳
        if (isGrounded && verticalVelocity < 0)
        {
            canDoubleJump = false;
            isJumping = false;
        }
    }

    /// <summary>
    /// 获取垂直移动向量
    /// </summary>
    public Vector3 GetVerticalMovement()
    {
        return Vector3.up * verticalVelocity;
    }

    /// <summary>
    /// 检查是否可以跳跃（包含甘地判定）
    /// </summary>
    public bool CanJump()
    {
        return isGrounded || (lastGroundedTime < playerData.verticalGraceTime);
    }

    /// <summary>
    /// 强制设置接地状态（用于某些特殊情况）
    /// </summary>
    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
        if (grounded)
        {
            verticalVelocity = -1f;
            isJumping = false;
        }
    }

    /// <summary>
    /// 重置跳跃状态（用于复活）
    /// </summary>
    public void ResetJump()
    {
        verticalVelocity = 0f;
        isJumping = false;
        canDoubleJump = false;
        lastGroundedTime = 0f;
    }
}
