using UnityEngine;

/// <summary>
/// 玩家下蹲系统 - 躲避高处障碍物
/// </summary>
public class PlayerCrouch : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerData playerData;

    [Header("下蹲设置")]
    [Tooltip("下蹲时的角色高度")]
    [Range(0.5f, 1.5f)] public float crouchHeight = 1.2f;

    [Tooltip("下蹲过渡速度")]
    [Range(5f, 20f)] public float crouchTransitionSpeed = 10f;

    [Tooltip("是否保持下蹲状态")]
    public bool holdToCrouch = true;

    [Header("组件引用")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerModel;

    // 下蹲状态
    private bool isCrouching;
    private bool wantsToCrouch;
    private float originalHeight;
    private float originalCenterY;
    private float currentHeight;

    // 状态属性
    public bool IsCrouching => isCrouching;
    public float CrouchHeight => crouchHeight;

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
            if (transform.childCount > 0)
            {
                playerModel = transform.GetChild(0);
            }
        }

        // 保存原始值
        originalHeight = characterController.height;
        currentHeight = originalHeight;
    }

    private void Update()
    {
        HandleInput();
        UpdateCrouch();
    }

    /// <summary>
    /// 处理下蹲输入
    /// </summary>
    private void HandleInput()
    {
        // 根据配置决定是按住还是切换
        if (holdToCrouch)
        {
            // 按住下蹲键
            wantsToCrouch = Input.GetKey(KeyCode.LeftControl) ||
                           Input.GetKey(KeyCode.RightControl) ||
                           Input.GetKey(KeyCode.S) ||
                           Input.GetKey(KeyCode.DownArrow);
        }
        else
        {
            // 切换模式（按下一次）
            if (Input.GetKeyDown(KeyCode.LeftControl) ||
                Input.GetKeyDown(KeyCode.RightControl) ||
                Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.DownArrow))
            {
                wantsToCrouch = !isCrouching;
            }
        }
    }

    /// <summary>
    /// 更新下蹲状态
    /// </summary>
    private void UpdateCrouch()
    {
        float targetHeight = wantsToCrouch ? crouchHeight : originalHeight;

        // 平滑过渡高度
        currentHeight = Mathf.Lerp(
            currentHeight,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        // 应用高度变化
        characterController.height = currentHeight;

        // 调整中心点
        Vector3 newCenter = characterController.center;
        newCenter.y = currentHeight * 0.5f;
        characterController.center = newCenter;

        // 更新状态
        isCrouching = wantsToCrouch;

        // 如果有玩家模型，也缩放模型
        if (playerModel != null)
        {
            float scaleRatio = currentHeight / originalHeight;
            Vector3 scale = playerModel.localScale;
            scale.y = Mathf.Lerp(scale.y, scaleRatio, crouchTransitionSpeed * Time.deltaTime);
            playerModel.localScale = scale;
        }
    }

    /// <summary>
    /// 设置下蹲状态（用于外部控制）
    /// </summary>
    public void SetCrouch(bool crouch)
    {
        wantsToCrouch = crouch;
    }

    /// <summary>
    /// 切换下蹲状态
    /// </summary>
    public void ToggleCrouch()
    {
        wantsToCrouch = !isCrouching;
    }

    /// <summary>
    /// 强制站立
    /// </summary>
    public void ForceStand()
    {
        wantsToCrouch = false;
        currentHeight = originalHeight;
        characterController.height = originalHeight;
        Vector3 newCenter = characterController.center;
        newCenter.y = originalHeight * 0.5f;
        characterController.center = newCenter;
    }

    /// <summary>
    /// 重置下蹲状态
    /// </summary>
    public void ResetCrouch()
    {
        wantsToCrouch = false;
        isCrouching = false;
        currentHeight = originalHeight;
        characterController.height = originalHeight;
        Vector3 newCenter = characterController.center;
        newCenter.y = originalHeight * 0.5f;
        characterController.center = newCenter;

        if (playerModel != null)
        {
            Vector3 scale = playerModel.localScale;
            scale.y = 1f;
            playerModel.localScale = scale;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (characterController == null) return;

        // 绘制当前高度
        Gizmos.color = isCrouching ? Color.yellow : Color.green;
        Vector3 pos = transform.position;
        float h = isCrouching ? crouchHeight : characterController.height;
        Gizmos.DrawLine(
            pos,
            pos + Vector3.up * h
        );

        // 绘制目标高度
        Gizmos.color = Color.blue;
        Vector3 targetPos = pos + Vector3.right * 0.5f;
        float targetH = wantsToCrouch ? crouchHeight : originalHeight;
        Gizmos.DrawLine(
            targetPos,
            targetPos + Vector3.up * targetH
        );
    }
#endif
}
