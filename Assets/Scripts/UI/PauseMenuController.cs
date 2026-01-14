using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 暂停菜单控制器
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("菜单按钮")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("暂停信息")]
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI pauseTitleText;

    [Header("面板")]
    [SerializeField] private GameObject pausePanel;

    private PlayerController playerController;

    private void Start()
    {
        SetupButtons();
        FindPlayer();
    }

    private void FindPlayer()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void SetupButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void OnResumeClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ResumeGame();
        }
    }

    private void OnRestartClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RestartGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ReturnToMainMenu();
        }
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        UpdateCurrentScore();
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void UpdateCurrentScore()
    {
        if (currentScoreText != null && playerController != null)
        {
            currentScoreText.text = $"当前分数: {playerController.Score:F0}";
        }
    }
}
