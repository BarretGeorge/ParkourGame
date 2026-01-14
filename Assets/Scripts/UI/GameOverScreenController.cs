using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 游戏结束界面控制器
/// </summary>
public class GameOverScreenController : MonoBehaviour
{
    [Header("分数显示")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI coinsCollectedText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("新记录提示")]
    [SerializeField] private GameObject newRecordPanel;
    [SerializeField] private TextMeshProUGUI newRecordText;
    [SerializeField] private Animator newRecordAnimator;

    [Header("操作按钮")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button doubleRewardButton;

    [Header("双倍奖励")]
    [SerializeField] private GameObject doubleRewardPanel;
    [SerializeField] private TextMeshProUGUI rewardMultiplierText;
    [SerializeField] private float doubleRewardDuration = 3f;

    private PlayerController playerController;
    private CollectibleManager collectibleManager;

    private const string HighScoreKey = "HighScore";
    private const string TotalCoinsKey = "TotalCoins";

    private int currentRunScore;
    private int currentRunCoins;
    private float currentRunDistance;
    private bool isNewHighScore;

    private void Start()
    {
        SetupButtons();
        FindManagers();
        HidePanels();
    }

    private void FindManagers()
    {
        playerController = FindObjectOfType<PlayerController>();
        collectibleManager = FindObjectOfType<CollectibleManager>();
    }

    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (doubleRewardButton != null)
        {
            doubleRewardButton.onClick.AddListener(OnDoubleRewardClicked);
        }
    }

    private void HidePanels()
    {
        if (newRecordPanel != null)
        {
            newRecordPanel.SetActive(false);
        }

        if (doubleRewardPanel != null)
        {
            doubleRewardPanel.SetActive(false);
        }
    }

    public void ShowGameOver()
    {
        FindManagers();

        if (playerController != null)
        {
            currentRunScore = (int)playerController.Score;
            currentRunCoins = playerController.CoinsCollected;
            currentRunDistance = playerController.DistanceTraveled;
        }

        CheckHighScore();
        DisplayGameOverStats();
        SaveGameData();
    }

    private void CheckHighScore()
    {
        int previousHighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        isNewHighScore = currentRunScore > previousHighScore;

        if (isNewHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, currentRunScore);
            PlayerPrefs.Save();

            if (newRecordPanel != null)
            {
                newRecordPanel.SetActive(true);
            }

            if (newRecordAnimator != null)
            {
                newRecordAnimator.SetTrigger("ShowNewRecord");
            }
        }
    }

    private void DisplayGameOverStats()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"分数: {currentRunScore}";
        }

        if (distanceText != null)
        {
            distanceText.text = $"距离: {currentRunDistance:F0}m";
        }

        if (coinsCollectedText != null)
        {
            coinsCollectedText.text = $"金币: {currentRunCoins}";
        }

        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (highScoreText != null)
        {
            highScoreText.text = $"最高分: {highScore}";
        }
    }

    private void SaveGameData()
    {
        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        totalCoins += currentRunCoins;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();
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

    private void OnDoubleRewardClicked()
    {
        StartCoroutine(HandleDoubleReward());
    }

    private System.Collections.IEnumerator HandleDoubleReward()
    {
        if (doubleRewardPanel != null)
        {
            doubleRewardPanel.SetActive(true);
        }

        if (rewardMultiplierText != null)
        {
            rewardMultiplierText.text = "x2";
        }

        yield return new WaitForSeconds(doubleRewardDuration);

        int bonusCoins = currentRunCoins;
        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        totalCoins += bonusCoins;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();

        if (coinsCollectedText != null)
        {
            coinsCollectedText.text = $"金币: {currentRunCoins + bonusCoins} (含奖励)";
        }

        yield return new WaitForSeconds(1f);

        if (doubleRewardPanel != null)
        {
            doubleRewardPanel.SetActive(false);
        }

        if (doubleRewardButton != null)
        {
            doubleRewardButton.interactable = false;
        }
    }

    public void ResetUI()
    {
        HidePanels();

        if (doubleRewardButton != null)
        {
            doubleRewardButton.interactable = true;
        }
    }
}
