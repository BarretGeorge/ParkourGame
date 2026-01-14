using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 商店管理器 - 管理所有角色和皮肤
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("角色列表")]
    [SerializeField] private List<CharacterData> allCharacters = new List<CharacterData>();

    [Header("皮肤列表")]
    [SerializeField] private List<SkinData> allSkins = new List<SkinData>();

    // 当前选择
    private int currentCharacterIndex = 0;
    private int currentSkinIndex = 0;

    // 解锁状态缓存
    private Dictionary<string, bool> characterUnlockStatus = new Dictionary<string, bool>();
    private Dictionary<string, bool> skinUnlockStatus = new Dictionary<string, bool>();

    // 单例
    private static ShopManager _instance;
    public static ShopManager Instance => _instance;

    // 事件
    public event System.Action<int> OnCharacterSelected;
    public event System.Action<int> OnSkinSelected;
    public event System.Action<string> OnCharacterUnlocked;
    public event System.Action<string> OnSkinUnlocked;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadShopData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUnlockStatus();
    }

    private void LoadShopData()
    {
        // 从SaveManager加载当前选择
        if (SaveManager.Instance != null)
        {
            SaveData saveData = SaveManager.Instance.GetSaveData();
            currentCharacterIndex = saveData.currentCharacterIndex;
            currentSkinIndex = saveData.currentSkinIndex;

            // 确保索引有效
            currentCharacterIndex = Mathf.Clamp(currentCharacterIndex, 0, allCharacters.Count - 1);
            currentSkinIndex = Mathf.Clamp(currentSkinIndex, 0, GetSkinsForCharacter(currentCharacterIndex).Count - 1);
        }
    }

    private void UpdateUnlockStatus()
    {
        int playerCoins = SaveManager.Instance?.TotalCoins ?? 0;
        int playerHighScore = SaveManager.Instance?.HighScore ?? 0;
        int playerLevel = SaveManager.Instance?.PlayerLevel ?? 1;

        // 更新角色解锁状态
        characterUnlockStatus.Clear();
        foreach (var character in allCharacters)
        {
            bool isUnlocked = character.IsUnlocked(playerCoins, playerHighScore, playerLevel);
            characterUnlockStatus[character.characterId] = isUnlocked;
        }

        // 更新皮肤解锁状态
        skinUnlockStatus.Clear();
        foreach (var skin in allSkins)
        {
            // 检查SaveData中的实际解锁状态
            if (SaveManager.Instance != null)
            {
                SaveData saveData = SaveManager.Instance.GetSaveData();
                int skinIndex = allSkins.IndexOf(skin);
                bool isUnlocked = skinIndex < saveData.unlockedSkins.Length && saveData.unlockedSkins[skinIndex];
                skinUnlockStatus[skin.skinId] = isUnlocked;
            }
        }
    }

    #region 角色管理

    public List<CharacterData> GetAllCharacters()
    {
        return new List<CharacterData>(allCharacters);
    }

    public CharacterData GetCharacter(int index)
    {
        if (index >= 0 && index < allCharacters.Count)
        {
            return allCharacters[index];
        }
        return null;
    }

    public CharacterData GetCharacter(string characterId)
    {
        foreach (var character in allCharacters)
        {
            if (character.characterId == characterId)
            {
                return character;
            }
        }
        return null;
    }

    public int GetCharacterCount()
    {
        return allCharacters.Count;
    }

    public bool IsCharacterUnlocked(int index)
    {
        if (index >= 0 && index < allCharacters.Count)
        {
            string characterId = allCharacters[index].characterId;
            return characterUnlockStatus.ContainsKey(characterId) && characterUnlockStatus[characterId];
        }
        return false;
    }

    public bool IsCharacterUnlocked(string characterId)
    {
        return characterUnlockStatus.ContainsKey(characterId) && characterUnlockStatus[characterId];
    }

    public bool UnlockCharacter(int index)
    {
        if (index >= 0 && index < allCharacters.Count)
        {
            CharacterData character = allCharacters[index];

            if (character.unlockType == UnlockType.BuyWithCoins)
            {
                int playerCoins = SaveManager.Instance?.TotalCoins ?? 0;

                if (playerCoins >= character.unlockCost)
                {
                    // 扣除金币
                    if (SaveManager.Instance != null)
                    {
                        SaveManager.Instance.SpendCoins(character.unlockCost);
                    }

                    // 解锁角色
                    if (SaveManager.Instance != null)
                    {
                        SaveManager.Instance.GetSaveData().UnlockCharacter(index);
                        SaveManager.Instance.SaveGame(false);
                    }

                    characterUnlockStatus[character.characterId] = true;
                    OnCharacterUnlocked?.Invoke(character.characterId);
                    return true;
                }
            }
        }
        return false;
    }

    public void SelectCharacter(int index)
    {
        if (index >= 0 && index < allCharacters.Count)
        {
            if (IsCharacterUnlocked(index))
            {
                currentCharacterIndex = index;
                OnCharacterSelected?.Invoke(index);

                // 保存选择
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.UpdateCharacterSelection(currentCharacterIndex, currentSkinIndex);
                }
            }
        }
    }

    public int GetCurrentCharacterIndex()
    {
        return currentCharacterIndex;
    }

    public CharacterData GetCurrentCharacter()
    {
        return GetCharacter(currentCharacterIndex);
    }

    #endregion

    #region 皮肤管理

    public List<SkinData> GetAllSkins()
    {
        return new List<SkinData>(allSkins);
    }

    public List<SkinData> GetSkinsForCharacter(int characterIndex)
    {
        List<SkinData> skins = new List<SkinData>();

        if (characterIndex >= 0 && characterIndex < allCharacters.Count)
        {
            string characterId = allCharacters[characterIndex].characterId;

            foreach (var skin in allSkins)
            {
                // 添加通用皮肤或该角色专属皮肤
                if (string.IsNullOrEmpty(skin.characterId) || skin.characterId == characterId)
                {
                    skins.Add(skin);
                }
            }
        }

        return skins;
    }

    public SkinData GetSkin(int index)
    {
        if (index >= 0 && index < allSkins.Count)
        {
            return allSkins[index];
        }
        return null;
    }

    public int GetSkinCount()
    {
        return allSkins.Count;
    }

    public bool IsSkinUnlocked(int index)
    {
        if (index >= 0 && index < allSkins.Count)
        {
            if (SaveManager.Instance != null)
            {
                SaveData saveData = SaveManager.Instance.GetSaveData();
                return index < saveData.unlockedSkins.Length && saveData.unlockedSkins[index];
            }
        }
        return false;
    }

    public bool UnlockSkin(int index)
    {
        if (index >= 0 && index < allSkins.Count)
        {
            SkinData skin = allSkins[index];

            if (skin.unlockType == UnlockType.BuyWithCoins)
            {
                int playerCoins = SaveManager.Instance?.TotalCoins ?? 0;

                if (playerCoins >= skin.unlockCost)
                {
                    // 扣除金币
                    if (SaveManager.Instance != null)
                    {
                        SaveManager.Instance.SpendCoins(skin.unlockCost);
                    }

                    // 解锁皮肤
                    if (SaveManager.Instance != null)
                    {
                        SaveManager.Instance.GetSaveData().UnlockSkin(index);
                        SaveManager.Instance.SaveGame(false);
                    }

                    skinUnlockStatus[skin.skinId] = true;
                    OnSkinUnlocked?.Invoke(skin.skinId);
                    return true;
                }
            }
        }
        return false;
    }

    public void SelectSkin(int index)
    {
        if (index >= 0 && index < allSkins.Count)
        {
            if (IsSkinUnlocked(index))
            {
                currentSkinIndex = index;
                OnSkinSelected?.Invoke(index);

                // 保存选择
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.UpdateCharacterSelection(currentCharacterIndex, currentSkinIndex);
                }
            }
        }
    }

    public int GetCurrentSkinIndex()
    {
        return currentSkinIndex;
    }

    public SkinData GetCurrentSkin()
    {
        return GetSkin(currentSkinIndex);
    }

    #endregion

    #region 商店操作

    public bool CanAfford(int cost)
    {
        int playerCoins = SaveManager.Instance?.TotalCoins ?? 0;
        return playerCoins >= cost;
    }

    public void SpendCoins(int amount)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SpendCoins(amount);
            Debug.Log($"花费 {amount} 金币");
        }
    }

    #endregion

    #region 编辑器辅助

#if UNITY_EDITOR
    public void AddCharacter(CharacterData character)
    {
        if (character != null && !allCharacters.Contains(character))
        {
            allCharacters.Add(character);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    public void RemoveCharacter(CharacterData character)
    {
        if (allCharacters.Contains(character))
        {
            allCharacters.Remove(character);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    public void AddSkin(SkinData skin)
    {
        if (skin != null && !allSkins.Contains(skin))
        {
            allSkins.Add(skin);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    public void RemoveSkin(SkinData skin)
    {
        if (allSkins.Contains(skin))
        {
            allSkins.Remove(skin);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    #endregion
}
