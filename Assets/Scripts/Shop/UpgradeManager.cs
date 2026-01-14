using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 升级数据
/// </summary>
[System.Serializable]
public class UpgradeData
{
    public string upgradeId;
    public string upgradeName;
    public int currentLevel;
    public int maxLevel;
    public int[] costsPerLevel;

    public int GetCostForNextLevel()
    {
        if (currentLevel >= maxLevel)
        {
            return -1; // 已达最高级
        }

        if (currentLevel >= 0 && currentLevel < costsPerLevel.Length)
        {
            return costsPerLevel[currentLevel];
        }

        return -1;
    }

    public bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }

    public void Upgrade()
    {
        if (CanUpgrade())
        {
            currentLevel++;
        }
    }

    public float GetUpgradeValue()
    {
        return (float)currentLevel / maxLevel;
    }
}

/// <summary>
/// 升级管理器
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("升级设置")]
    [SerializeField] private int maxUpgradeLevel = 10;

    // 升级数据
    private Dictionary<string, UpgradeData> upgrades = new Dictionary<string, UpgradeData>();

    // 单例
    private static UpgradeManager _instance;
    public static UpgradeManager Instance => _instance;

    // 事件
    public event System.Action<string, int> OnUpgradeCompleted;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUpgrades();
            LoadUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUpgrades()
    {
        // 磁铁范围升级
        CreateUpgrade("magnet_range", "磁铁范围", maxUpgradeLevel, new int[] { 100, 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 });

        // 护盾强度升级
        CreateUpgrade("shield_strength", "护盾强度", maxUpgradeLevel, new int[] { 100, 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 });

        // 加速持续时间升级
        CreateUpgrade("speed_boost_duration", "加速时间", maxUpgradeLevel, new int[] { 100, 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 });

        // 初始金币升级
        CreateUpgrade("starting_coins", "初始金币", maxUpgradeLevel, new int[] { 100, 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 });

        // 分数倍率升级
        CreateUpgrade("score_multiplier", "分数倍率", maxUpgradeLevel, new int[] { 100, 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 });
    }

    private void CreateUpgrade(string id, string name, int maxLevel, int[] costs)
    {
        if (!upgrades.ContainsKey(id))
        {
            UpgradeData upgrade = new UpgradeData();
            upgrade.upgradeId = id;
            upgrade.upgradeName = name;
            upgrade.currentLevel = 0;
            upgrade.maxLevel = maxLevel;
            upgrade.costsPerLevel = costs;
            upgrades[id] = upgrade;
        }
    }

    #region 升级操作

    public bool CanUpgrade(string upgradeId)
    {
        if (!upgrades.ContainsKey(upgradeId)) return false;

        UpgradeData upgrade = upgrades[upgradeId];
        int cost = upgrade.GetCostForNextLevel();

        if (cost < 0) return false;

        int playerCoins = SaveManager.Instance?.TotalCoins ?? 0;
        return playerCoins >= cost;
    }

    public bool Upgrade(string upgradeId)
    {
        if (!CanUpgrade(upgradeId)) return false;

        UpgradeData upgrade = upgrades[upgradeId];
        int cost = upgrade.GetCostForNextLevel();

        // 扣除金币
        // TODO: 实现金币扣除

        upgrade.Upgrade();
        SaveUpgrades();
        OnUpgradeCompleted?.Invoke(upgradeId, upgrade.currentLevel);

        return true;
    }

    #endregion

    #region 升级查询

    public int GetUpgradeLevel(string upgradeId)
    {
        if (upgrades.ContainsKey(upgradeId))
        {
            return upgrades[upgradeId].currentLevel;
        }
        return 0;
    }

    public float GetUpgradeValue(string upgradeId)
    {
        if (upgrades.ContainsKey(upgradeId))
        {
            return upgrades[upgradeId].GetUpgradeValue();
        }
        return 0f;
    }

    public int GetUpgradeCost(string upgradeId)
    {
        if (upgrades.ContainsKey(upgradeId))
        {
            return upgrades[upgradeId].GetCostForNextLevel();
        }
        return -1;
    }

    public bool IsMaxLevel(string upgradeId)
    {
        if (upgrades.ContainsKey(upgradeId))
        {
            return !upgrades[upgradeId].CanUpgrade();
        }
        return true;
    }

    public List<UpgradeData> GetAllUpgrades()
    {
        return new List<UpgradeData>(upgrades.Values);
    }

    #endregion

    #region 升级效果

    public float GetMagnetRangeMultiplier()
    {
        int level = GetUpgradeLevel("magnet_range");
        return 1f + (level * 0.1f); // 每级+10%
    }

    public int GetShieldStrength()
    {
        int level = GetUpgradeLevel("shield_strength");
        return 3 + level; // 基础3次+等级
    }

    public float GetSpeedBoostDuration()
    {
        int level = GetUpgradeLevel("speed_boost_duration");
        return 10f + (level * 2f); // 基础10秒+每级+2秒
    }

    public int GetStartingCoins()
    {
        int level = GetUpgradeLevel("starting_coins");
        return level * 10; // 每级+10金币
    }

    public float GetScoreMultiplier()
    {
        int level = GetUpgradeLevel("score_multiplier");
        return 1f + (level * 0.05f); // 每级+5%
    }

    #endregion

    #region 保存和加载

    private void SaveUpgrades()
    {
        try
        {
            UpgradeSaveData saveData = new UpgradeSaveData();
            saveData.upgrades = new List<UpgradeData>(upgrades.Values);

            string jsonData = JsonUtility.ToJson(saveData, true);
            string filePath = GetUpgradeFilePath();
            System.IO.File.WriteAllText(filePath, jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存升级失败: {e.Message}");
        }
    }

    private void LoadUpgrades()
    {
        try
        {
            string filePath = GetUpgradeFilePath();
            if (System.IO.File.Exists(filePath))
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                UpgradeSaveData saveData = JsonUtility.FromJson<UpgradeSaveData>(jsonData);

                if (saveData != null && saveData.upgrades != null)
                {
                    foreach (var upgrade in saveData.upgrades)
                    {
                        if (upgrades.ContainsKey(upgrade.upgradeId))
                        {
                            upgrades[upgrade.upgradeId] = upgrade;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载升级失败: {e.Message}");
        }
    }

    private string GetUpgradeFilePath()
    {
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        return System.IO.Path.Combine(directory, "upgrades.dat");
    }

    #endregion

    #region 重置

    public void ResetAllUpgrades()
    {
        foreach (var upgrade in upgrades.Values)
        {
            upgrade.currentLevel = 0;
        }
        SaveUpgrades();
    }

    public void ResetUpgrade(string upgradeId)
    {
        if (upgrades.ContainsKey(upgradeId))
        {
            upgrades[upgradeId].currentLevel = 0;
            SaveUpgrades();
        }
    }

    #endregion
}

[System.Serializable]
public class UpgradeSaveData
{
    public List<UpgradeData> upgrades;
}
