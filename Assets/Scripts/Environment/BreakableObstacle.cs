using UnityEngine;

/// <summary>
/// 可破坏障碍物 - 可以被玩家破坏的障碍物
/// </summary>
[RequireComponent(typeof(Obstacle))]
public class BreakableObstacle : MonoBehaviour
{
    [Header("破坏设置")]
    [Tooltip("需要碰撞次数")]
    [SerializeField] private int hitsToDestroy = 1;

    [Tooltip("可以通过滑铲破坏")]
    [SerializeField] private bool breakableBySlide = true;

    [Tooltip("可以通过跳跃破坏")]
    [SerializeField] private bool breakableByJump = false;

    [Tooltip("可以通过蹬墙跑破坏")]
    [SerializeField] private bool breakableByWallRun = false;

    [Header("视觉效果")]
    [Tooltip("受伤时闪烁")]
    [SerializeField] private bool flashOnHit = true;

    [Tooltip("受伤闪烁颜色")]
    [SerializeField] private Color hitColor = Color.yellow;

    [Tooltip("破坏特效预制件")]
    [SerializeField] private GameObject destroyEffectPrefab;

    // 状态
    private int currentHits = 0;
    private Renderer obstacleRenderer;
    private Color originalColor;
    private Obstacle obstacle;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        obstacle = GetComponent<Obstacle>();
        obstacleRenderer = GetComponent<Renderer>();

        if (obstacleRenderer != null)
        {
            originalColor = obstacleRenderer.material.color;
        }

        // 设置为可破坏
        if (obstacle != null)
        {
            obstacle.canBeDestroyed = true;
            obstacle.health = hitsToDestroy;
        }
    }

    /// <summary>
    /// 处理特殊破坏方式
    /// </summary>
    public void HandleSpecialBreak(PlayerController player)
    {
        if (obstacle == null || obstacle.IsDestroyed) return;

        bool canBreak = false;

        // 检查破坏条件
        if (breakableBySlide && player.IsSliding)
        {
            canBreak = true;
            Debug.Log($"[BreakableObstacle] Broken by slide!");
        }
        else if (breakableByJump && player.IsJumping)
        {
            canBreak = true;
            Debug.Log($"[BreakableObstacle] Broken by jump!");
        }
        else if (breakableByWallRun && player.IsWallRunning)
        {
            canBreak = true;
            Debug.Log($"[BreakableObstacle] Broken by wall run!");
        }

        if (canBreak)
        {
            // 直接破坏
            obstacle.DestroyObstacle();

            // 给予额外奖励（金币）
            player.AddCoin(5);

            // 播放破坏特效
            PlayDestroyEffect();
        }
    }

    /// <summary>
    /// 处理受到伤害
    /// </summary>
    public void OnHit()
    {
        currentHits++;

        // 视觉反馈
        if (flashOnHit)
        {
            Flash();
        }

        // 震动效果
        if (Camera.main != null)
        {
            CameraController cameraController = Camera.main.GetComponent<CameraController>();
            cameraController?.ShakeCamera(0.2f, 0.1f);
        }

        // 检查是否销毁
        if (currentHits >= hitsToDestroy)
        {
            if (obstacle != null)
            {
                obstacle.DestroyObstacle();
            }
            PlayDestroyEffect();
        }
    }

    /// <summary>
    /// 闪烁效果
    /// </summary>
    private void Flash()
    {
        if (obstacleRenderer == null) return;

        StartCoroutine(FlashCoroutine());
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        if (obstacleRenderer != null)
        {
            obstacleRenderer.material.color = hitColor;
            yield return new WaitForSeconds(0.1f);
            obstacleRenderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// 播放破坏特效
    /// </summary>
    private void PlayDestroyEffect()
    {
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // 默认特效：简单的粒子爆炸
            CreateDefaultDestroyEffect();
        }
    }

    /// <summary>
    /// 创建默认破坏特效
    /// </summary>
    private void CreateDefaultDestroyEffect()
    {
        // TODO: Phase 9 实现完整的粒子系统
        Debug.Log($"[BreakableObstacle] Creating default destroy effect");

        // 简单的视觉反馈
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 0.5f;

        // 向外扩散的碎片
        for (int i = 0; i < 8; i++)
        {
            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fragment.transform.position = transform.position;
            fragment.transform.localScale = Vector3.one * 0.2f;

            // 随机方向
            Vector3 dir = Random.onUnitSphere;
            fragment.GetComponent<Rigidbody>()?.AddForce(dir * 5f, ForceMode.Impulse);

            // 自动销毁
            Destroy(fragment, 2f);
        }

        Destroy(effect, 0.5f);
    }

    /// <summary>
    /// 重置障碍物
    /// </summary>
    public void ResetBreakable()
    {
        currentHits = 0;

        if (obstacleRenderer != null)
        {
            obstacleRenderer.material.color = originalColor;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 显示破坏信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 显示剩余血量
        GUI.color = Color.white;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        if (screenPos.z > 0)
        {
            Vector2 guiPos = GUIUtility.ScreenToGUIPoint(screenPos);
            Rect rect = new Rect(guiPos.x - 20f, Screen.height - guiPos.y, 40f, 20f);
            GUI.Box(rect, $"{currentHits}/{hitsToDestroy}");
        }
    }
#endif
}
