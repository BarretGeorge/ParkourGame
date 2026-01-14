using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 成就状态
/// </summary>
[System.Serializable]
public class AchievementStatus
{
    public string achievementId;
    public bool isUnlocked;
    public float progress;
    public float targetValue;
    public string unlockTime;
    public int currentStage; // 用于多阶段成就

    public float ProgressPercentage => targetValue > 0 ? (progress / targetValue) * 100f : 0f;
}

/// <summary>
/// 成就管理器
/// </summary>
public class AchievementManager : MonoBehaviour
{
    [Header("成就配置")]
    [SerializeField] private List<AchievementDefinition> achievementDefinitions;

    // 成就状态
    private Dictionary<string, AchievementStatus> achievementStatus = new Dictionary<string, AchievementStatus>();

    // 单例
    private static AchievementManager _instance;
    public static AchievementManager Instance => _instance;

    // 事件
    public event System.Action<AchievementDefinition> OnAchievementUnlocked;
    public event System.Action<AchievementDefinition, float> OnAchievementProgress;

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
        // 从ScriptableObject初始化成就
        achievementStatus.Clear();

        foreach (var definition in achievementDefinitions)
        {
            if (definition != null && !achievementStatus.ContainsKey(definition.achievementId))
            {
                AchievementStatus status = new AchievementStatus();
                status.achievementId = definition.achievementId;
                status.isUnlocked = false;
                status.progress = 0f;
                status.targetValue = definition.targetValue;
                status.currentStage = 0;
                achievementStatus[definition.achievementId] = status;
            }
        }
    }

    #region 成就更新

    public void UpdateAchievement(string achievementId, float value)
    {
        if (!achievementStatus.ContainsKey(achievementId))
        {
            Debug.LogWarning($"成就 {achievementId} 不存在");
            return;
        }

        AchievementStatus status = achievementStatus[achievementId];

        if (status.isUnlocked) return; // 已解锁

        // 检查是否有前置成就
        AchievementDefinition definition = GetAchievementDefinition(achievementId);
        if (definition != null && definition.type == AchievementType.Chain)
        {
            if (!string.IsNullOrEmpty(definition.linkedAchievementId))
            {
                if (!IsAchievementUnlocked(definition.linkedAchievementId))
                {
                    return; // 前置成就未完成
                }
            }
        }

        // 更新进度
        status.progress = Mathf.Max(status.progress, value);
        OnAchievementProgress?.Invoke(definition, status.progress);

        // 检查是否完成
        if (status.progress >= status.targetValue)
        {
            UnlockAchievement(achievementId);
        }

        SaveAchievements();
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!achievementStatus.ContainsKey(achievementId)) return;

        AchievementStatus status = achievementStatus[achievementId];

        if (status.isUnlocked) return; // 已解锁

        status.isUnlocked = true;
        status.unlockTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        AchievementDefinition definition = GetAchievementDefinition(achievementId);
        if (definition != null)
        {
            OnAchievementUnlocked?.Invoke(definition);

            // 发放奖励
            GrantAchievementRewards(definition);

            Debug.Log($"🏆 成就解锁: {definition.achievementName}");
        }

        SaveAchievements();
    }

    private void GrantAchievementRewards(AchievementDefinition definition)
    {
        // 金币奖励
        if (definition.coinReward > 0 && SaveManager.Instance != null)
        {
            SaveManager.Instance.AddCoins(definition.coinReward);
            Debug.Log($"成就奖励: {definition.coinReward} 金币");
        }

        // 经验奖励
        if (definition.expReward > 0 && SaveManager.Instance != null)
        {
            SaveManager.Instance.AddExperience(definition.expReward);
            Debug.Log($"成就奖励: {definition.expReward} 经验");
        }

        // 解锁角色
        if (!string.IsNullOrEmpty(definition.unlockCharacterId) && ShopManager.Instance != null)
        {
            CharacterData character = ShopManager.Instance.GetCharacter(definition.unlockCharacterId);
            if (character != null)
            {
                int characterIndex = ShopManager.Instance.GetAllCharacters().IndexOf(character);
                if (characterIndex >= 0)
                {
                    SaveManager.Instance.GetSaveData().UnlockCharacter(characterIndex);
                    Debug.Log($"成就奖励: 解锁角色 {definition.unlockCharacterId}");
                }
            }
        }

        // 解锁皮肤
        if (!string.IsNullOrEmpty(definition.unlockSkinId) && ShopManager.Instance != null)
        {
            SkinData skin = ShopManager.Instance.GetSkin(definition.unlockSkinId);
            if (skin != null)
            {
                int skinIndex = ShopManager.Instance.GetAllSkins().IndexOf(skin);
                if (skinIndex >= 0)
                {
                    SaveManager.Instance.GetSaveData().UnlockSkin(skinIndex);
                    Debug.Log($"成就奖励: 解锁皮肤 {definition.unlockSkinId}");
                }
            }
        }
    }

    #endregion

    #region 成就查询

    public AchievementDefinition GetAchievementDefinition(string achievementId)
    {
        foreach (var definition in achievementDefinitions)
        {
            if (definition != null && definition.achievementId == achievementId)
            {
                return definition;
            }
        }
        return null;
    }

    public List<AchievementDefinition> GetAllAchievementDefinitions()
    {
        return achievementDefinitions.Where(d => d != null).ToList();
    }

    public List<AchievementDefinition> GetAchievementsByCategory(AchievementCategory category)
    {
        return achievementDefinitions
            .Where(d => d != null && d.category == category)
            .ToList();
    }

    public List<AchievementDefinition> GetUnlockedAchievements()
    {
        List<AchievementDefinition> unlocked = new List<AchievementDefinition>();

        foreach (var definition in achievementDefinitions)
        {
            if (definition != null && IsAchievementUnlocked(definition.achievementId))
            {
                unlocked.Add(definition);
            }
        }

        return unlocked;
    }

    public List<AchievementDefinition> GetLockedAchievements()
    {
        List<AchievementDefinition> locked = new List<AchievementDefinition>();

        foreach (var definition in achievementDefinitions)
        {
            if (definition != null && !IsAchievementUnlocked(definition.achievementId))
            {
                // 隐藏成就不显示
                if (!definition.isHidden)
                {
                    locked.Add(definition);
                }
            }
        }

        return locked;
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        if (achievementStatus.ContainsKey(achievementId))
        {
            return achievementStatus[achievementId].isUnlocked;
        }
        return false;
    }

    public float GetAchievementProgress(string achievementId)
    {
        if (achievementStatus.ContainsKey(achievementId))
        {
            return achievementStatus[achievementId].progress;
        }
        return 0f;
    }

    public float GetAchievementProgressPercentage(string achievementId)
    {
        if (achievementStatus.ContainsKey(achievementId))
        {
            return achievementStatus[achievementId].ProgressPercentage;
        }
        return 0f;
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var status in achievementStatus.Values)
        {
            if (status.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }

    public int GetTotalCount()
    {
        return achievementDefinitions.Count;
    }

    public float GetCompletionPercentage()
    {
        int total = GetTotalCount();
        if (total == 0) return 0f;
        return (float)GetUnlockedCount() / total * 100f;
    }

    #endregion

    #region 统计

    public Dictionary<AchievementRarity, int> GetUnlockCountByRarity()
    {
        Dictionary<AchievementRarity, int> countByRarity = new Dictionary<AchievementRarity, int>();

        foreach (var definition in achievementDefinitions)
        {
            if (definition != null)
            {
                if (!countByRarity.ContainsKey(definition.rarity))
                {
                    countByRarity[definition.rarity] = 0;
                }

                if (IsAchievementUnlocked(definition.achievementId))
                {
                    countByRarity[definition.rarity]++;
                }
            }
        }

        return countByRarity;
    }

    #endregion

    #region 保存和加载

    private void SaveAchievements()
    {
        try
        {
            AchievementSaveData saveData = new AchievementSaveData();
            saveData.statusList = new List<AchievementStatus>(achievementStatus.Values);

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

                if (saveData != null && saveData.statusList != null)
                {
                    foreach (var status in saveData.statusList)
                    {
                        if (achievementStatus.ContainsKey(status.achievementId))
                        {
                            achievementStatus[status.achievementId] = status;
                        }
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
        return System.IO.Path.Combine(directory, "achievements_v2.dat");
    }

    #endregion

    #region 重置

    public void ResetAllAchievements()
    {
        foreach (var status in achievementStatus.Values)
        {
            status.isUnlocked = false;
            status.progress = 0f;
            status.currentStage = 0;
            status.unlockTime = null;
        }
        SaveAchievements();
    }

    public void ResetAchievement(string achievementId)
    {
        if (achievementStatus.ContainsKey(achievementId))
        {
            AchievementStatus status = achievementStatus[achievementId];
            status.isUnlocked = false;
            status.progress = 0f;
            status.currentStage = 0;
            status.unlockTime = null;
            SaveAchievements();
        }
    }

    #endregion
}

[System.Serializable]
public class AchievementSaveData
{
    public List<AchievementStatus> statusList;
}
