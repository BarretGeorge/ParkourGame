using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 挑战任务数据
/// </summary>
[System.Serializable]
public class ChallengeData
{
    public string challengeId;
    public string challengeName;
    public string description;
    public ChallengeType type;
    public List<ChallengeCondition> conditions;
    public List<ChallengeReward> rewards;
    public bool isActive;
    public bool isCompleted;
    public System.DateTime startTime;
    public System.DateTime endTime;

    public bool IsExpired()
    {
        return System.DateTime.Now > endTime;
    }

    public bool IsUpcoming()
    {
        return System.DateTime.Now < startTime;
    }
}

[System.Serializable]
public class ChallengeCondition
{
    public string statName;
    public int targetValue;
}

[System.Serializable]
public class ChallengeReward
{
    public RewardType type;
    public int amount;
    public string itemId; // 角色ID、皮肤ID等
}

public enum ChallengeType
{
    Weekly,         // 周挑战
    Weekend,        // 周末挑战
    Event,          // 活动挑战
    Community       // 社区挑战
}

public enum RewardType
{
    Coins,
    Experience,
    Character,
    Skin,
    Title
}

/// <summary>
/// 挑战任务管理器
/// </summary>
public class ChallengeManager : MonoBehaviour
{
    [Header("挑战设置")]
    [SerializeField] private int maxActiveChallenges = 5;

    // 挑战列表
    private List<ChallengeData> challenges = new List<ChallengeData>();

    // 单例
    private static ChallengeManager _instance;
    public static ChallengeManager Instance => _instance;

    // 事件
    public event System.Action<ChallengeData> OnChallengeCompleted;
    public event System.Action OnChallengeRefreshed;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadChallenges();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CheckChallengeExpiry();
    }

    private void Update()
    {
        // 每分钟检查一次挑战过期
        if (Time.frameCount % 3600 == 0)
        {
            CheckChallengeExpiry();
        }
    }

    private void CheckChallengeExpiry()
    {
        bool removedExpired = false;

        for (int i = challenges.Count - 1; i >= 0; i--)
        {
            if (challenges[i].IsExpired())
            {
                challenges.RemoveAt(i);
                removedExpired = true;
            }
        }

        if (removedExpired)
        {
            SaveChallenges();
        }
    }

    #region 挑战管理

    public void AddChallenge(ChallengeData challenge)
    {
        if (challenges.Count < maxActiveChallenges)
        {
            challenges.Add(challenge);
            SaveChallenges();
        }
    }

    public void CreateWeeklyChallenge(string name, string description, Dictionary<string, int> conditions, List<ChallengeReward> rewards)
    {
        ChallengeData challenge = new ChallengeData();
        challenge.challengeId = $"weekly_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        challenge.challengeName = name;
        challenge.description = description;
        challenge.type = ChallengeType.Weekly;
        challenge.isActive = true;
        challenge.isCompleted = false;

        // 本周一
        System.DateTime today = System.DateTime.Now;
        int daysFromMonday = ((int)today.DayOfWeek - (int)System.DayOfWeek.Monday + 7) % 7;
        challenge.startTime = today.AddDays(-daysFromMonday).Date;
        challenge.endTime = challenge.startTime.AddDays(7);

        // 转换条件
        challenge.conditions = new List<ChallengeCondition>();
        foreach (var condition in conditions)
        {
            challenge.conditions.Add(new ChallengeCondition
            {
                statName = condition.Key,
                targetValue = condition.Value
            });
        }

        challenge.rewards = rewards;

        AddChallenge(challenge);
    }

    public void UpdateChallengeProgress(string statName, int value)
    {
        foreach (var challenge in challenges)
        {
            if (challenge.isActive && !challenge.isCompleted)
            {
                bool allConditionsMet = true;

                foreach (var condition in challenge.conditions)
                {
                    if (condition.statName == statName)
                    {
                        // 这个条件已满足
                    }

                    // 检查是否所有条件都满足
                    int currentValue = StatTracker.Instance?.GetCumulativeStat(condition.statName) ?? 0;
                    if (currentValue < condition.targetValue)
                    {
                        allConditionsMet = false;
                    }
                }

                if (allConditionsMet)
                {
                    CompleteChallenge(challenge);
                }
            }
        }
    }

    private void CompleteChallenge(ChallengeData challenge)
    {
        challenge.isCompleted = true;
        OnChallengeCompleted?.Invoke(challenge);

        // 发放奖励
        GrantChallengeRewards(challenge);

        Debug.Log($"挑战完成: {challenge.challengeName}");
        SaveChallenges();
    }

    private void GrantChallengeRewards(ChallengeData challenge)
    {
        foreach (var reward in challenge.rewards)
        {
            switch (reward.type)
            {
                case RewardType.Coins:
                    // TODO: 添加金币
                    Debug.Log($"挑战奖励: {reward.amount} 金币");
                    break;

                case RewardType.Experience:
                    // TODO: 添加经验
                    Debug.Log($"挑战奖励: {reward.amount} 经验");
                    break;

                case RewardType.Character:
                    // TODO: 解锁角色
                    Debug.Log($"挑战奖励: 解锁角色 {reward.itemId}");
                    break;

                case RewardType.Skin:
                    // TODO: 解锁皮肤
                    Debug.Log($"挑战奖励: 解锁皮肤 {reward.itemId}");
                    break;

                case RewardType.Title:
                    // TODO: 解锁称号
                    Debug.Log($"挑战奖励: 解锁称号 {reward.itemId}");
                    break;
            }
        }
    }

    #endregion

    #region 挑战查询

    public List<ChallengeData> GetActiveChallenges()
    {
        List<ChallengeData> active = new List<ChallengeData>();
        foreach (var challenge in challenges)
        {
            if (challenge.isActive && !challenge.isCompleted && !challenge.IsExpired() && !challenge.IsUpcoming())
            {
                active.Add(challenge);
            }
        }
        return active;
    }

    public List<ChallengeData> GetCompletedChallenges()
    {
        List<ChallengeData> completed = new List<ChallengeData>();
        foreach (var challenge in challenges)
        {
            if (challenge.isCompleted)
            {
                completed.Add(challenge);
            }
        }
        return completed;
    }

    public List<ChallengeData> GetChallengesByType(ChallengeType type)
    {
        List<ChallengeData> result = new List<ChallengeData>();
        foreach (var challenge in challenges)
        {
            if (challenge.type == type)
            {
                result.Add(challenge);
            }
        }
        return result;
    }

    #endregion

    #region 保存和加载

    private void SaveChallenges()
    {
        try
        {
            ChallengeSaveData saveData = new ChallengeSaveData();
            saveData.challenges = challenges;

            string jsonData = JsonUtility.ToJson(saveData, true);
            string filePath = GetChallengeFilePath();
            System.IO.File.WriteAllText(filePath, jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存挑战失败: {e.Message}");
        }
    }

    private void LoadChallenges()
    {
        try
        {
            string filePath = GetChallengeFilePath();
            if (System.IO.File.Exists(filePath))
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                ChallengeSaveData saveData = JsonUtility.FromJson<ChallengeSaveData>(jsonData);

                if (saveData != null && saveData.challenges != null)
                {
                    challenges = saveData.challenges;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载挑战失败: {e.Message}");
        }
    }

    private string GetChallengeFilePath()
    {
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        return System.IO.Path.Combine(directory, "challenges.dat");
    }

    #endregion
}

[System.Serializable]
public class ChallengeSaveData
{
    public List<ChallengeData> challenges;
}
