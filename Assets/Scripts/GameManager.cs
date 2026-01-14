using UnityEngine;

/// <summary>
/// 游戏主管理器 - 负责游戏的核心逻辑
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("游戏设置")]
    [SerializeField] private float gameSpeed = 1.0f;

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    void Awake()
    {
        // 单例模式
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("游戏初始化成功！");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 游戏开始时的初始化逻辑
        InitializeGame();
    }

    void Update()
    {
        // 游戏主循环
        HandleInput();
    }

    private void InitializeGame()
    {
        Debug.Log($"游戏速度: {gameSpeed}");
        // 在这里添加你的初始化代码
    }

    private void HandleInput()
    {
        // 处理输入
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("游戏暂停");
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        Debug.Log("游戏开始！");
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("游戏已暂停");
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("游戏继续");
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public void GameOver()
    {
        Debug.Log("游戏结束");
        Time.timeScale = 1f;
    }
}
