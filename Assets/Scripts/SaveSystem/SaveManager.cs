using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// 存档管理器 - 负责保存和加载游戏数据
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

    // 自动保存设置
    [Header("自动保存")]
    [SerializeField] private float autoSaveInterval = 30f;
    [SerializeField] private bool enableAutoSave = true;

    // 加密密钥（简单XOR加密）
    private const byte ENCRYPTION_KEY = 0x5A;

    // 事件
    public event System.Action OnSaveCompleted;
    public event System.Action OnLoadCompleted;
    public event System.Action<System.Exception> OnSaveError;

    private float autoSaveTimer;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSavePath();
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
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
                Debug.Log($"游戏已保存 - 金币: {currentSaveData.totalCoins}, 最高分: {currentSaveData.highScore}");
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

                    Debug.Log($"存档加载成功 - 金币: {currentSaveData.totalCoins}, 最高分: {currentSaveData.highScore}");
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
