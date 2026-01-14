using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// UI管理器 - 统一管理所有UI界面
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI面板")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("HUD组件")]
    [SerializeField] private HUDController hudController;

    [Header("游戏数据")]
    [SerializeField] private PlayerController playerController;

    // 当前UI状态
    private UIState currentState = UIState.MainMenu;

    // 单例
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    // 事件
    public event Action OnMainMenuShown;
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameOver;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        if (hudController == null)
        {
            hudController = GetComponentInChildren<HUDController>();
        }

        HideAllPanels();
        ShowMainMenu();
    }

    private void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.OnPlayerDeath += HandlePlayerDeath;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == UIState.Playing)
            {
                PauseGame();
            }
            else if (currentState == UIState.Paused)
            {
                ResumeGame();
            }
        }
    }

    public void ShowMainMenu()
    {
        SetUIState(UIState.MainMenu);
        ShowPanel(mainMenuPanel);
        HidePanel(hudPanel);
        HidePanel(pauseMenuPanel);
        HidePanel(gameOverPanel);

        Time.timeScale = 1f;
        OnMainMenuShown?.Invoke();
    }

    public void StartGame()
    {
        SetUIState(UIState.Playing);
        HidePanel(mainMenuPanel);
        ShowPanel(hudPanel);
        HidePanel(pauseMenuPanel);
        HidePanel(gameOverPanel);

        Time.timeScale = 1f;
        OnGameStarted?.Invoke();

        if (playerController != null)
        {
            playerController.StartGame();
        }
    }

    public void PauseGame()
    {
        if (currentState != UIState.Playing) return;

        SetUIState(UIState.Paused);
        ShowPanel(pauseMenuPanel);

        Time.timeScale = 0f;
        OnGamePaused?.Invoke();

        if (playerController != null)
        {
            playerController.PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (currentState != UIState.Paused) return;

        SetUIState(UIState.Playing);
        HidePanel(pauseMenuPanel);

        Time.timeScale = 1f;
        OnGameResumed?.Invoke();

        if (playerController != null)
        {
            playerController.ResumeGame();
        }
    }

    public void ShowGameOver()
    {
        SetUIState(UIState.GameOver);
        ShowPanel(gameOverPanel);

        Time.timeScale = 1f;
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        if (playerController != null)
        {
            playerController.ResetPlayer();
        }

        StartGame();
    }

    public void ReturnToMainMenu()
    {
        if (playerController != null)
        {
            playerController.ResetPlayer();
        }

        ShowMainMenu();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void HandlePlayerDeath()
    {
        ShowGameOver();
    }

    private void SetUIState(UIState newState)
    {
        currentState = newState;
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(true);
    }

    private void HidePanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(false);
    }

    private void HideAllPanels()
    {
        HidePanel(mainMenuPanel);
        HidePanel(hudPanel);
        HidePanel(pauseMenuPanel);
        HidePanel(gameOverPanel);
    }

    public HUDController GetHUD()
    {
        return hudController;
    }

    public UIState CurrentState => currentState;
    public bool IsGamePlaying => currentState == UIState.Playing;
    public bool IsPaused => currentState == UIState.Paused;
}

public enum UIState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}
