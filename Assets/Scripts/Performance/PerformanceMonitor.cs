using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 性能监控器 - 实时监控游戏性能
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("监控设置")]
    [SerializeField] private bool showFPS = true;
    [SerializeField] private bool showMemory = true;
    [SerializeField] private bool showRenderStats = true;
    [SerializeField] private float updateInterval = 0.5f;

    [Header("警告阈值")]
    [SerializeField] private int fpsWarningThreshold = 30;
    [SerializeField] private long memoryWarningThreshold = 500 * 1024 * 1024; // 500MB

    [Header("UI设置")]
    [SerializeField] private Vector2 position = new Vector2(10, 10);
    [SerializeField private int fontSize = 16;

    // 性能数据
    private float currentFPS;
    private float currentMemoryMB;
    private int drawCalls;
    private int triangles;

    // FPS计算
    private float fpsAccumulator;
    private int fpsFrames;
    private float fpsTimer;

    // 历史数据
    private Queue<float> fpsHistory = new Queue<float>();
    private int maxHistorySize = 60;

    // 单例
    private static PerformanceMonitor _instance;
    public static PerformanceMonitor Instance => _instance;

    // 事件
    public event System.Action<float> OnFPSChanged;
    public event System.Action<float> OnMemoryWarning;
    public event System.Action<float> OnFPSWarning;

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
        // 计算FPS
        fpsAccumulator += Time.timeScale / Time.deltaTime;
        fpsFrames++;

        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= updateInterval)
        {
            currentFPS = fpsAccumulator / fpsFrames;

            // 更新历史
            fpsHistory.Enqueue(currentFPS);
            if (fpsHistory.Count > maxHistorySize)
            {
                fpsHistory.Dequeue();
            }

            // 触发事件
            OnFPSChanged?.Invoke(currentFPS);

            // 检查警告
            if (currentFPS < fpsWarningThreshold)
            {
                OnFPSWarning?.Invoke(currentFPS);
            }

            fpsAccumulator = 0;
            fpsFrames = 0;
            fpsTimer = 0f;
        }

        // 更新内存
        currentMemoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);

        if (currentMemoryMB * 1024 * 1024 > memoryWarningThreshold)
        {
            OnMemoryWarning?.Invoke(currentMemoryMB);
        }

        // 更新渲染统计
        UpdateRenderStats();
    }

    private void UpdateRenderStats()
    {
        drawCalls = UnityEngine.Statistics.DrawCalls;
        triangles = UnityEngine.Statistics.Triangles;
    }

    private void OnGUI()
    {
        if (!showFPS && !showMemory && !showRenderStats) return;

        GUILayout.BeginArea(new Rect(position.x, position.y, 300, 400));
        GUILayout.BeginVertical("Box");

        GUI.fontSize = fontSize;

        if (showFPS)
        {
            DisplayFPS();
        }

        if (showMemory)
        {
            DisplayMemory();
        }

        if (showRenderStats)
        {
            DisplayRenderStats();
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DisplayFPS()
    {
        Color color = Color.white;

        if (currentFPS < fpsWarningThreshold)
        {
            color = Color.red;
        }
        else if (currentFPS < fpsWarningThreshold * 1.5f)
        {
            color = Color.yellow;
        }

        GUI.color = color;
        GUILayout.Label($"FPS: {currentFPS:F1}");
        GUI.color = Color.white;

        // 显示平均FPS
        if (fpsHistory.Count > 0)
        {
            float avgFPS = 0f;
            foreach (float fps in fpsHistory)
            {
                avgFPS += fps;
            }
            avgFPS /= fpsHistory.Count;

            GUILayout.Label($"平均FPS: {avgFPS:F1}");
        }

        // 显示最小/最大FPS
        if (fpsHistory.Count > 0)
        {
            float minFPS = float.MaxValue;
            float maxFPS = float.MinValue;

            foreach (float fps in fpsHistory)
            {
                minFPS = Mathf.Min(minFPS, fps);
                maxFPS = Mathf.Max(maxFPS, fps);
            }

            GUILayout.Label($"最小FPS: {minFPS:F1}, 最大FPS: {maxFPS:F1}");
        }
    }

    private void DisplayMemory()
    {
        GUILayout.Label($"内存: {currentMemoryMB:F1} MB");

        // 获取更多内存信息
        long totalMemory = System.GC.GetTotalMemory(false);
        GUILayout.Label($"总内存: {totalMemory / (1024f * 1024f):F1} MB");

        // GC次数
        int gen0 = System.GC.CollectionCount(0);
        int gen1 = System.GC.CollectionCount(1);
        int gen2 = System.GC.CollectionCount(2);

        GUILayout.Label($"GC Gen0: {gen0}, Gen1: {gen1}, Gen2: {gen2}");
    }

    private void DisplayRenderStats()
    {
        GUILayout.Label($"Draw Calls: {drawCalls}");
        GUILayout.Label($"Triangles: {triangles}");
    }

    /// <summary>
    /// 获取当前FPS
    /// </summary>
    public float GetFPS()
    {
        return currentFPS;
    }

    /// <summary>
    /// 获取平均FPS
    /// </summary>
    public float GetAverageFPS()
    {
        if (fpsHistory.Count == 0) return 0f;

        float sum = 0f;
        foreach (float fps in fpsHistory)
        {
            sum += fps;
        }
        return sum / fpsHistory.Count;
    }

    /// <summary>
    /// 获取内存使用量（MB）
    /// </summary>
    public float GetMemoryUsageMB()
    {
        return currentMemoryMB;
    }

    /// <summary>
    /// 获取Draw Calls数量
    /// </summary>
    public int GetDrawCalls()
    {
        return drawCalls;
    }

    /// <summary>
    /// 获取三角形数量
    /// </summary>
    public int GetTriangles()
    {
        return triangles;
    }

    /// <summary>
    /// 强制垃圾回收
    /// </summary>
    public void ForceGarbageCollection()
    {
        Debug.Log("执行垃圾回收...");
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        Debug.Log("垃圾回收完成");
    }

    /// <summary>
    /// 获取性能报告
    /// </summary>
    public string GetPerformanceReport()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("=== 性能报告 ===");
        sb.AppendLine($"FPS: {currentFPS:F1}");
        sb.AppendLine($"平均FPS: {GetAverageFPS():F1}");
        sb.AppendLine($"内存: {currentMemoryMB:F1} MB");
        sb.AppendLine($"Draw Calls: {drawCalls}");
        sb.AppendLine($"Triangles: {triangles}");

        return sb.ToString();
    }

    /// <summary>
    /// 设置显示位置
    /// </summary>
    public void SetDisplayPosition(Vector2 pos)
    {
        position = pos;
    }

    /// <summary>
    /// 设置显示选项
    /// </summary>
    public void SetDisplayOptions(bool showFPS, bool showMemory, bool showRenderStats)
    {
        this.showFPS = showFPS;
        this.showMemory = showMemory;
        this.showRenderStats = showRenderStats;
    }
}
