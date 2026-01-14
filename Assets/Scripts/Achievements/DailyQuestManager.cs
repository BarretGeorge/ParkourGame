using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 每日任务数据
/// </summary>
[System.Serializable]
public class DailyQuestData
{
    public string questId;
    public string questName;
    public string description;
    public QuestType type;
    public int targetValue;
    public int currentValue;
    public int coinReward;
    public int expReward;
    public bool isCompleted;
    public bool isClaimed;
    public string questDate; // YYYY-MM-DD格式

    public float Progress => targetValue > 0 ? (float)currentValue / targetValue : 0f;

    public void UpdateProgress(int value)
    {
        if (!isCompleted)
        {
            currentValue = Mathf.Min(value, targetValue);
            if (currentValue >= targetValue)
            {
                isCompleted = true;
            }
        }
    }

    public bool IsExpired()
    {
        if (string.IsNullOrEmpty(questDate)) return true;

        DateTime questDateTime = DateTime.ParseExact(questDate, "yyyy-MM-dd", null);
        DateTime today = DateTime.Now.Date;
        return questDateTime < today;
    }
}

public enum QuestType
{
    RunDistance,        // 跑X米
    CollectCoins,       // 收集X金币
    ReachScore,         // 达到X分数
    UsePowerUp,         // 使用X次道具
    PerfectRun,         // 完美通关（无碰撞）
    PlayRounds,         // 完成X局游戏
    CollectSpecific,    // 收集特定物品X个
    ComboReach,         // 达到X连击
    SurvivalTime        // 生存X秒
}

/// <summary>
/// 每日任务管理器
/// </summary>
public class DailyQuestManager : MonoBehaviour
{
    [Header("任务设置")]
    [SerializeField] private int dailyQuestCount = 3;
    [SerializeField] private int questRefreshHour = 0; // 每天几点刷新

    // 当前任务列表
    private List<DailyQuestData> dailyQuests = new List<DailyQuestData>();

    // 单例
    private static DailyQuestManager _instance;
    public static DailyQuestManager Instance => _instance;

    // 事件
    public event Action<DailyQuestData> OnQuestCompleted;
    public event Action<DailyQuestData> OnQuestClaimed;
    public event Action OnQuestsRefreshed;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDailyQuests();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CheckAndRefreshQuests();
    }

    private void Update()
    {
        // 检查是否需要刷新
        if (ShouldRefreshQuests())
        {
            RefreshDailyQuests();
        }
    }

    private bool ShouldRefreshQuests()
    {
        foreach (var quest in dailyQuests)
        {
            if (quest.IsExpired())
            {
                return true;
            }
        }
        return false;
    }

    private void CheckAndRefreshQuests()
    {
        if (dailyQuests.Count == 0 || ShouldRefreshQuests())
        {
            RefreshDailyQuests();
        }
    }

    #region 任务刷新

    public void RefreshDailyQuests()
    {
        dailyQuests.Clear();

        // 生成新的每日任务
        for (int i = 0; i < dailyQuestCount; i++)
        {
            DailyQuestData quest = GenerateRandomQuest(i);
            dailyQuests.Add(quest);
        }

        SaveDailyQuests();
        OnQuestsRefreshed?.Invoke();

        Debug.Log("每日任务已刷新");
    }

    private DailyQuestData GenerateRandomQuest(int index)
    {
        DailyQuestData quest = new DailyQuestData();
        quest.questId = $"daily_{DateTime.Now:yyyyMMdd}_{index}";
        quest.questDate = DateTime.Now.ToString("yyyy-MM-dd");
        quest.isCompleted = false;
        quest.isClaimed = false;
        quest.currentValue = 0;

        // 随机选择任务类型
        Array questTypes = Enum.GetValues(typeof(QuestType));
        QuestType randomType = (QuestType)questTypes.GetValue(UnityEngine.Random.Range(0, questTypes.Length));

        quest.type = randomType;

        // 根据类型设置任务参数
        switch (randomType)
        {
            case QuestType.RunDistance:
                quest.questName = "跑者马拉松";
                quest.description = "在一次游戏中跑1000米";
                quest.targetValue = 1000;
                quest.coinReward = 100;
                quest.expReward = 50;
                break;

            case QuestType.CollectCoins:
                quest.questName = "金币猎人";
                quest.description = "收集100个金币";
                quest.targetValue = 100;
                quest.coinReward = 150;
                quest.expReward = 75;
                break;

            case QuestType.ReachScore:
                quest.questName = "高分挑战";
                quest.description = "达到5000分";
                quest.targetValue = 5000;
                quest.coinReward = 200;
                quest.expReward = 100;
                break;

            case QuestType.UsePowerUp:
                quest.questName = "道具大师";
                quest.description = "使用5次道具";
                quest.targetValue = 5;
                quest.coinReward = 120;
                quest.expReward = 60;
                break;

            case QuestType.PerfectRun:
                quest.questName = "完美表现";
                quest.description = "完成一次无碰撞的游戏";
                quest.targetValue = 1;
                quest.coinReward = 300;
                quest.expReward = 150;
                break;

            case QuestType.PlayRounds:
                quest.questName = "坚持不懈";
                quest.description = "完成3局游戏";
                quest.targetValue = 3;
                quest.coinReward = 80;
                quest.expReward = 40;
                break;

            case QuestType.ComboReach:
                quest.questName = "连击达人";
                quest.description = "达到20连击";
                quest.targetValue = 20;
                quest.coinReward = 180;
                quest.expReward = 90;
                break;

            case QuestType.SurvivalTime:
                quest.questName = "生存专家";
                quest.description = "单局生存60秒";
                quest.targetValue = 60;
                quest.coinReward = 130;
                quest.expReward = 65;
                break;

            default:
                quest.questName = "每日任务";
                quest.description = "完成目标";
                quest.targetValue = 10;
                quest.coinReward = 50;
                quest.expReward = 25;
                break;
        }

        return quest;
    }

    #endregion

    #region 任务更新

    public void UpdateQuestProgress(QuestType type, int value)
    {
        foreach (var quest in dailyQuests)
        {
            if (quest.type == type && !quest.isCompleted)
            {
                quest.UpdateProgress(value);

                if (quest.isCompleted)
                {
                    OnQuestCompleted?.Invoke(quest);
                    Debug.Log($"任务完成: {quest.questName}");
                }

                SaveDailyQuests();
            }
        }
    }

    #endregion

    #region 任务查询

    public List<DailyQuestData> GetDailyQuests()
    {
        return new List<DailyQuestData>(dailyQuests);
    }

    public List<DailyQuestData> GetCompletedQuests()
    {
        List<DailyQuestData> completed = new List<DailyQuestData>();
        foreach (var quest in dailyQuests)
        {
            if (quest.isCompleted)
            {
                completed.Add(quest);
            }
        }
        return completed;
    }

    public List<DailyQuestData> GetUnclaimedQuests()
    {
        List<DailyQuestData> unclaimed = new List<DailyQuestData>();
        foreach (var quest in dailyQuests)
        {
            if (quest.isCompleted && !quest.isClaimed)
            {
                unclaimed.Add(quest);
            }
        }
        return unclaimed;
    }

    public int GetCompletedCount()
    {
        int count = 0;
        foreach (var quest in dailyQuests)
        {
            if (quest.isCompleted)
            {
                count++;
            }
        }
        return count;
    }

    #endregion

    #region 奖励领取

    public bool ClaimQuestReward(string questId)
    {
        foreach (var quest in dailyQuests)
        {
            if (quest.questId == questId && quest.isCompleted && !quest.isClaimed)
            {
                quest.isClaimed = true;

                // 发放奖励
                GrantRewards(quest);

                OnQuestClaimed?.Invoke(quest);
                SaveDailyQuests();

                Debug.Log($"已领取任务奖励: {quest.questName} - {quest.coinReward}金币");
                return true;
            }
        }
        return false;
    }

    public bool ClaimAllRewards()
    {
        bool claimedAny = false;
        List<DailyQuestData> unclaimed = GetUnclaimedQuests();

        foreach (var quest in unclaimed)
        {
            if (ClaimQuestReward(quest.questId))
            {
                claimedAny = true;
            }
        }

        return claimedAny;
    }

    private void GrantRewards(DailyQuestData quest)
    {
        // 发放金币奖励
        if (SaveManager.Instance != null && quest.coinReward > 0)
        {
            // TODO: 添加金币到玩家账户
            Debug.Log($"获得 {quest.coinReward} 金币");
        }

        // 发放经验奖励
        if (quest.expReward > 0)
        {
            // TODO: 添加经验到玩家账户
            Debug.Log($"获得 {quest.expReward} 经验");
        }
    }

    #endregion

    #region 保存和加载

    private void SaveDailyQuests()
    {
        try
        {
            DailyQuestSaveData saveData = new DailyQuestSaveData();
            saveData.quests = dailyQuests;

            string jsonData = JsonUtility.ToJson(saveData, true);
            string filePath = GetQuestFilePath();
            System.IO.File.WriteAllText(filePath, jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError($"保存每日任务失败: {e.Message}");
        }
    }

    private void LoadDailyQuests()
    {
        try
        {
            string filePath = GetQuestFilePath();
            if (System.IO.File.Exists(filePath))
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                DailyQuestSaveData saveData = JsonUtility.FromJson<DailyQuestSaveData>(jsonData);

                if (saveData != null && saveData.quests != null)
                {
                    dailyQuests = saveData.quests;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载每日任务失败: {e.Message}");
        }
    }

    private string GetQuestFilePath()
    {
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        return System.IO.Path.Combine(directory, "daily_quests.dat");
    }

    #endregion
}

[System.Serializable]
public class DailyQuestSaveData
{
    public List<DailyQuestData> quests;
}
