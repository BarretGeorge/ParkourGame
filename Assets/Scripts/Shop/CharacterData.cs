using UnityEngine;

/// <summary>
/// 角色数据定义
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("基本信息")]
    public string characterName;
    public string characterId;
    public Sprite characterIcon;
    [TextArea]
    public string description;

    [Header("解锁条件")]
    public UnlockType unlockType;
    public int unlockCost;           // 金币购买价格
    public int requiredScore;         // 需要达到的分数
    public int requiredLevel;         // 需要达到的等级

    [Header("属性修正")]
    [Range(0.8f, 1.2f)]
    public float speedModifier = 1f;
    [Range(0.8f, 1.2f)]
    public float jumpModifier = 1f;
    [Range(0.8f, 1.2f)]
    public float coinMagnetModifier = 1f;

    [Header("特殊能力")]
    public bool hasSpecialAbility;
    public string specialAbilityName;
    [TextArea]
    public string specialAbilityDescription;

    [Header("视觉效果")]
    public GameObject characterPrefab;
    public Material characterMaterial;
    public RuntimeAnimatorController animatorController;

    [Header("稀有度")]
    public Rarity rarity;

    public bool IsUnlocked(int playerCoins, int playerHighScore, int playerLevel)
    {
        switch (unlockType)
        {
            case UnlockType.Free:
                return true;
            case UnlockType.BuyWithCoins:
                return playerCoins >= unlockCost;
            case UnlockType.ReachScore:
                return playerHighScore >= requiredScore;
            case UnlockType.ReachLevel:
                return playerLevel >= requiredLevel;
            case UnlockType.Special:
                return false; // 需要特殊解锁条件
            default:
                return false;
        }
    }

    public string GetUnlockRequirementText()
    {
        switch (unlockType)
        {
            case UnlockType.Free:
                return "免费";
            case UnlockType.BuyWithCoins:
                return $"需要 {unlockCost} 金币";
            case UnlockType.ReachScore:
                return $"需要达到 {requiredScore} 分";
            case UnlockType.ReachLevel:
                return $"需要达到 {requiredLevel} 级";
            case UnlockType.Special:
                return "特殊解锁条件";
            default:
                return "未知";
        }
    }
}

public enum UnlockType
{
    Free,
    BuyWithCoins,
    ReachScore,
    ReachLevel,
    Special
}

public enum Rarity
{
    Common,     // 普通
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary   // 传说
}
