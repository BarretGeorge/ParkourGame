using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 主菜单控制器
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("菜单按钮")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button charactersButton;
    [SerializeField] private Button quitButton;

    [Header("信息显示")]
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("面板")]
    [SerializeField] private GameObject settingsPanel;

    private const string HighScoreKey = "HighScore";
    private const string TotalCoinsKey = "TotalCoins";

    private void Start()
    {
        SetupButtons();
        DisplayPlayerInfo();
        DisplayVersion();
    }

    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OnShopClicked);
        }

        if (charactersButton != null)
        {
            charactersButton.onClick.AddListener(OnCharactersClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void DisplayPlayerInfo()
    {
        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);

        if (highScoreText != null)
        {
            highScoreText.text = $"最高分: {highScore}";
        }

        if (totalCoinsText != null)
        {
            totalCoinsText.text = $"金币: {totalCoins}";
        }
    }

    private void DisplayVersion()
    {
        if (versionText != null)
        {
            versionText.text = $"v{Application.version}";
        }
    }

    private void OnPlayClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartGame();
        }
    }

    private void OnSettingsClicked()
    {
        ToggleSettingsPanel();
    }

    private void OnShopClicked()
    {
        Debug.Log("商店功能将在Phase 12实现");
    }

    private void OnCharactersClicked()
    {
        Debug.Log("角色选择功能将在Phase 12实现");
    }

    private void OnQuitClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.QuitGame();
        }
    }

    private void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
        }
    }

    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}
