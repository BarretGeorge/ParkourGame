using UnityEngine;

/// <summary>
/// 车道管理器 - 处理玩家在不同车道间的切换
/// </summary>
public class LaneManager : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private PlayerData playerData;

    // 当前状态
    private int currentLane = 1; // 中间车道（假设3车道）
    private float targetXPosition;
    private float currentXPosition;

    // 状态标志
    public bool IsChangingLane { get; private set; }
    public int CurrentLane => currentLane;
    public float TargetXPosition => targetXPosition;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 如果没有设置 PlayerData，尝试从 GameManager 获取
        if (playerData == null)
        {
            Debug.LogWarning("PlayerData not set on LaneManager. Using default values.");
        }

        // 计算初始车道位置
        int middleLane = Mathf.FloorToInt(playerData.laneCount / 2);
        currentLane = middleLane;
        targetXPosition = CalculateLanePosition(currentLane);
        currentXPosition = targetXPosition;
    }

    /// <summary>
    /// 计算指定车道的X坐标
    /// </summary>
    private float CalculateLanePosition(int laneIndex)
    {
        int middleLane = Mathf.FloorToInt(playerData.laneCount / 2);
        float offset = (laneIndex - middleLane) * playerData.laneWidth;
        return offset;
    }

    /// <summary>
    /// 向左移动一个车道
    /// </summary>
    public void MoveLeft()
    {
        if (currentLane > 0 && !IsChangingLane)
        {
            currentLane--;
            targetXPosition = CalculateLanePosition(currentLane);
            IsChangingLane = true;
        }
    }

    /// <summary>
    /// 向右移动一个车道
    /// </summary>
    public void MoveRight()
    {
        if (currentLane < playerData.laneCount - 1 && !IsChangingLane)
        {
            currentLane++;
            targetXPosition = CalculateLanePosition(currentLane);
            IsChangingLane = true;
        }
    }

    /// <summary>
    /// 立即切换到指定车道（用于某些特殊情况）
    /// </summary>
    public void ChangeToLane(int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < playerData.laneCount)
        {
            currentLane = laneIndex;
            targetXPosition = CalculateLanePosition(currentLane);
            IsChangingLane = true;
        }
    }

    /// <summary>
    /// 每帧更新车道位置
    /// </summary>
    public float UpdateLanePosition(float deltaTime)
    {
        // 平滑移动到目标位置
        float smoothSpeed = playerData.laneChangeSpeed;
        currentXPosition = Mathf.Lerp(
            currentXPosition,
            targetXPosition,
            smoothSpeed * deltaTime
        );

        // 检查是否接近目标位置
        if (Mathf.Abs(currentXPosition - targetXPosition) < 0.01f)
        {
            currentXPosition = targetXPosition;
            IsChangingLane = false;
        }

        return currentXPosition;
    }

    /// <summary>
    /// 获取车道切换进度（0-1，用于动画和特效）
    /// </summary>
    public float GetLaneChangeProgress()
    {
        float totalDistance = playerData.laneWidth;
        float currentDistance = Mathf.Abs(currentXPosition - targetXPosition);
        return 1f - (currentDistance / totalDistance);
    }

    /// <summary>
    /// 获取车道切换方向（-1左，1右，0无）
    /// </summary>
    public int GetLaneChangeDirection()
    {
        if (!IsChangingLane) return 0;
        return targetXPosition > currentXPosition ? 1 : -1;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中绘制车道可视化
    /// </summary>
    private void OnDrawGizmos()
    {
        if (playerData == null) return;

        Gizmos.color = Color.cyan;

        // 绘制所有车道
        int middleLane = Mathf.FloorToInt(playerData.laneCount / 2);
        for (int i = 0; i < playerData.laneCount; i++)
        {
            float x = (i - middleLane) * playerData.laneWidth;
            Vector3 lanePos = transform.position + Vector3.right * x;
            Gizmos.DrawLine(lanePos + Vector3.back * 10, lanePos + Vector3.forward * 10);
        }

        // 绘制当前车道
        Gizmos.color = Color.green;
        float currentX = (currentLane - middleLane) * playerData.laneWidth;
        Vector3 currentLanePos = transform.position + Vector3.right * currentX;
        Gizmos.DrawLine(currentLanePos + Vector3.back * 10, currentLanePos + Vector3.forward * 10);
    }
#endif
}
