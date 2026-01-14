using UnityEngine;

/// <summary>
/// 难度管理器 - 管理游戏难度变化
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private LevelConfig levelConfig;

    [Header("当前难度")]
    [SerializeField] private float currentDifficulty = 0f;

    [SerializeField] private float currentDistance = 0f;

    // 统计数据
    public float CurrentDifficulty => currentDifficulty;
    public float CurrentDistance => currentDistance;
    public int CurrentLevel => Mathf.FloorToInt(currentDistance / 500f) + 1; // 每500米一级

    // 事件
    public System.Action<float> OnDifficultyChanged;
    public System.Action<int> OnLevelUp;

    private float lastDifficultyUpdateDistance = 0f;
    private const float DIFFICULTY_UPDATE_INTERVAL = 10f; // 每10米更新一次难度

    private void Update()
    {
        if (levelConfig == null) return;

        // 更新距离（从玩家获取）
        UpdateDistance();

        // 更新难度
        UpdateDifficulty();
    }

    /// <summary>
    /// 更新距离
    /// </summary>
    private void UpdateDistance()
    {
        // 这里可以从PlayerController获取实际距离
        // 暂时使用模拟数据
        if (Application.isPlaying)
        {
            currentDistance += Time.deltaTime * 10f; // 假设10m/s
        }
    }

    /// <summary>
    /// 设置实际距离（从PlayerController调用）
    /// </summary>
    public void SetDistance(float distance)
    {
        currentDistance = distance;
    }

    /// <summary>
    /// 更新难度
    /// </summary>
    private void UpdateDifficulty()
    {
        // 检查是否需要更新
        if (currentDistance - lastDifficultyUpdateDistance < DIFFICULTY_UPDATE_INTERVAL)
        {
            return;
        }

        lastDifficultyUpdateDistance = currentDistance;

        // 计算新难度
        float newDifficulty = levelConfig.GetDifficultyAtDistance(currentDistance);

        // 检查难度是否变化
        if (Mathf.Abs(newDifficulty - currentDifficulty) > 0.01f)
        {
            float oldDifficulty = currentDifficulty;
            currentDifficulty = newDifficulty;

            // 触发事件
            OnDifficultyChanged?.Invoke(currentDifficulty);

            // 检查是否升级
            CheckLevelUp();

            Debug.Log($"[DifficultyManager] Difficulty updated: {oldDifficulty:F2} -> {currentDifficulty:F2} at {currentDistance:F0}m");
        }
    }

    /// <summary>
    /// 检查是否升级
    /// </summary>
    private void CheckLevelUp()
    {
        int newLevel = CurrentLevel;
        int previousLevel = Mathf.FloorToInt((currentDistance - DIFFICULTY_UPDATE_INTERVAL) / 500f) + 1;

        if (newLevel > previousLevel)
        {
            OnLevelUp?.Invoke(newLevel);
            Debug.Log($"[DifficultyManager] Level Up! Now at Level {newLevel}");
        }
    }

    /// <summary>
    /// 重置难度
    /// </summary>
    public void ResetDifficulty()
    {
        currentDifficulty = levelConfig?.initialDifficulty ?? 0f;
        currentDistance = 0f;
        lastDifficultyUpdateDistance = 0f;

        OnDifficultyChanged?.Invoke(currentDifficulty);
    }

    /// <summary>
    /// 设置难度（用于测试）
    /// </summary>
    public void SetDifficulty(float difficulty)
    {
        currentDifficulty = Mathf.Clamp(difficulty, 0f, 1f);
        OnDifficultyChanged?.Invoke(currentDifficulty);
    }

    /// <summary>
    /// 获取指定距离的难度
    /// </summary>
    public float GetDifficultyAtDistance(float distance)
    {
        if (levelConfig == null) return 0f;
        return levelConfig.GetDifficultyAtDistance(distance);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 调试显示
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 750, 250, 150));
        GUILayout.Box("Difficulty Manager");

        GUILayout.Label($"Distance: {currentDistance:F0}m");
        GUILayout.Label($"Level: {CurrentLevel}");
        GUILayout.Label($"Difficulty: {currentDifficulty:P0}");

        GUILayout.EndArea();
    }
#endif
}
