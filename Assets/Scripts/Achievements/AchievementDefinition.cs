using UnityEngine;

/// <summary>
/// 成就定义 - ScriptableObject配置
/// </summary>
[CreateAssetMenu(fileName = "NewAchievement", menuName = "Game/Achievement")]
public class AchievementDefinition : ScriptableObject
{
    [Header("基本信息")]
    public string achievementId;
    public string achievementName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("成就类型")]
    public AchievementType type;
    public AchievementCategory category;

    [Header "完成条件")]
    public float targetValue;
    public string linkedAchievementId; // 需要先完成的其他成就

    [Header("奖励")]
    public int coinReward;
    public int expReward;
    public string unlockCharacterId;
    public string unlockSkinId;

    [Header("显示设置")]
    public bool showNotification = true;
    public bool isHidden = false; // 隐藏成就（完成前不显示）

    [Header("稀有度")]
    public AchievementRarity rarity;
}

public enum AchievementType
{
    Score,              // 分数类
    Distance,           // 距离类
    Coins,              // 金币类
    Runs,               // 运行次数类
    Collectibles,       // 收集品类
    Combo,              // 连击类
    Special,            // 特殊类
    Chain,              // 成就链（需完成前置成就）
    Cumulative          // 累计类
}

public enum AchievementCategory
{
    Gameplay,
    Collection,
    Challenge,
    Secret,
    Multiplayer
}

public enum AchievementRarity
{
    Common,     // 普通
    Uncommon,   // 不常见
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary   // 传说
}
