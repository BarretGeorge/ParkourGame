using System;
using UnityEngine;

/// <summary>
/// 存档数据类
/// </summary>
[Serializable]
public class SaveData
{
    [Header("玩家统计")]
    public int totalCoins;
    public int highScore;
    public float totalDistance;
    public int totalRuns;
    public int playerExperience;
    public int playerLevel;

    [Header("最高分记录")]
    public int dailyHighScore;
    public int weeklyHighScore;
    public int monthlyHighScore;

    [Header("解锁进度")]
    public bool[] unlockedCharacters;
    public bool[] unlockedSkins;
    public int currentCharacterIndex;
    public int currentSkinIndex;

    [Header("游戏进度")]
    public int reachedLevel;
    public int completedLevels;
    public int totalPlayTime; // 秒

    [Header("设置")]
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public int qualityLevel;
    public bool vSyncEnabled;
    public bool fullScreenEnabled;

    [Header("时间戳")]
    public string lastSaveTime;
    public string lastPlayTime;

    // 经验等级常量
    private const int BASE_EXP_REQUIRED = 100;
    private const float EXP_SCALING_FACTOR = 1.5f;

    public SaveData()
    {
        // 初始化默认值
        totalCoins = 0;
        highScore = 0;
        totalDistance = 0f;
        totalRuns = 0;
        playerExperience = 0;
        playerLevel = 1;

        dailyHighScore = 0;
        weeklyHighScore = 0;
        monthlyHighScore = 0;

        unlockedCharacters = new bool[10]; // 10个角色
        unlockedSkins = new bool[50]; // 50个皮肤
        currentCharacterIndex = 0;
        currentSkinIndex = 0;

        reachedLevel = 1;
        completedLevels = 0;
        totalPlayTime = 0;

        masterVolume = 1f;
        musicVolume = 0.8f;
        sfxVolume = 1f;
        qualityLevel = QualitySettings.GetQualityLevel();
        vSyncEnabled = true;
        fullScreenEnabled = true;

        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        lastPlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public void UpdateHighScore(int newScore)
    {
        if (newScore > highScore)
        {
            highScore = newScore;
        }

        // 更新时间段最高分
        dailyHighScore = Mathf.Max(dailyHighScore, newScore);
        weeklyHighScore = Mathf.Max(weeklyHighScore, newScore);
        monthlyHighScore = Mathf.Max(monthlyHighScore, newScore);
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
    }

    public void AddDistance(float distance)
    {
        totalDistance += distance;
    }

    public void IncrementRuns()
    {
        totalRuns++;
    }

    public void AddPlayTime(int seconds)
    {
        totalPlayTime += seconds;
    }

    public void UnlockCharacter(int index)
    {
        if (index >= 0 && index < unlockedCharacters.Length)
        {
            unlockedCharacters[index] = true;
        }
    }

    public void UnlockSkin(int index)
    {
        if (index >= 0 && index < unlockedSkins.Length)
        {
            unlockedSkins[index] = true;
        }
    }

    public bool IsCharacterUnlocked(int index)
    {
        if (index >= 0 && index < unlockedCharacters.Length)
        {
            return unlockedCharacters[index];
        }
        return false;
    }

    public bool IsSkinUnlocked(int index)
    {
        if (index >= 0 && index < unlockedSkins.Length)
        {
            return unlockedSkins[index];
        }
        return false;
    }

    public void UpdateTimestamp()
    {
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public void UpdateLastPlayTime()
    {
        lastPlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    #region 经验和等级系统

    /// <summary>
    /// 添加经验值并自动升级
    /// </summary>
    public void AddExperience(int amount)
    {
        playerExperience += amount;
        CheckLevelUp();
    }

    /// <summary>
    /// 获取当前等级所需经验值
    /// </summary>
    public int GetExperienceRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        return Mathf.FloorToInt(BASE_EXP_REQUIRED * Mathf.Pow(EXP_SCALING_FACTOR, level - 2));
    }

    /// <summary>
    /// 获取升级到下一级所需的经验值
    /// </summary>
    public int GetExperienceToNextLevel()
    {
        return GetExperienceRequiredForLevel(playerLevel + 1) - playerExperience;
    }

    /// <summary>
    /// 检查是否升级
    /// </summary>
    private void CheckLevelUp()
    {
        int requiredExp = GetExperienceRequiredForLevel(playerLevel + 1);
        while (playerExperience >= requiredExp && requiredExp > 0)
        {
            playerLevel++;
            requiredExp = GetExperienceRequiredForLevel(playerLevel + 1);
        }
    }

    /// <summary>
    /// 获取当前等级经验百分比
    /// </summary>
    public float GetExperiencePercentage()
    {
        int currentLevelExp = GetExperienceRequiredForLevel(playerLevel);
        int nextLevelExp = GetExperienceRequiredForLevel(playerLevel + 1);
        if (nextLevelExp <= currentLevelExp) return 1f;
        return Mathf.Clamp01((float)(playerExperience - currentLevelExp) / (nextLevelExp - currentLevelExp));
    }

    #endregion
}
