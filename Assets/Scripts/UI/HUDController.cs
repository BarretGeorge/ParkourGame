using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD控制器 - 游戏内界面显示
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("分数显示")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private bool formatScoreWithCommas = true;

    [Header("金币显示")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI coinsMultiplierText;
    [SerializeField] private GameObject coinIcon;

    [Header("速度显示")]
    [SerializeField] private Slider speedBar;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Image speedFillImage;
    [SerializeField] private Gradient speedColorGradient;

    [Header("道具状态")]
    [SerializeField] private GameObject magnetIcon;
    [SerializeField] private GameObject shieldIcon;
    [SerializeField] private GameObject speedBoostIcon;
    [SerializeField] private Image shieldFillImage;
    [SerializeField] private Image magnetFillImage;

    [Header("操作提示")]
    [SerializeField] private TextMeshProUGUI controlsHintText;
    [SerializeField] private GameObject controlsPanel;

    // 引用
    private PlayerController playerController;
    private CollectibleManager collectibleManager;

    // 缓存的数据
    private int lastCoins = -1;
    private float lastScore = -1f;
    private float lastSpeed = -1f;

    private void Start()
    {
        FindManagers();
        HideControlsPanel();
    }

    private void FindManagers()
    {
        playerController = FindObjectOfType<PlayerController>();
        collectibleManager = FindObjectOfType<CollectibleManager>();
    }

    private void Update()
    {
        if (playerController == null) return;

        UpdateScore();
        UpdateCoins();
        UpdateSpeed();
        UpdatePowerUpStatus();
    }

    /// <summary>
    /// 更新分数显示
    /// </summary>
    private void UpdateScore()
    {
        if (scoreText == null && distanceText == null) return;

        float currentScore = playerController.Score;
        float distance = playerController.DistanceTraveled;

        if (Mathf.Abs(currentScore - lastScore) > 1f)
        {
            if (scoreText != null)
            {
                scoreText.text = formatScoreWithCommas
                    ? currentScore.ToString("N0")
                    : currentScore.ToString("F0");
            }
            lastScore = currentScore;
        }

        if (distanceText != null)
        {
            distanceText.text = $"{distance:F0}m";
        }
    }

    /// <summary>
    /// 更新金币显示
    /// </summary>
    private void UpdateCoins()
    {
        if (coinsText == null) return;

        int currentCoins = playerController.CoinsCollected;

        if (currentCoins != lastCoins)
        {
            coinsText.text = currentCoins.ToString();
            lastCoins = currentCoins;

            if (coinIcon != null)
            {
                StartCoroutine(AnimateCoinIcon());
            }
        }

        // 更新分数倍率
        if (coinsMultiplierText != null && collectibleManager != null)
        {
            float multiplier = collectibleManager.CurrentScoreMultiplier;
            if (multiplier > 1f)
            {
                coinsMultiplierText.text = $"x{multiplier:F1}";
                coinsMultiplierText.gameObject.SetActive(true);
            }
            else
            {
                coinsMultiplierText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 更新速度显示
    /// </summary>
    private void UpdateSpeed()
    {
        if (speedBar == null && speedText == null && speedFillImage == null) return;

        float currentSpeed = playerController.CurrentSpeed;
        float maxSpeed = playerController.PlayerData.maxSpeed;
        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);

        if (speedBar != null)
        {
            speedBar.value = speedRatio;
        }

        if (speedText != null)
        {
            speedText.text = $"{currentSpeed:F1} m/s";
        }

        if (speedFillImage != null && speedColorGradient != null)
        {
            speedFillImage.color = speedColorGradient.Evaluate(speedRatio);
        }

        lastSpeed = currentSpeed;
    }

    /// <summary>
    /// 更新道具状态
    /// </summary>
    private void UpdatePowerUpStatus()
    {
        if (collectibleManager == null) return;

        if (magnetIcon != null)
        {
            magnetIcon.SetActive(collectibleManager.HasMagnet);
        }

        if (shieldIcon != null)
        {
            shieldIcon.SetActive(collectibleManager.HasShield);

            ShieldPowerUp shield = FindObjectOfType<ShieldPowerUp>();
            if (shieldFillImage != null && shield != null)
            {
                float strengthRatio = (float)shield.GetCurrentStrength() / 3f;
                shieldFillImage.fillAmount = strengthRatio;
            }
        }

        if (speedBoostIcon != null)
        {
            SpeedBoostPowerUp speedBoost = FindObjectOfType<SpeedBoostPowerUp>();
            bool hasSpeedBoost = speedBoost != null && speedBoost.isActive;
            speedBoostIcon.SetActive(hasSpeedBoost);
        }

        if (magnetFillImage != null && collectibleManager.HasMagnet)
        {
            MagnetPowerUp magnet = FindObjectOfType<MagnetPowerUp>();
            if (magnet != null)
            {
                float progress = 1f - magnet.GetRemainingTime() / 10f;
                magnetFillImage.fillAmount = progress;
            }
        }
    }

    /// <summary>
    /// 显示操作提示
    /// </summary>
    public void ShowControlsPanel()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏操作提示
    /// </summary>
    public void HideControlsPanel()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 金币图标动画
    /// </summary>
    private System.Collections.IEnumerator AnimateCoinIcon()
    {
        if (coinIcon == null) yield break;

        Vector3 originalScale = coinIcon.transform.localScale;

        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.3f, t / 0.2f);
            coinIcon.transform.localScale = originalScale * scale;
            yield return null;
        }

        t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1.3f, 1f, t / 0.2f);
            coinIcon.transform.localScale = originalScale * scale;
            yield return null;
        }

        coinIcon.transform.localScale = originalScale;
    }

    public void SetPlayer(PlayerController player)
    {
        playerController = player;
    }

    public void SetCollectibleManager(CollectibleManager manager)
    {
        collectibleManager = manager;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (speedColorGradient == null)
        {
            speedColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.green, 0f);
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.red, 1f);
            speedColorGradient.colorKeys = colorKeys;

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            speedColorGradient.alphaKeys = alphaKeys;
        }
    }
#endif
}
