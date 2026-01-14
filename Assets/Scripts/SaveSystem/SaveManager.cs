using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// 存档管理器 - 负责保存和加载游戏数据
/// 支持本地存档和网络存档混合模式
/// </summary>
public class SaveManager : MonoBehaviour
{
    // 单例
    private static SaveManager _instance;
    public static SaveManager Instance => _instance;

    // 存档数据
    private SaveData currentSaveData;

    // 存档路径
    private string saveFilePath;
    private string backupFilePath;

    // 网络存档管理器（可选）
    private HybridSaveManager hybridSaveManager;

    // 自动保存设置
    [Header("自动保存")]
    [SerializeField] private float autoSaveInterval = 30f;
    [SerializeField] private bool enableAutoSave = true;

    // 网络模式指示
    [Header("网络模式")]
    [SerializeField] private bool enableNetworkMode = true;
    public bool IsNetworkMode => hybridSaveManager != null && hybridSaveManager.IsNetworkMode();

    // 加密密钥（简单XOR加密）
    private const byte ENCRYPTION_KEY = 0x5A;

    // 事件
    public event System.Action OnSaveCompleted;
    public event System.Action OnLoadCompleted;
    public event System.Action<System.Exception> OnSaveError;
    public event System.Action<bool> OnNetworkModeChanged;

    private float autoSaveTimer;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSavePath();
            InitializeNetworkSave();
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化网络存档系统
    /// </summary>
    private void InitializeNetworkSave()
    {
        if (!enableNetworkMode)
        {
            Debug.Log("网络模式已禁用，使用本地存档");
            return;
        }

        // 查找或创建HybridSaveManager
        hybridSaveManager = FindObjectOfType<HybridSaveManager>();
        if (hybridSaveManager == null)
        {
            GameObject networkObj = new GameObject("HybridSaveManager");
            hybridSaveManager = networkObj.AddComponent<HybridSaveManager>();
            DontDestroyOnLoad(networkObj);
        }

        // 订阅网络模式变化事件
        if (hybridSaveManager != null)
        {
            hybridSaveManager.OnSyncCompleted += (success) =>
            {
                Debug.Log(success ? "网络存档同步成功" : "网络存档同步失败");
            };

            hybridSaveManager.OnError += (error) =>
            {
                Debug.LogWarning($"网络存档错误: {error}");
            };

            // 检查是否启用网络模式
            bool isNetworkEnabled = hybridSaveManager.IsNetworkMode();
            Debug.Log($"网络模式状态: {(isNetworkEnabled ? "已启用" : "未启用")}");
            OnNetworkModeChanged?.Invoke(isNetworkEnabled);
        }
    }

    private void Update()
    {
        if (enableAutoSave)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveGame(false); // 静默保存
            }
        }
    }

    private void InitializeSavePath()
    {
        // 使用持久化数据路径
        string directory = Path.Combine(Application.persistentDataPath, "Saves");

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        saveFilePath = Path.Combine(directory, "save.dat");
        backupFilePath = Path.Combine(directory, "save_backup.dat");

        Debug.Log($"存档路径: {saveFilePath}");
    }

    #region 保存和加载

    public void SaveGame(bool showNotification = true)
    {
        if (currentSaveData == null)
        {
            currentSaveData = new SaveData();
        }

        // 如果启用网络模式且网络可用，使用HybridSaveManager
        if (IsNetworkMode && hybridSaveManager != null)
        {
            hybridSaveManager.SaveGame(currentSaveData, (success, message) =>
            {
                if (showNotification)
                {
                    Debug.Log($"游戏已保存 - {message} - 金币: {currentSaveData.totalCoins}, 最高分: {currentSaveData.highScore}");
                }
                OnSaveCompleted?.Invoke();
            });
            return;
        }

        // 否则使用本地保存
        try
        {
            // 更新时间戳
            currentSaveData.UpdateTimestamp();

            // 序列化数据
            string jsonData = JsonUtility.ToJson(currentSaveData, true);

            // 加密数据
            byte[] encryptedData = EncryptData(jsonData);

            // 写入文件
            File.WriteAllBytes(saveFilePath, encryptedData);

            // 创建备份
            if (File.Exists(saveFilePath))
            {
                File.Copy(saveFilePath, backupFilePath, true);
            }

            if (showNotification)
            {
                Debug.Log($"游戏已保存（本地） - 金币: {currentSaveData.totalCoins}, 最高分: {currentSaveData.highScore}");
            }

            OnSaveCompleted?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存失败: {e.Message}");
            OnSaveError?.Invoke(e);
        }
    }

    public void LoadGame()
    {
        // 如果启用网络模式且网络可用，使用HybridSaveManager
        if (IsNetworkMode && hybridSaveManager != null)
        {
            hybridSaveManager.LoadGame((success, saveData, message) =>
            {
                if (success && saveData != null)
                {
                    currentSaveData = saveData;
                    ApplySettings();
                    Debug.Log($"存档加载成功（网络） - {message} - 金币: {currentSaveData.totalCoins}, 最高分: {currentSaveData.highScore}");
                    OnLoadCompleted?.Invoke();
                }
                else
                {
                    // 网络加载失败，尝试本地加载
                    Debug.Log($"网络加载失败: {message}，尝试本地加载...");
                    LoadGameLocal();
                }
            });
            return;
        }

        // 使用本地加载
        LoadGameLocal();
    }

    /// <summary>
    /// 本地加载游戏数据
    /// </summary>
    private void LoadGameLocal()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                // 读取文件
                byte[] encryptedData = File.ReadAllBytes(saveFilePath);

                // 解密数据
                string jsonData = DecryptData(encryptedData);

                // 反序列化
                currentSaveData = JsonUtility.FromJson<SaveData>(jsonData);

                if (currentSaveData != null)
                {
                    // 应用设置
                    ApplySettings();

                    Debug.Log($"存档加载成功（本地） - 金币: {currentSaveData.totalCoins}, 最高分: {currentSaveData.highScore}");
                    OnLoadCompleted?.Invoke();
                }
                else
                {
                    CreateNewSave();
                }
            }
            else
            {
                Debug.Log("未找到存档文件，创建新存档");
                CreateNewSave();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载存档失败: {e.Message}");

            // 尝试加载备份
            if (File.Exists(backupFilePath))
            {
                Debug.Log("尝试加载备份存档...");
                LoadBackup();
            }
            else
            {
                CreateNewSave();
            }
        }
    }

    private void LoadBackup()
    {
        try
        {
            byte[] encryptedData = File.ReadAllBytes(backupFilePath);
            string jsonData = DecryptData(encryptedData);
            currentSaveData = JsonUtility.FromJson<SaveData>(jsonData);

            if (currentSaveData != null)
            {
                ApplySettings();
                Debug.Log("备份存档加载成功");
                OnLoadCompleted?.Invoke();
            }
            else
            {
                CreateNewSave();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载备份失败: {e.Message}");
            CreateNewSave();
        }
    }

    private void CreateNewSave()
    {
        currentSaveData = new SaveData();
        SaveGame(false);
        OnLoadCompleted?.Invoke();
    }

    #endregion

    #region 加密和解密

    private byte[] EncryptData(string data)
    {
        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
        byte[] encryptedBytes = new byte[dataBytes.Length];

        for (int i = 0; i < dataBytes.Length; i++)
        {
            encryptedBytes[i] = (byte)(dataBytes[i] ^ ENCRYPTION_KEY);
        }

        return encryptedBytes;
    }

    private string DecryptData(byte[] encryptedData)
    {
        byte[] decryptedBytes = new byte[encryptedData.Length];

        for (int i = 0; i < encryptedData.Length; i++)
        {
            decryptedBytes[i] = (byte)(encryptedData[i] ^ ENCRYPTION_KEY);
        }

        return System.Text.Encoding.UTF8.GetString(decryptedBytes);
    }

    #endregion

    #region 数据访问

    public SaveData GetSaveData()
    {
        if (currentSaveData == null)
        {
            currentSaveData = new SaveData();
        }
        return currentSaveData;
    }

    public int TotalCoins
    {
        get => currentSaveData != null ? currentSaveData.totalCoins : 0;
        set
        {
            if (currentSaveData != null)
            {
                currentSaveData.totalCoins = value;
            }
        }
    }

    public int PlayerLevel
    {
        get => currentSaveData != null ? currentSaveData.playerLevel : 1;
    }

    public int PlayerExperience
    {
        get => currentSaveData != null ? currentSaveData.playerExperience : 0;
    }

    public int ExperienceToNextLevel
    {
        get => currentSaveData != null ? currentSaveData.GetExperienceToNextLevel() : 100;
    }

    public float ExperiencePercentage
    {
        get => currentSaveData != null ? currentSaveData.GetExperiencePercentage() : 0f;
    }

    public int HighScore
    {
        get => currentSaveData != null ? currentSaveData.highScore : 0;
    }

    public float TotalDistance
    {
        get => currentSaveData != null ? currentSaveData.totalDistance : 0f;
    }

    public int TotalRuns
    {
        get => currentSaveData != null ? currentSaveData.totalRuns : 0;
    }

    #endregion

    #region 金币和经验管理

    /// <summary>
    /// 添加金币
    /// </summary>
    public void AddCoins(int amount)
    {
        if (currentSaveData != null && amount > 0)
        {
            currentSaveData.totalCoins += amount;
            SaveGame(false);
        }
    }

    /// <summary>
    /// 花费金币
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (currentSaveData != null && currentSaveData.totalCoins >= amount)
        {
            currentSaveData.totalCoins -= amount;
            SaveGame(false);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 添加经验值
    /// </summary>
    public void AddExperience(int amount)
    {
        if (currentSaveData != null && amount > 0)
        {
            int oldLevel = currentSaveData.playerLevel;
            currentSaveData.AddExperience(amount);
            int newLevel = currentSaveData.playerLevel;

            SaveGame(false);

            // 如果升级了，可以触发事件
            if (newLevel > oldLevel)
            {
                Debug.Log($"升级！从 {oldLevel} 升到 {newLevel}");
            }
        }
    }

    #endregion

    #region 设置应用

    private void ApplySettings()
    {
        if (currentSaveData == null) return;

        // 应用音量设置
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MasterVolume = currentSaveData.masterVolume;
            AudioManager.Instance.MusicVolume = currentSaveData.musicVolume;
            AudioManager.Instance.SFXVolume = currentSaveData.sfxVolume;
        }

        // 应用图形设置
        QualitySettings.SetQualityLevel(currentSaveData.qualityLevel);
        QualitySettings.vSyncCount = currentSaveData.vSyncEnabled ? 1 : 0;
        Screen.fullScreen = currentSaveData.fullScreenEnabled;
    }

    public void UpdateSettings(float masterVolume, float musicVolume, float sfxVolume,
                               int qualityLevel, bool vSync, bool fullScreen)
    {
        if (currentSaveData == null) return;

        currentSaveData.masterVolume = masterVolume;
        currentSaveData.musicVolume = musicVolume;
        currentSaveData.sfxVolume = sfxVolume;
        currentSaveData.qualityLevel = qualityLevel;
        currentSaveData.vSyncEnabled = vSync;
        currentSaveData.fullScreenEnabled = fullScreen;
    }

    #endregion

    #region 存档管理

    public void DeleteSave()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }

            Debug.Log("存档已删除");
            CreateNewSave();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"删除存档失败: {e.Message}");
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    public string GetSaveFilePath()
    {
        return saveFilePath;
    }

    public System.DateTime GetLastSaveTime()
    {
        if (currentSaveData != null && !string.IsNullOrEmpty(currentSaveData.lastSaveTime))
        {
            if (System.DateTime.TryParse(currentSaveData.lastSaveTime, out System.DateTime result))
            {
                return result;
            }
        }
        return System.DateTime.MinValue;
    }

    #endregion

    #region 游戏数据更新

    public void UpdateRunStats(int score, int coins, float distance)
    {
        if (currentSaveData == null) return;

        currentSaveData.UpdateHighScore(score);
        currentSaveData.AddCoins(coins);
        currentSaveData.AddDistance(distance);
        currentSaveData.IncrementRuns();
        currentSaveData.UpdateLastPlayTime();

        SaveGame(false);
    }

    public void UpdateCharacterSelection(int characterIndex, int skinIndex)
    {
        if (currentSaveData == null) return;

        currentSaveData.currentCharacterIndex = characterIndex;
        currentSaveData.currentSkinIndex = skinIndex;

        SaveGame(false);
    }

    #endregion

    #region 导出/导入

    public string ExportSaveData()
    {
        if (currentSaveData == null) return null;

        try
        {
            return JsonUtility.ToJson(currentSaveData, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导出存档失败: {e.Message}");
            return null;
        }
    }

    public bool ImportSaveData(string jsonData)
    {
        try
        {
            SaveData importedData = JsonUtility.FromJson<SaveData>(jsonData);
            if (importedData != null)
            {
                currentSaveData = importedData;
                SaveGame(false);
                ApplySettings();
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导入存档失败: {e.Message}");
        }
        return false;
    }

    #endregion

    #region 重置时间段最高分

    public void ResetDailyHighScore()
    {
        if (currentSaveData != null)
        {
            currentSaveData.dailyHighScore = 0;
            SaveGame(false);
        }
    }

    public void ResetWeeklyHighScore()
    {
        if (currentSaveData != null)
        {
            currentSaveData.weeklyHighScore = 0;
            SaveGame(false);
        }
    }

    public void ResetMonthlyHighScore()
    {
        if (currentSaveData != null)
        {
            currentSaveData.monthlyHighScore = 0;
            SaveGame(false);
        }
    }

    #endregion

    #region 网络存档功能

    /// <summary>
    /// 手动同步服务器存档
    /// </summary>
    public void SyncWithServer(System.Action<bool, string> callback = null)
    {
        if (hybridSaveManager == null || !IsNetworkMode)
        {
            callback?.Invoke(false, "网络模式未启用");
            return;
        }

        hybridSaveManager.SyncWithServer(callback);
    }

    /// <summary>
    /// 强制上传存档到服务器
    /// </summary>
    public void ForceUploadToServer(System.Action<bool, string> callback = null)
    {
        if (currentSaveData == null)
        {
            callback?.Invoke(false, "没有存档数据");
            return;
        }

        if (hybridSaveManager == null || !IsNetworkMode)
        {
            callback?.Invoke(false, "网络模式未启用");
            return;
        }

        hybridSaveManager.SaveGame(currentSaveData, callback);
    }

    /// <summary>
    /// 强制从服务器下载存档
    /// </summary>
    public void ForceDownloadFromServer(System.Action<bool, string> callback = null)
    {
        if (hybridSaveManager == null || !IsNetworkMode)
        {
            callback?.Invoke(false, "网络模式未启用");
            return;
        }

        hybridSaveManager.LoadGame((success, saveData, message) =>
        {
            if (success && saveData != null)
            {
                currentSaveData = saveData;
                ApplySettings();
            }
            callback?.Invoke(success, message);
        });
    }

    /// <summary>
    /// 切换网络模式
    /// </summary>
    public void SetNetworkModeEnabled(bool enabled)
    {
        enableNetworkMode = enabled;

        if (!enabled && hybridSaveManager != null)
        {
            // 禁用网络模式时，确保保存到本地
            SaveGame(false);
        }

        Debug.Log($"网络模式已{(enabled ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 获取网络连接状态
    /// </summary>
    public bool IsNetworkAvailable()
    {
        if (hybridSaveManager == null) return false;
        return hybridSaveManager.IsNetworkMode();
    }

    /// <summary>
    /// 获取上次同步时间
    /// </summary>
    public System.DateTime GetLastSyncTime()
    {
        if (hybridSaveManager == null) return System.DateTime.MinValue;
        return hybridSaveManager.GetLastSyncTime();
    }

    #endregion

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame(false);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame(false);
    }
}
