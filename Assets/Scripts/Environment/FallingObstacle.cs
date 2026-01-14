using UnityEngine;

/// <summary>
/// 掉落障碍物 - 当玩家接近时从上方掉落
/// </summary>
[RequireComponent(typeof(Obstacle))]
public class FallingObstacle : MonoBehaviour
{
    [Header("掉落设置")]
    [Tooltip("触发距离（玩家在此距离内时触发）")]
    [SerializeField] private float triggerDistance = 20f;

    [Tooltip("掉落延迟（秒）")]
    [SerializeField] private float fallDelay = 0.5f;

    [Tooltip("掉落速度")]
    [SerializeField] private float fallSpeed = 10f;

    [Header("起始位置")]
    [Tooltip("起始高度（相对于地面）")]
    [SerializeField] private float startHeight = 10f;

    [Tooltip("目标高度（相对于地面）")]
    [SerializeField] private float targetHeight = 0f;

    [Tooltip("提前警告时间（秒）")]
    [SerializeField] private float warningTime = 1f;

    // 状态
    private bool isTriggered = false;
    private bool isFalling = false;
    private float fallTimer = 0f;
    private Vector3 targetPosition;
    private Rigidbody rb;

    private void Awake()
    {
        // 设置起始位置
        Vector3 startPos = transform.position;
        startPos.y = startHeight;
        transform.position = startPos;

        targetPosition = transform.position;
        targetPosition.y = targetHeight;

        // 获取或添加Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    /// <summary>
    /// 检查玩家是否接近
    /// </summary>
    public bool CheckPlayerProximity(Vector3 playerPosition)
    {
        if (isTriggered || isFalling) return false;

        float distance = Vector3.Distance(transform.position, playerPosition);

        if (distance <= triggerDistance)
        {
            TriggerFall();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 触发掉落
    /// </summary>
    public void TriggerFall()
    {
        if (isTriggered) return;

        isTriggered = true;

        // 显示警告（Phase 9实现视觉特效）
        ShowWarning();

        // 延迟后开始掉落
        Invoke(nameof(StartFalling), fallDelay);
    }

    /// <summary>
    /// 开始掉落
    /// </summary>
    private void StartFalling()
    {
        isFalling = true;
        rb.isKinematic = false;
        rb.useGravity = true;

        // 给予向下的初速度
        rb.velocity = Vector3.down * fallSpeed;

        Debug.Log($"[FallingObstacle] {gameObject.name} started falling!");
    }

    /// <summary>
    /// 显示警告
    /// </summary>
    private void ShowWarning()
    {
        // TODO: Phase 9 实现警告特效（如闪烁红色、显示箭头等）
        Debug.Log($"[FallingObstacle] Warning! Obstacle will fall in {fallDelay + warningTime}s");

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            StartCoroutine(WarningFlash(renderer));
        }
    }

    private System.Collections.IEnumerator WarningFlash(Renderer renderer)
    {
        float timer = 0f;
        Color originalColor = renderer.material.color;

        while (timer < fallDelay + warningTime)
        {
            timer += Time.deltaTime;
            float flash = Mathf.PingPong(timer * 5f, 1f); // 快速闪烁
            renderer.material.color = Color.Lerp(originalColor, Color.red, flash);
            yield return null;
        }

        renderer.material.color = originalColor;
    }

    protected void OnUpdate()
    {
        if (!isFalling) return;

        // 检查是否到达地面
        if (transform.position.y <= targetHeight)
        {
            // 停止掉落
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            transform.position = targetPosition;

            // 如果是可破坏的，掉落后销毁
            Obstacle obstacle = GetComponent<Obstacle>();
            if (obstacle != null && obstacle.CanBeDestroyed)
            {
                obstacle.DestroyObstacle();
            }
        }
    }

    /// <summary>
    /// 重置障碍物
    /// </summary>
    public void ResetFallingObstacle()
    {
        isTriggered = false;
        isFalling = false;
        fallTimer = 0f;

        // 重置位置
        Vector3 newPos = transform.position;
        newPos.y = startHeight;
        transform.position = newPos;

        // 重置Rigidbody
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制触发范围和掉落路径
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制触发范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        // 绘制掉落路径
        Gizmos.color = Color.red;
        Vector3 topPos = transform.position;
        topPos.y = startHeight;
        Vector3 bottomPos = transform.position;
        bottomPos.y = targetHeight;

        Gizmos.DrawLine(topPos, bottomPos);
        Gizmos.DrawWireSphere(topPos, 0.5f);
        Gizmos.DrawWireSphere(bottomPos, 0.5f);
    }
#endif
}
