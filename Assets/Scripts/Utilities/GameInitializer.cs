using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏初始化器 - 管理游戏的启动和初始化流程
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("初始化设置")]
    [SerializeField] private bool showLoadingScreen = true;
    [SerializeField] private float minimumLoadingTime = 1f;

    [Header("启动场景")]
    [SerializeField] private string firstScene = "MainMenu";

    // 初始化进度
    private float initializationProgress = 0f;
    private bool isInitialized = false;

    // 事件
    public event System.Action<float> OnInitializationProgress;
    public event System.Action OnInitializationComplete;

    private void Awake()
    {
        // 设置为DontDestroyOnLoad以保持跨场景存在
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(InitializeGame());
    }

    private System.Collections.IEnumerator InitializeGame()
    {
        Debug.Log("开始游戏初始化...");

        // 步骤1: 应用游戏设置
        yield return StartCoroutine(ApplyGameSettings());
        initializationProgress = 0.2f;
        OnInitializationProgress?.Invoke(initializationProgress);

        // 步骤2: 加载存档数据
        yield return StartCoroutine(LoadSaveData());
        initializationProgress = 0.4f;
        OnInitializationProgress?.Invoke(initializationProgress);

        // 步骤3: 初始化管理器
        yield return StartCoroutine(InitializeManagers());
        initializationProgress = 0.6f;
        OnInitializationProgress?.Invoke(initializationProgress);

        // 步骤4: 预热对象池
        yield return StartCoroutine(PrewarmObjectPools());
        initializationProgress = 0.8f;
        OnInitializationProgress?.Invoke(initializationProgress);

        // 步骤5: 应用性能设置
        yield return StartCoroutine(ApplyPerformanceSettings());
        initializationProgress = 1f;
        OnInitializationProgress?.Invoke(initializationProgress);

        isInitialized = true;
        OnInitializationComplete?.Invoke();

        Debug.Log("游戏初始化完成");

        // 加载启动场景
        if (SceneManager.GetActiveScene().name != firstScene)
        {
            SceneManager.LoadScene(firstScene);
        }
    }

    private System.Collections.IEnumerator ApplyGameSettings()
    {
        Debug.Log("应用游戏设置...");

        // 查找并应用GameSettings
        GameSettings[] settings = Resources.LoadAll<GameSettings>("");
        if (settings != null && settings.Length > 0)
        {
            settings[0].ApplySettings();
        }

        yield return null;
    }

    private System.Collections.IEnumerator LoadSaveData()
    {
        Debug.Log("加载存档数据...");

        // SaveManager会在Awake中自动加载数据
        yield return new WaitForSeconds(0.1f);
    }

    private System.Collections.IEnumerator InitializeManagers()
    {
        Debug.Log("初始化管理器...");

        // 确保所有单例管理器都已创建
        yield return new WaitForEndOfFrame();
    }

    private System.Collections.IEnumerator PrewarmObjectPools()
    {
        Debug.Log("预热对象池...");

        // 预热常用的对象池
        if (GameObjectPool.Instance != null)
        {
            GameObjectPool.Instance.Prewarm("Coins", 50);
            GameObjectPool.Instance.Prewarm("Particles", 30);
        }

        yield return new WaitForSeconds(0.1f);
    }

    private System.Collections.IEnumerator ApplyPerformanceSettings()
    {
        Debug.Log("应用性能设置...");

        // 应用保存的性能设置
        if (SaveManager.Instance != null)
        {
            SaveData saveData = SaveManager.Instance.GetSaveData();

            QualitySettings.SetQualityLevel(saveData.qualityLevel);
            QualitySettings.vSyncCount = saveData.vSyncEnabled ? 1 : 0;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MasterVolume = saveData.masterVolume;
                AudioManager.Instance.MusicVolume = saveData.musicVolume;
                AudioManager.Instance.SFXVolume = saveData.sfxVolume;
            }
        }

        yield return null;
    }

    /// <summary>
    /// 获取初始化进度
    /// </summary>
    public float GetInitializationProgress()
    {
        return initializationProgress;
    }

    /// <summary>
    /// 检查是否已初始化
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized;
    }

    /// <summary>
    /// 重启游戏
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("重启游戏...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstScene);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("退出游戏...");

        // 保存所有数据
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(false);
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
