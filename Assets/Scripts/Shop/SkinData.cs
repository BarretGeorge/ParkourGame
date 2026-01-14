using UnityEngine;

/// <summary>
/// 皮肤数据定义
/// </summary>
[CreateAssetMenu(fileName = "NewSkin", menuName = "Game/Skin Data")]
public class SkinData : ScriptableObject
{
    [Header("基本信息")]
    public string skinName;
    public string skinId;
    public Sprite skinIcon;
    [TextArea]
    public string description;

    [Header("所属角色")]
    public string characterId; // 该皮肤属于哪个角色（空表示通用）

    [Header("解锁条件")]
    public UnlockType unlockType;
    public int unlockCost;           // 金币购买价格
    public int requiredWins;         // 需要胜利次数
    public int requiredDistance;     // 需要跑的总距离

    [Header("视觉效果")]
    public Material skinMaterial;
    public Texture2D skinTexture;
    public GameObject skinPrefab;
    public Color skinColor = Color.white;

    [Header("粒子特效")]
    public bool hasTrailEffect;
    public GameObject trailEffectPrefab;
    public bool hasGlowEffect;
    public Color glowColor;

    [Header("稀有度")]
    public Rarity rarity;

    public bool IsUnlocked(int playerCoins, int playerWins, float playerDistance)
    {
        switch (unlockType)
        {
            case UnlockType.Free:
                return true;
            case UnlockType.BuyWithCoins:
                return playerCoins >= unlockCost;
            case UnlockType.ReachScore:
                return playerWins >= requiredWins;
            case UnlockType.ReachLevel:
                return playerDistance >= requiredDistance;
            case UnlockType.Special:
                return false;
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
                return $"需要 {requiredWins} 次胜利";
            case UnlockType.ReachLevel:
                return $"需要跑 {requiredDistance} 米";
            case UnlockType.Special:
                return "特殊解锁条件";
            default:
                return "未知";
        }
    }
}
