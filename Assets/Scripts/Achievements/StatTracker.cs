using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 游戏统计数据追踪器 - 用于成就和任务系统
/// </summary>
public class StatTracker : MonoBehaviour
{
    [Header("当前游戏统计")]
    private int currentScore;
    private int currentCoins;
    private float currentDistance;
    private float currentSurvivalTime;
    private int currentCombo;
    private int currentPerfectRuns;

    // 收集品统计
    private Dictionary<CollectibleType, int> currentCollectibles = new Dictionary<CollectibleType, int>();
    private Dictionary<PowerUpType, int> currentPowerUps = new Dictionary<PowerUpType, int>();

    // 累计统计（跨游戏）
    private Dictionary<string, int> cumulativeStats = new Dictionary<string, int>();

    // 单例
    private static StatTracker _instance;
    public static StatTracker Instance => _instance;

    // 事件
    public event System.Action<string, int> OnStatChanged;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStats();
            LoadCumulativeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStats()
    {
        // 初始化收集品字典
        foreach (CollectibleType type in System.Enum.GetValues(typeof(CollectibleType)))
        {
            currentCollectibles[type] = 0;
        }

        // 初始化道具备字典
        foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
        {
            currentPowerUps[type] = 0;
        }
    }

    private void LoadCumulativeStats()
    {
        // 从SaveManager加载累计统计
        if (SaveManager.Instance != null)
        {
            SaveData saveData = SaveManager.Instance.GetSaveData();
            cumulativeStats["total_runs"] = saveData.totalRuns;
            cumulativeStats["total_coins"] = saveData.totalCoins;
            cumulativeStats["high_score"] = saveData.highScore;
        }
    }

    #region 当前游戏统计

    public void ResetCurrentGameStats()
    {
        currentScore = 0;
        currentCoins = 0;
        currentDistance = 0f;
        currentSurvivalTime = 0f;
        currentCombo = 0;
        currentPerfectRuns = 0;

        foreach (CollectibleType type in currentCollectibles.Keys)
        {
            currentCollectibles[type] = 0;
        }

        foreach (PowerUpType type in currentPowerUps.Keys)
        {
            currentPowerUps[type] = 0;
        }
    }

    public void UpdateScore(int score)
    {
        currentScore = score;
        NotifyStatChanged("score", currentScore);

        // 更新成就
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UpdateAchievement("score_1000", currentScore);
            AchievementManager.Instance.UpdateAchievement("score_5000", currentScore);
            AchievementManager.Instance.UpdateAchievement("score_10000", currentScore);
        }

        // 更新每日任务
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UpdateQuestProgress(QuestType.ReachScore, currentScore);
        }
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        NotifyStatChanged("coins", currentCoins);

        // 更新成就
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UpdateAchievement("coins_100", currentCoins);
            AchievementManager.Instance.UpdateAchievement("coins_1000", currentCoins);
        }

        // 更新每日任务
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UpdateQuestProgress(QuestType.CollectCoins, currentCoins);
        }
    }

    public void UpdateDistance(float distance)
    {
        currentDistance = distance;
        NotifyStatChanged("distance", (int)currentDistance);

        // 更新成就
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UpdateAchievement("distance_1000", currentDistance);
            AchievementManager.Instance.UpdateAchievement("distance_5000", currentDistance);
        }

        // 更新每日任务
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UpdateQuestProgress(QuestType.RunDistance, (int)currentDistance);
        }
    }

    public void UpdateSurvivalTime(float time)
    {
        currentSurvivalTime = time;
        NotifyStatChanged("survival_time", (int)currentSurvivalTime);

        // 更新每日任务
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UpdateQuestProgress(QuestType.SurvivalTime, (int)currentSurvivalTime);
        }
    }

    public void UpdateCombo(int combo)
    {
        currentCombo = combo;
        NotifyStatChanged("combo", currentCombo);

        // 更新成就
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UpdateAchievement("combo_10", currentCombo);
            AchievementManager.Instance.UpdateAchievement("combo_20", currentCombo);
        }

        // 更新每日任务
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UpdateQuestProgress(QuestType.ComboReach, currentCombo);
        }
    }

    public void CollectItem(CollectibleType type)
    {
        if (currentCollectibles.ContainsKey(type))
        {
            currentCollectibles[type]++;
            NotifyStatChanged(type.ToString(), currentCollectibles[type]);
        }
    }

    public void UsePowerUp(PowerUpType type)
    {
        if (currentPowerUps.ContainsKey(type))
        {
            currentPowerUps[type]++;
            NotifyStatChanged($"powerup_{type}", currentPowerUps[type]);

            // 更新每日任务
            if (DailyQuestManager.Instance != null)
            {
                int totalPowerUps = 0;
                foreach (var count in currentPowerUps.Values)
                {
                    totalPowerUps += count;
                }
                DailyQuestManager.Instance.UpdateQuestProgress(QuestType.UsePowerUp, totalPowerUps);
            }
        }
    }

    public void RecordPerfectRun()
    {
        currentPerfectRuns++;
        NotifyStatChanged("perfect_runs", currentPerfectRuns);

        // 更新每日任务
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UpdateQuestProgress(QuestType.PerfectRun, currentPerfectRuns);
        }
    }

    #endregion

    #region 累计统计

    public void IncrementRunCount()
    {
        if (cumulativeStats.ContainsKey("total_runs"))
        {
            cumulativeStats["total_runs"]++;
        }
        else
        {
            cumulativeStats["total_runs"] = 1;
        }

        // 更新成就
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UpdateAchievement("runs_10", cumulativeStats["total_runs"]);
            AchievementManager.Instance.UpdateAchievement("runs_100", cumulativeStats["total_runs"]);
        }

        // 更新每日任务
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UpdateQuestProgress(QuestType.PlayRounds, cumulativeStats["total_runs"]);
        }
    }

    public int GetCumulativeStat(string statName)
    {
        if (cumulativeStats.ContainsKey(statName))
        {
            return cumulativeStats[statName];
        }
        return 0;
    }

    public void SetCumulativeStat(string statName, int value)
    {
        cumulativeStats[statName] = value;
        NotifyStatChanged(statName, value);
    }

    #endregion

    #region Getter

    public int CurrentScore => currentScore;
    public int CurrentCoins => currentCoins;
    public float CurrentDistance => currentDistance;
    public float CurrentSurvivalTime => currentSurvivalTime;
    public int CurrentCombo => currentCombo;
    public int CurrentPerfectRuns => currentPerfectRuns;

    public int GetCollectibleCount(CollectibleType type)
    {
        return currentCollectibles.ContainsKey(type) ? currentCollectibles[type] : 0;
    }

    public int GetPowerUpCount(PowerUpType type)
    {
        return currentPowerUps.ContainsKey(type) ? currentPowerUps[type] : 0;
    }

    #endregion

    private void NotifyStatChanged(string statName, int value)
    {
        OnStatChanged?.Invoke(statName, value);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCumulativeStats();
        }
    }

    private void OnApplicationQuit()
    {
        SaveCumulativeStats();
    }

    private void SaveCumulativeStats()
    {
        // 将累计统计保存到SaveManager
        if (SaveManager.Instance != null)
        {
            // SaveManager会自动保存
        }
    }
}
