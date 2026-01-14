using UnityEngine;

/// <summary>
/// 游戏全局设置
/// </summary>
[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("游戏信息")]
    [SerializeField] private string gameVersion = "1.0.0";
    [SerializeField] private string gameName = "3D Runner";
    [SerializeField] private string companyName = "Your Company";

    [Header("帧率设置")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool vSyncEnabled = true;

    [Header("质量设置")]
    [SerializeField] private int defaultQualityLevel = 2; // Medium

    [Header("时间设置")]
    [SerializeField] private float fixedTimeStep = 0.02f;
    [SerializeField] private float maximumAllowedTimestep = 0.1f;

    [Header("物理设置")]
    [SerializeField] private int defaultSolverIterations = 8;
    [SerializeField] private float defaultGravity = -9.81f;

    [Header("输入设置")]
    [SerializeField] private float inputBufferTime = 0.2f;
    [SerializeField] private bool enableInputBuffer = true;

    [Header("调试设置")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool showColliders = false;
    [SerializeField] private bool showGizmos = true;

    [Header("作弊设置（仅开发）")]
    [SerializeField] private bool enableCheats = false;

    public string GameVersion => gameVersion;
    public string GameName => gameName;
    public string CompanyName => companyName;

    public void ApplySettings()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
        QualitySettings.SetQualityLevel(defaultQualityLevel);

        Time.fixedDeltaTime = fixedTimeStep;
        Time.maximumDeltaTime = maximumAllowedTimestep;

        Physics.defaultSolverIterationCount = defaultSolverIterations;
        Physics.gravity = new Vector3(0, defaultGravity, 0);

        Debug.Log($"游戏设置已应用 - 版本: {gameVersion}");
    }

    public void SetQualityLevel(int level)
    {
        defaultQualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(defaultQualityLevel);
        Debug.Log($"质量等级设置为: {QualitySettings.names[defaultQualityLevel]}");
    }

    public void SetTargetFrameRate(int fps)
    {
        targetFrameRate = fps;
        Application.targetFrameRate = targetFrameRate;
    }

    public void EnableVSync(bool enable)
    {
        vSyncEnabled = enable;
        QualitySettings.vSyncCount = enable ? 1 : 0;
    }

    public bool IsDebugMode() => debugMode;
    public bool AreCheatsEnabled() => enableCheats;
}
