using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 成就追踪数据
/// </summary>
[System.Serializable]
public class AchievementData
{
    public string achievementId;
    public bool isUnlocked;
    public float progress;
    public float targetValue;
    public string unlockTime;

    public AchievementData(string id, float target)
    {
        achievementId = id;
        isUnlocked = false;
        progress = 0f;
        targetValue = target;
        unlockTime = null;
    }

    public void UpdateProgress(float value)
    {
        progress = Mathf.Min(value, targetValue);
        if (progress >= targetValue && !isUnlocked)
        {
            Unlock();
        }
    }

    public void Unlock()
    {
        isUnlocked = true;
        unlockTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public float GetProgressPercentage()
    {
        return targetValue > 0 ? (progress / targetValue) * 100f : 0f;
    }
}

/// <summary>
/// 成就追踪器
/// </summary>
public class AchievementTracker : MonoBehaviour
{
    [Header("成就设置")]
    [SerializeField] private bool showNotification = true;
    [SerializeField] private float notificationDuration = 3f;

    // 成就数据
    private Dictionary<string, AchievementData> achievements = new Dictionary<string, AchievementData>();

    // 单例
    private static AchievementTracker _instance;
    public static AchievementTracker Instance => _instance;

    // 事件
    public event System.Action<AchievementData> OnAchievementUnlocked;
    public event System.Action<AchievementData> OnAchievementProgress;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
            LoadAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAchievements()
    {
        // 创建默认成就
        AddAchievement("first_run", 1f);
        AddAchievement("score_1000", 1000f);
        AddAchievement("score_5000", 5000f);
        AddAchievement("score_10000", 10000f);
        AddAchievement("distance_1000", 1000f);
        AddAchievement("distance_5000", 5000f);
        AddAchievement("coins_100", 100f);
        AddAchievement("coins_1000", 1000f);
        AddAchievement("runs_10", 10f);
        AddAchievement("runs_100", 100f);
    }

    private void AddAchievement(string id, float target)
    {
        if (!achievements.ContainsKey(id))
        {
            achievements[id] = new AchievementData(id, target);
        }
    }

    #region 成就更新

    public void UpdateScoreAchievement(int score)
    {
        UpdateAchievement("score_1000", score);
        UpdateAchievement("score_5000", score);
        UpdateAchievement("score_10000", score);
    }

    public void UpdateDistanceAchievement(float distance)
    {
        UpdateAchievement("distance_1000", distance);
        UpdateAchievement("distance_5000", distance);
    }

    public void UpdateCoinsAchievement(int coins)
    {
        UpdateAchievement("coins_100", coins);
        UpdateAchievement("coins_1000", coins);
    }

    public void UpdateRunsAchievement(int runs)
    {
        UpdateAchievement("runs_10", runs);
        UpdateAchievement("runs_100", runs);
    }

    public void UpdateAchievement(string id, float value)
    {
        if (achievements.ContainsKey(id))
        {
            AchievementData achievement = achievements[id];

            if (!achievement.isUnlocked)
            {
                achievement.UpdateProgress(value);
                OnAchievementProgress?.Invoke(achievement);
                SaveAchievements();

                if (achievement.isUnlocked)
                {
                    OnAchievementUnlocked?.Invoke(achievement);
                    if (showNotification)
                    {
                        ShowAchievementNotification(achievement);
                    }
                }
            }
        }
    }

    #endregion

    #region 成就查询

    public bool IsAchievementUnlocked(string id)
    {
        if (achievements.ContainsKey(id))
        {
            return achievements[id].isUnlocked;
        }
        return false;
    }

    public float GetAchievementProgress(string id)
    {
        if (achievements.ContainsKey(id))
        {
            return achievements[id].progress;
        }
        return 0f;
    }

    public float GetAchievementProgressPercentage(string id)
    {
        if (achievements.ContainsKey(id))
        {
            return achievements[id].GetProgressPercentage();
        }
        return 0f;
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var achievement in achievements.Values)
        {
            if (achievement.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }

    public int GetTotalCount()
    {
        return achievements.Count;
    }

    public List<AchievementData> GetAllAchievements()
    {
        return new List<AchievementData>(achievements.Values);
    }

    public List<AchievementData> GetUnlockedAchievements()
    {
        List<AchievementData> unlocked = new List<AchievementData>();
        foreach (var achievement in achievements.Values)
        {
            if (achievement.isUnlocked)
            {
                unlocked.Add(achievement);
            }
        }
        return unlocked;
    }

    #endregion

    #region 保存和加载

    private void SaveAchievements()
    {
        try
        {
            AchievementSaveData saveData = new AchievementSaveData();
            saveData.achievements = new List<AchievementData>(achievements.Values);

            string jsonData = JsonUtility.ToJson(saveData, true);
            string filePath = GetAchievementFilePath();
            System.IO.File.WriteAllText(filePath, jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存成就失败: {e.Message}");
        }
    }

    private void LoadAchievements()
    {
        try
        {
            string filePath = GetAchievementFilePath();
            if (System.IO.File.Exists(filePath))
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                AchievementSaveData saveData = JsonUtility.FromJson<AchievementSaveData>(jsonData);

                if (saveData != null && saveData.achievements != null)
                {
                    achievements.Clear();
                    foreach (var achievement in saveData.achievements)
                    {
                        achievements[achievement.achievementId] = achievement;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载成就失败: {e.Message}");
        }
    }

    private string GetAchievementFilePath()
    {
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        return System.IO.Path.Combine(directory, "achievements.dat");
    }

    #endregion

    #region 重置

    public void ResetAllAchievements()
    {
        foreach (var achievement in achievements.Values)
        {
            achievement.isUnlocked = false;
            achievement.progress = 0f;
            achievement.unlockTime = null;
        }
        SaveAchievements();
    }

    public void ResetAchievement(string id)
    {
        if (achievements.ContainsKey(id))
        {
            achievements[id].isUnlocked = false;
            achievements[id].progress = 0f;
            achievements[id].unlockTime = null;
            SaveAchievements();
        }
    }

    #endregion

    #region 通知

    private void ShowAchievementNotification(AchievementData achievement)
    {
        Debug.Log($"🏆 成就解锁: {achievement.achievementId}!");
        // TODO: 显示UI通知
    }

    #endregion
}

[System.Serializable]
public class AchievementSaveData
{
    public List<AchievementData> achievements;
}
