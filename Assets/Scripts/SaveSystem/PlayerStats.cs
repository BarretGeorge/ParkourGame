using UnityEngine;

/// <summary>
/// 玩家统计追踪器
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("当前运行统计")]
    [SerializeField] private int currentRunScore;
    [SerializeField] private int currentRunCoins;
    [SerializeField] private float currentRunDistance;
    [SerializeField] private float currentRunTime;

    [Header("累计统计")]
    [SerializeField] private int lifetimeCoins;
    [SerializeField] private float lifetimeDistance;
    [SerializeField] private int lifetimeRuns;

    // 引用
    private PlayerController playerController;

    // 事件
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnCoinsChanged;
    public event System.Action<float> OnDistanceChanged;

    private void Start()
    {
        FindPlayer();
        LoadLifetimeStats();
    }

    private void FindPlayer()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void LoadLifetimeStats()
    {
        if (SaveManager.Instance != null)
        {
            SaveData saveData = SaveManager.Instance.GetSaveData();
            lifetimeCoins = saveData.totalCoins;
            lifetimeDistance = saveData.totalDistance;
            lifetimeRuns = saveData.totalRuns;
        }
    }

    private void Update()
    {
        if (playerController == null) return;

        // 更新当前运行统计
        currentRunScore = (int)playerController.Score;
        currentRunCoins = playerController.CoinsCollected;
        currentRunDistance = playerController.DistanceTraveled;
        currentRunTime += Time.deltaTime;
    }

    public void UpdateRunStats()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UpdateRunStats(
                currentRunScore,
                currentRunCoins,
                currentRunDistance
            );

            // 更新累计统计
            lifetimeCoins += currentRunCoins;
            lifetimeDistance += currentRunDistance;
            lifetimeRuns++;
        }
    }

    public void ResetCurrentRun()
    {
        currentRunScore = 0;
        currentRunCoins = 0;
        currentRunDistance = 0f;
        currentRunTime = 0f;
    }

    #region Getter

    public int CurrentRunScore => currentRunScore;
    public int CurrentRunCoins => currentRunCoins;
    public float CurrentRunDistance => currentRunDistance;
    public float CurrentRunTime => currentRunTime;

    public int LifetimeCoins => lifetimeCoins;
    public float LifetimeDistance => lifetimeDistance;
    public int LifetimeRuns => lifetimeRuns;

    #endregion

    #region 统计计算

    public float GetAverageScore()
    {
        if (lifetimeRuns == 0) return 0f;
        SaveData saveData = SaveManager.Instance?.GetSaveData();
        if (saveData != null)
        {
            return (float)saveData.highScore / lifetimeRuns;
        }
        return 0f;
    }

    public float GetAverageDistance()
    {
        if (lifetimeRuns == 0) return 0f;
        return lifetimeDistance / lifetimeRuns;
    }

    public float GetAverageCoins()
    {
        if (lifetimeRuns == 0) return 0f;
        return (float)lifetimeCoins / lifetimeRuns;
    }

    public string GetFormattedPlaytime()
    {
        int totalSeconds = SaveManager.Instance?.GetSaveData()?.totalPlayTime ?? 0;

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (hours > 0)
        {
            return $"{hours}h {minutes}m {seconds}s";
        }
        else if (minutes > 0)
        {
            return $"{minutes}m {seconds}s";
        }
        else
        {
            return $"{seconds}s";
        }
    }

    #endregion
}
