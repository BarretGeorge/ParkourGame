using UnityEngine;
using System.Collections;

/// <summary>
/// 垃圾回收管理器 - 智能管理GC
/// </summary>
public class GCManager : MonoBehaviour
{
    [Header("GC设置")]
    [SerializeField] private bool enableAutoGC = true;
    [SerializeField] private float gcInterval = 30f; // 自动GC间隔
    [SerializeField] private long memoryThreshold = 500 * 1024 * 1024; // 500MB

    [Header("GC策略")]
    [SerializeField] private bool aggressiveMode = false;
    [SerializeField] private int frameSkipBetweenGC = 30;

    [Header("显示信息")]
    [SerializeField] private bool showGCInfo = true;

    private float gcTimer;
    private int frameCount;
    private long lastMemory;

    // 统计
    private int totalGCCalls;
    private float totalGCTime;

    // 单例
    private static GCManager _instance;
    public static GCManager Instance => _instance;

    // 事件
    public event System.Action OnGarbageCollection;
    public event System.Action<float> OnMemoryFreed;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!enableAutoGC) return;

        gcTimer += Time.deltaTime;
        frameCount++;

        // 检查内存阈值
        long currentMemory = System.GC.GetTotalMemory(false);

        if (currentMemory > memoryThreshold)
        {
            if (showGCInfo)
            {
                Debug.Log($"内存超限 ({currentMemory / (1024f * 1024f):F1} MB)，执行垃圾回收");
            }

            PerformGarbageCollection();
        }

        // 定时GC
        if (gcTimer >= gcInterval)
        {
            gcTimer = 0f;
            PerformGarbageCollection();
        }
    }

    /// <summary>
    /// 执行垃圾回收
    /// </summary>
    public void PerformGarbageCollection()
    {
        if (aggressiveMode)
        {
            PerformAggressiveGC();
        }
        else
        {
            PerformStandardGC();
        }
    }

    /// <summary>
    /// 标准垃圾回收
    /// </summary>
    private void PerformStandardGC()
    {
        float startTime = Time.realtimeSinceStartup;

        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        float endTime = Time.realtimeSinceStartup;
        float gcTime = (endTime - startTime) * 1000f; // 转换为毫秒

        totalGCCalls++;
        totalGCTime += gcTime;

        long memoryBefore = lastMemory;
        long memoryAfter = System.GC.GetTotalMemory(false);
        long memoryFreed = memoryBefore - memoryAfter;

        if (showGCInfo)
        {
            Debug.Log($"GC完成 - 耗时: {gcTime:F2}ms, 释放内存: {memoryFreed / (1024f * 1024f):F1} MB");
        }

        lastMemory = memoryAfter;
        OnGarbageCollection?.Invoke();
        OnMemoryFreed?.Invoke(memoryFreed / (1024f * 1024f));
    }

    /// <summary>
    /// 激进垃圾回收（更彻底但更慢）
    /// </summary>
    private void PerformAggressiveGC()
    {
        float startTime = Time.realtimeSinceStartup;

        // 多次GC以确保完全回收
        for (int i = 0; i < 3; i++)
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        float endTime = Time.realtimeSinceStartup;
        float gcTime = (endTime - startTime) * 1000f;

        totalGCCalls++;
        totalGCTime += gcTime;

        if (showGCInfo)
        {
            Debug.Log($"激进GC完成 - 耗时: {gcTime:F2}ms");
        }

        lastMemory = System.GC.GetTotalMemory(false);
        OnGarbageCollection?.Invoke();
    }

    /// <summary>
    /// 延迟垃圾回收
    /// </summary>
    public void DelayedGarbageCollection(float delay)
    {
        StartCoroutine(DelayedGCCoroutine(delay));
    }

    private IEnumerator DelayedGCCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        PerformGarbageCollection();
    }

    /// <summary>
    /// 在场景加载时执行GC
    /// </summary>
    public void OnSceneLoad()
    {
        // 清理之前场景的内存
        PerformGarbageCollection();

        // 卸载未使用的资源
        StartCoroutine(UnloadUnusedAssets());
    }

    private IEnumerator UnloadUnusedAssets()
    {
        yield return Resources.UnloadUnusedAssetsAsync();
        if (showGCInfo)
        {
            Debug.Log("已卸载未使用的资源");
        }
    }

    /// <summary>
    /// 获取GC统计信息
    /// </summary>
    public GCStats GetGCStats()
    {
        return new GCStats
        {
            totalCalls = totalGCCalls,
            totalTime = totalGCTime,
            averageTime = totalGCCalls > 0 ? totalGCTime / totalGCCalls : 0f,
            currentMemory = System.GC.GetTotalMemory(false) / (1024f * 1024f)
        };
    }

    /// <summary>
    /// 设置内存阈值
    /// </summary>
    public void SetMemoryThreshold(long threshold)
    {
        memoryThreshold = threshold;
    }

    /// <summary>
    /// 设置GC间隔
    /// </summary>
    public void SetGCInterval(float interval)
    {
        gcInterval = interval;
    }

    /// <summary>
    /// 启用/禁用自动GC
    /// </summary>
    public void SetAutoGC(bool enabled)
    {
        enableAutoGC = enabled;
    }

    /// <summary>
    /// 启用/禁用激进模式
    /// </summary>
    public void SetAggressiveMode(bool aggressive)
    {
        aggressiveMode = aggressive;
    }
}

[System.Serializable]
public class GCStats
{
    public int totalCalls;
    public float totalTime; // 毫秒
    public float averageTime; // 毫秒
    public float currentMemory; // MB
}
