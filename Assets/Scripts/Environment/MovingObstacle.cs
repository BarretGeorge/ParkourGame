using UnityEngine;

/// <summary>
/// 移动障碍物 - 在预设路径上移动的障碍物
/// </summary>
[RequireComponent(typeof(Obstacle))]
public class MovingObstacle : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动路径点（本地坐标）")]
    [SerializeField] private Vector3[] waypoints = new Vector3[] { Vector3.left * 3f, Vector3.right * 3f };

    [Tooltip("移动速度")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("移动方式")]
    [SerializeField] private MovementType movementType = MovementType.PingPong;

    [Tooltip("是否循环")]
    [SerializeField] private bool loop = true;

    [Header("高级设置")]
    [Tooltip("等待时间（到达每个点后等待）")]
    [SerializeField] private float waitTime = 0f;

    [Tooltip("移动曲线（控制速度变化）")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // 状态
    private int currentWaypointIndex = 0;
    private bool movingForward = true;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.localPosition;
    }

    protected void OnUpdate()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        if (isWaiting)
        {
            UpdateWait();
            return;
        }

        MoveTowardsWaypoint();
    }

    /// <summary>
    /// 向路径点移动
    /// </summary>
    private void MoveTowardsWaypoint()
    {
        Vector3 targetPosition = startPosition + waypoints[currentWaypointIndex];
        Vector3 currentPos = transform.localPosition;

        // 计算方向
        Vector3 direction = (targetPosition - currentPos).normalized;
        float distance = Vector3.Distance(currentPos, targetPosition);

        // 计算速度（应用曲线）
        float progress = 1f - (distance / GetTotalPathLength());
        float curveValue = moveCurve.Evaluate(Mathf.Clamp01(progress));
        float currentSpeed = moveSpeed * curveValue;

        // 移动
        transform.localPosition += direction * currentSpeed * Time.deltaTime;

        // 检查是否到达
        if (distance < 0.1f)
        {
            OnWaypointReached();
        }
    }

    /// <summary>
    /// 到达路径点
    /// </summary>
    private void OnWaypointReached()
    {
        // 开始等待
        if (waitTime > 0f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }

        // 移动到下一个点
        switch (movementType)
        {
            case MovementType.Loop:
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                break;

            case MovementType.PingPong:
                if (movingForward)
                {
                    if (currentWaypointIndex >= waypoints.Length - 1)
                    {
                        movingForward = false;
                        currentWaypointIndex--;
                    }
                    else
                    {
                        currentWaypointIndex++;
                    }
                }
                else
                {
                    if (currentWaypointIndex <= 0)
                    {
                        movingForward = true;
                        currentWaypointIndex++;
                    }
                    else
                    {
                        currentWaypointIndex--;
                    }
                }
                break;

            case MovementType.Once:
                if (currentWaypointIndex < waypoints.Length - 1)
                {
                    currentWaypointIndex++;
                }
                else
                {
                    // 完成一次移动，停止或重新开始
                    if (loop)
                    {
                        currentWaypointIndex = 0;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 更新等待
    /// </summary>
    private void UpdateWait()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTime)
        {
            isWaiting = false;
            waitTimer = 0f;
        }
    }

    /// <summary>
    /// 获取总路径长度
    /// </summary>
    private float GetTotalPathLength()
    {
        if (waypoints == null || waypoints.Length < 2) return 1f;

        float totalLength = 0f;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            totalLength += Vector3.Distance(waypoints[i], waypoints[i + 1]);
        }
        return totalLength;
    }

    /// <summary>
    /// 获取当前移动速度
    /// </summary>
    public float GetCurrentSpeed()
    {
        if (isWaiting) return 0f;

        float progress = 1f - (Vector3.Distance(transform.localPosition, startPosition + waypoints[currentWaypointIndex]) / GetTotalPathLength());
        return moveSpeed * moveCurve.Evaluate(Mathf.Clamp01(progress));
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制移动路径
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        Vector3 basePos = Application.isPlaying ? startPosition : transform.localPosition;

        // 绘制路径
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 point = basePos + waypoints[i];
            Gizmos.DrawWireSphere(point, 0.3f);

            if (i < waypoints.Length - 1)
            {
                Vector3 nextPoint = basePos + waypoints[i + 1];
                Gizmos.DrawLine(point, nextPoint);
            }
        }

        // 如果是PingPong模式，绘制返回路径
        if (movementType == MovementType.PingPong)
        {
            Gizmos.color = Color.cyan;
            for (int i = waypoints.Length - 1; i > 0; i--)
            {
                Vector3 point = basePos + waypoints[i];
                Vector3 prevPoint = basePos + waypoints[i - 1];
                Gizmos.DrawLine(point, prevPoint);
            }
        }

        // 高亮当前目标
        Gizmos.color = Color.red;
        if (Application.isPlaying && currentWaypointIndex < waypoints.Length)
        {
            Vector3 currentTarget = basePos + waypoints[currentWaypointIndex];
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
        }
    }
#endif
}

/// <summary>
/// 移动类型
/// </summary>
public enum MovementType
{
    Loop,     // 循环
    PingPong, // 来回
    Once      // 单次
}
