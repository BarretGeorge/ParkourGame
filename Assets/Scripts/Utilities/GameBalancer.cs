using UnityEngine;

/// <summary>
/// 游戏平衡性调整器 - 用于调整游戏难度和平衡性
/// </summary>
[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Game/Balance Config")]
public class GameBalancer : ScriptableObject
{
    [Header("玩家平衡")]
    [Range(1f, 20f)]
    [SerializeField] private float playerSpeed = 10f;
    [Range(5f, 15f)]
    [SerializeField] private float playerJumpForce = 10f;
    [Range(0.1f, 1f)]
    [SerializeField] private float playerTurnSpeed = 0.3f;

    [Header("难度曲线")]
    [SerializeField] private float difficultyIncreaseRate = 0.1f;
    [SerializeField] private float maxDifficultyMultiplier = 3f;
    [SerializeField] private float distancePerDifficultyLevel = 500f;

    [Header("障碍物设置")]
    [Range(0.1f, 2f)]
    [SerializeField] private float obstacleSpawnRate = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float obstacleClusterChance = 0.3f;
    [SerializeField] private int minObstaclesPerCluster = 2;
    [SerializeField] private int maxObstaclesPerCluster = 5;

    [Header("收集品设置")]
    [Range(0.1f, 5f)]
    [SerializeField] private float coinSpawnRate = 1f;
    [Range(0.1f, 3f)]
    [SerializeField] private float powerUpSpawnRate = 0.5f;
    [SerializeField] private float powerUpDurationBase = 10f;
    [SerializeField] private float powerUpDurationIncrease = 2f;

    [Header("经济平衡")]
    [SerializeField] private int coinValueBase = 1;
    [SerializeField] private int scorePerCoin = 10;
    [SerializeField] private float scorePerMeter = 1f;
    [SerializeField] private float scoreMultiplierBase = 1f;

    [Header("甘地判定（碰撞宽容度）")]
    [Range(0, 5)]
    [SerializeField] private int gracePeriodUses = 3;
    [Range(0.05f, 0.5f)]
    [SerializeField] private float gracePeriodVerticalTolerance = 0.1f;
    [Range(0.1f, 1f)]
    [SerializeField] private float gracePeriodHorizontalTolerance = 0.2f;

    public float GetPlayerSpeed() => playerSpeed;
    public float GetPlayerJumpForce() => playerJumpForce;
    public float GetPlayerTurnSpeed() => playerTurnSpeed;

    public float GetDifficultyMultiplier(float distance)
    {
        float difficultyLevel = distance / distancePerDifficultyLevel;
        float multiplier = 1f + (difficultyLevel * difficultyIncreaseRate);
        return Mathf.Min(multiplier, maxDifficultyMultiplier);
    }

    public float GetObstacleSpawnRate(float distance)
    {
        float difficulty = GetDifficultyMultiplier(distance);
        return obstacleSpawnRate * difficulty;
    }

    public float GetCoinSpawnRate() => coinSpawnRate;
    public float GetPowerUpSpawnRate() => powerUpSpawnRate;

    public float GetPowerUpDuration(int upgradeLevel)
    {
        return powerUpDurationBase + (upgradeLevel * powerUpDurationIncrease);
    }

    public int GetCoinValue(CoinType type)
    {
        switch (type)
        {
            case CoinType.Bronze:
                return coinValueBase;
            case CoinType.Silver:
                return coinValueBase * 5;
            case CoinType.Gold:
                return coinValueBase * 10;
            case CoinType.Platinum:
                return coinValueBase * 50;
            case CoinType.Diamond:
                return coinValueBase * 100;
            default:
                return coinValueBase;
        }
    }

    public int GetScoreForCoin(int coins)
    {
        return coins * scorePerCoin;
    }

    public float GetScoreForDistance(float distance)
    {
        return distance * scorePerMeter;
    }

    public float GetScoreMultiplier(float distance)
    {
        float difficulty = GetDifficultyMultiplier(distance);
        return scoreMultiplierBase * (1f + (difficulty - 1f) * 0.5f);
    }

    public int GetGracePeriodUses() => gracePeriodUses;
    public float GetGracePeriodVerticalTolerance() => gracePeriodVerticalTolerance;
    public float GetGracePeriodHorizontalTolerance() => gracePeriodHorizontalTolerance;

    #region 平衡性预设

    public void SetEasyMode()
    {
        playerSpeed = 8f;
        difficultyIncreaseRate = 0.05f;
        obstacleSpawnRate = 0.7f;
        gracePeriodUses = 5;
        coinSpawnRate = 2f;
        powerUpSpawnRate = 1f;
        Debug.Log("设置为简单模式");
    }

    public void SetNormalMode()
    {
        playerSpeed = 10f;
        difficultyIncreaseRate = 0.1f;
        obstacleSpawnRate = 1f;
        gracePeriodUses = 3;
        coinSpawnRate = 1f;
        powerUpSpawnRate = 0.5f;
        Debug.Log("设置为普通模式");
    }

    public void SetHardMode()
    {
        playerSpeed = 12f;
        difficultyIncreaseRate = 0.15f;
        obstacleSpawnRate = 1.5f;
        gracePeriodUses = 1;
        coinSpawnRate = 0.7f;
        powerUpSpawnRate = 0.3f;
        Debug.Log("设置为困难模式");
    }

    #endregion
}
