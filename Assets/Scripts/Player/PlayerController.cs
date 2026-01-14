using UnityEngine;
using System;

/// <summary>
/// 玩家核心控制器 - 整合所有玩家系统
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerData playerData;

    [Header("核心组件")]
    [SerializeField] private LaneManager laneManager;
    [SerializeField] private PlayerJump playerJump;

    [Header("跑酷动作组件")]
    [SerializeField] private PlayerSlide playerSlide;
    [SerializeField] private PlayerCrouch playerCrouch;
    [SerializeField] private PlayerWallRun playerWallRun;
    [SerializeField] private PlayerClimb playerClimb;

    [Header("可视化")]
    [SerializeField] private bool showDebugInfo = true;

    // 组件
    private CharacterController characterController;

    // 玩家状态
    private float currentSpeed;
    private float distanceTraveled;
    private bool isGameActive = true;
    private bool isPaused = false;

    // 统计数据
    public int CoinsCollected { get; private set; }
    public float Score { get; private set; }

    // 事件
    public event Action OnPlayerDeath;
    public event Action<float> OnSpeedChanged;
    public event Action<int> OnCoinCollected;

    #region 属性

    public float CurrentSpeed => currentSpeed;
    public float DistanceTraveled => distanceTraveled;
    public bool IsGameActive => isGameActive;
    public bool IsGrounded => playerJump.IsGrounded;
    public bool IsJumping => playerJump.IsJumping;
    public bool IsSliding => playerSlide != null && playerSlide.IsSliding;
    public bool IsCrouching => playerCrouch != null && playerCrouch.IsCrouching;
    public bool IsWallRunning => playerWallRun != null && playerWallRun.IsWallRunning;
    public bool IsClimbing => playerClimb != null && playerClimb.IsClimbing;
    public int CurrentLane => laneManager.CurrentLane;
    public PlayerData PlayerData => playerData;

    #endregion

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 获取或创建必需组件
        characterController = GetComponent<CharacterController>();

        // 获取或创建核心组件
        if (laneManager == null)
        {
            laneManager = GetComponent<LaneManager>();
            if (laneManager == null) laneManager = gameObject.AddComponent<LaneManager>();
        }

        if (playerJump == null)
        {
            playerJump = GetComponent<PlayerJump>();
            if (playerJump == null) playerJump = gameObject.AddComponent<PlayerJump>();
        }

        // 获取或创建跑酷动作组件（可选）
        if (playerSlide == null)
        {
            playerSlide = GetComponent<PlayerSlide>();
            if (playerSlide == null) playerSlide = gameObject.AddComponent<PlayerSlide>();
        }

        if (playerCrouch == null)
        {
            playerCrouch = GetComponent<PlayerCrouch>();
            if (playerCrouch == null) playerCrouch = gameObject.AddComponent<PlayerCrouch>();
        }

        if (playerWallRun == null)
        {
            playerWallRun = GetComponent<PlayerWallRun>();
            if (playerWallRun == null) playerWallRun = gameObject.AddComponent<PlayerWallRun>();
        }

        if (playerClimb == null)
        {
            playerClimb = GetComponent<PlayerClimb>();
            if (playerClimb == null) playerClimb = gameObject.AddComponent<PlayerClimb>();
        }

        // 初始化状态
        currentSpeed = playerData.baseSpeed;
        distanceTraveled = 0f;
        CoinsCollected = 0;
        Score = 0f;

        Debug.Log("PlayerController initialized with all parkour actions");
    }

    private void Update()
    {
        if (!isGameActive || isPaused) return;

        HandleInput();
        UpdateMovement();
        UpdateStats();
    }

    #region 输入处理

    private void HandleInput()
    {
        // 如果正在攀爬，禁止其他输入
        if (IsClimbing) return;

        // 变道控制
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            laneManager.MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            laneManager.MoveRight();
        }

        // 跳跃控制
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            // 优先尝试蹬墙跑跳跃
            if (playerWallRun != null && playerWallRun.IsWallRunning)
            {
                playerWallRun.TryWallRunJump();
            }
            // 其次尝试攀爬
            else if (playerClimb != null && playerClimb.CanClimb)
            {
                playerClimb.TryClimb();
            }
            // 普通跳跃
            else
            {
                playerJump.Jump();
            }
        }

        // 滑铲控制（按下时）
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (playerSlide != null && !playerSlide.IsSliding)
            {
                playerSlide.TrySlide();
            }
        }

        // 下蹲由PlayerCrouch组件自动处理（通过按键检测）
    }

    #endregion

    #region 移动系统

    private void UpdateMovement()
    {
        // 如果正在攀爬，跳过常规移动
        if (IsClimbing) return;

        float deltaTime = Time.deltaTime;

        // 1. 计算速度倍率（所有动作系统的综合）
        float speedMultiplier = CalculateSpeedMultiplier();
        float actualSpeed = currentSpeed * speedMultiplier;

        // 2. 更新水平位置（变道）
        float horizontalPosition = laneManager.UpdateLanePosition(deltaTime);

        // 3. 更新垂直位置（跳跃和重力）
        playerJump.UpdateJumpPhysics(deltaTime);
        Vector3 verticalMovement = playerJump.GetVerticalMovement();

        // 应用蹬墙跑的重力修改
        if (playerWallRun != null && playerWallRun.IsWallRunning)
        {
            verticalMovement *= playerWallRun.GetGravityMultiplier();
        }

        // 4. 构建移动向量
        Vector3 movement = Vector3.zero;

        // 水平变道
        movement.x = horizontalPosition - transform.position.x;

        // 前进移动（使用实际速度）
        movement.z = actualSpeed * deltaTime;

        // 垂直移动
        movement.y = verticalMovement.y * deltaTime;

        // 5. 应用移动
        characterController.Move(movement);

        // 6. 确保不会在Z轴上倒退
        if (transform.position.z < 0)
        {
            Vector3 pos = transform.position;
            pos.z = 0;
            transform.position = pos;
        }
    }

    /// <summary>
    /// 计算综合速度倍率
    /// </summary>
    private float CalculateSpeedMultiplier()
    {
        float multiplier = 1f;

        // 滑铲加速
        if (playerSlide != null && playerSlide.IsSliding)
        {
            multiplier *= playerSlide.GetSpeedMultiplier();
        }

        // 蹬墙跑加速
        if (playerWallRun != null && playerWallRun.IsWallRunning)
        {
            multiplier *= playerWallRun.GetSpeedMultiplier();
        }

        return multiplier;
    }

    #endregion

    #region 速度管理

    private void UpdateSpeed()
    {
        // 随着时间逐渐增加速度
        if (currentSpeed < playerData.maxSpeed)
        {
            float oldSpeed = currentSpeed;
            currentSpeed += playerData.speedIncreaseRate * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, playerData.maxSpeed);

            // 触发速度变化事件
            if (Mathf.Abs(currentSpeed - oldSpeed) > 0.1f)
            {
                OnSpeedChanged?.Invoke(currentSpeed);
            }
        }
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = Mathf.Clamp(speed, playerData.baseSpeed, playerData.maxSpeed);
        OnSpeedChanged?.Invoke(currentSpeed);
    }

    public void BoostSpeed(float boostAmount, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(boostAmount, duration));
    }

    private System.Collections.IEnumerator SpeedBoostCoroutine(float boostAmount, float duration)
    {
        float originalSpeed = currentSpeed;
        SetSpeed(originalSpeed + boostAmount);

        yield return new WaitForSeconds(duration);

        SetSpeed(originalSpeed);
    }

    #endregion

    #region 统计和得分

    private void UpdateStats()
    {
        // 更新速度
        UpdateSpeed();

        // 更新距离
        distanceTraveled += currentSpeed * Time.deltaTime;

        // 更新分数（距离 + 速度倍率）
        Score += currentSpeed * Time.deltaTime * 0.1f;
    }

    public void AddCoin(int amount = 1)
    {
        CoinsCollected += amount;
        Score += amount * 10f;
        OnCoinCollected?.Invoke(CoinsCollected);
    }

    #endregion

    #region 游戏状态

    public void StartGame()
    {
        isGameActive = true;
        currentSpeed = playerData.baseSpeed;
        Debug.Log("Game started!");
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void Die()
    {
        if (!isGameActive) return;

        isGameActive = false;
        OnPlayerDeath?.Invoke();
        Debug.Log($"Player died! Distance: {distanceTraveled:F1}m, Score: {Score:F0}");
    }

    public void ResetPlayer()
    {
        // 重置位置
        characterController.enabled = false;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        characterController.enabled = true;

        // 重置状态
        currentSpeed = playerData.baseSpeed;
        distanceTraveled = 0f;
        CoinsCollected = 0;
        Score = 0f;
        isGameActive = true;
        isPaused = false;

        // 重置核心子系统
        playerJump.ResetJump();
        laneManager.ChangeToLane(Mathf.FloorToInt(playerData.laneCount / 2));

        // 重置跑酷动作系统
        if (playerSlide != null) playerSlide.ResetSlide();
        if (playerCrouch != null) playerCrouch.ResetCrouch();
        if (playerWallRun != null) playerWallRun.ResetWallRun();
        if (playerClimb != null) playerClimb.ResetClimb();

        Debug.Log("Player reset with all parkour actions");
    }

    #endregion

    #region 碰撞处理

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 检查是否撞到障碍物
        if (hit.collider.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 收集金币
        if (other.CompareTag("Coin"))
        {
            AddCoin(1);
            Destroy(other.gameObject);
        }
        // 磁铁道具
        else if (other.CompareTag("Magnet"))
        {
            // TODO: 实现磁铁效果
            Destroy(other.gameObject);
        }
        // 护盾道具
        else if (other.CompareTag("Shield"))
        {
            // TODO: 实现护盾效果
            Destroy(other.gameObject);
        }
    }

    #endregion

    #region 调试和可视化

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.Box("Player Debug Info");

        GUILayout.Label("=== 核心状态 ===");
        GUILayout.Label($"Speed: {currentSpeed:F2} m/s");
        GUILayout.Label($"Distance: {distanceTraveled:F1} m");
        GUILayout.Label($"Score: {Score:F0}");
        GUILayout.Label($"Coins: {CoinsCollected}");
        GUILayout.Label($"Lane: {laneManager.CurrentLane}");

        GUILayout.Label("\n=== 移动状态 ===");
        GUILayout.Label($"Grounded: {playerJump.IsGrounded}");
        GUILayout.Label($"Jumping: {playerJump.IsJumping}");
        GUILayout.Label($"Can Double Jump: {playerJump.CanDoubleJump}");
        GUILayout.Label($"Vertical Vel: {playerJump.VerticalVelocity:F2}");

        GUILayout.Label("\n=== 跑酷动作 ===");
        GUILayout.Label($"Sliding: {IsSliding}");
        if (playerSlide != null && playerSlide.IsSliding)
        {
            GUILayout.Label($"  Slide Progress: {playerSlide.SlideProgress:P0}");
        }

        GUILayout.Label($"Crouching: {IsCrouching}");
        GUILayout.Label($"Wall Running: {IsWallRunning}");
        if (playerWallRun != null && playerWallRun.IsWallRunning)
        {
            GUILayout.Label($"  On Left Wall: {playerWallRun.IsOnLeftWall}");
            GUILayout.Label($"  Wall Run Progress: {playerWallRun.WallRunProgress:P0}");
        }

        GUILayout.Label($"Climbing: {IsClimbing}");
        if (playerClimb != null && playerClimb.IsClimbing)
        {
            GUILayout.Label($"  Climb Progress: {playerClimb.ClimbProgress:P0}");
        }

        GUILayout.EndArea();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 绘制速度向量
        if (characterController != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * 2);

            // 绘制车道信息
            if (laneManager != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 lanePos = transform.position + Vector3.right * laneManager.TargetXPosition;
                Gizmos.DrawWireSphere(lanePos, 0.5f);
            }
        }
    }
#endif

    #endregion
}
