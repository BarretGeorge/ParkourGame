using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 收集品管理器 - 管理所有收集品的生成和交互
/// </summary>
public class CollectibleManager : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private LevelConfig levelConfig;

    [Header("收集品预制件库")]
    [SerializeField] private List<CollectiblePrefab> collectiblePrefabs = new List<CollectiblePrefab>();

    [Header("生成设置")]
    [SerializeField] private bool autoSpawnCollectibles = true;

    [SerializeField] private float spawnCheckInterval = 1f;

    [Header("玩家引用")]
    [SerializeField] private Transform playerTransform;

    // 活跃收集品
    private List<Collectible> activeCollectibles = new List<Collectible>();

    // 活跃能量道具
    private List<PowerUp> activePowerUps = new List<PowerUp>();

    // 统计
    public int TotalCollectiblesSpawned { get; private set; }
    public int TotalCoinsCollected { get; private set; }
    public int TotalPowerUpsCollected { get; private set; }

    // 当前效果
    public float CurrentScoreMultiplier { get; private set; } = 1f;
    public bool HasMagnet { get; private set; } = false;
    public bool HasShield { get; private set; } = false;

    // 事件
    public System.Action<Collectible> OnCollectibleSpawned;
    public System.Action<Collectible> OnCollectibleCollected;
    public System.Action<PowerUp> OnPowerUpActivated;
    public System.Action<PowerUp> OnPowerUpDeactivated;

    private float lastSpawnCheckTime = 0f;

    private void Update()
    {
        // 更新活跃能量道具
        UpdateActivePowerUps();

        // 定期检查收集品生成
        if (autoSpawnCollectibles && Time.time - lastSpawnCheckTime >= spawnCheckInterval)
        {
            lastSpawnCheckTime = Time.time;
            CheckAndSpawnCollectibles();
        }

        // 清理已收集的物品
        CleanupCollectedItems();
    }

    /// <summary>
    /// 检查并生成收集品
    /// </summary>
    private void CheckAndSpawnCollectibles()
    {
        // 这个方法通常由LevelGenerator调用
        // 这里主要更新能量道具状态
    }

    /// <summary>
    /// 更新活跃能量道具
    /// </summary>
    private void UpdateActivePowerUps()
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            PowerUp powerUp = activePowerUps[i];

            if (powerUp == null)
            {
                activePowerUps.RemoveAt(i);
                continue;
            }

            // 更新道具效果
            if (powerUp.isActive)
            {
                // 这里可以添加特定道具的额外逻辑
                if (powerUp is MagnetPowerUp magnet)
                {
                    HasMagnet = true;
                }
                else if (powerUp is ShieldPowerUp shield)
                {
                    HasShield = shield.GetCurrentStrength() > 0;
                }
            }
            else
            {
                activePowerUps.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 清理已收集的物品
    /// </summary>
    private void CleanupCollectedItems()
    {
        for (int i = activeCollectibles.Count - 1; i >= 0; i--)
        {
            if (activeCollectibles[i] == null)
            {
                activeCollectibles.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 注册收集品
    /// </summary>
    public void RegisterCollectible(Collectible collectible)
    {
        if (collectible == null) return;

        activeCollectibles.Add(collectible);
        TotalCollectiblesSpawned++;

        // 设置玩家引用
        if (playerTransform != null)
        {
            collectible.SetPlayer(playerTransform);
        }

        // 订阅事件
        collectible.OnCollected += HandleCollectibleCollected;

        // 触发事件
        OnCollectibleSpawned?.Invoke(collectible);
    }

    /// <summary>
    /// 处理收集品被收集
    /// </summary>
    private void HandleCollectibleCollected(Collectible collectible)
    {
        TotalCoinsCollected += collectible.Value;

        // 检查是否是能量道具
        if (collectible is PowerUp powerUp)
        {
            TotalPowerUpsCollected++;
            activePowerUps.Add(powerUp);

            // 订阅道具事件
            powerUp.OnPowerUpActivated += HandlePowerUpActivated;
            powerUp.OnPowerUpDeactivated += HandlePowerUpDeactivated;
        }

        OnCollectibleCollected?.Invoke(collectible);
    }

    /// <summary>
    /// 处理能量道具激活
    /// </summary>
    private void HandlePowerUpActivated(PowerUp powerUp)
    {
        OnPowerUpActivated?.Invoke(powerUp);

        // 更新当前效果
        if (powerUp is MagnetPowerUp)
        {
            HasMagnet = true;
        }
        else if (powerUp is ShieldPowerUp)
        {
            HasShield = true;
        }
    }

    /// <summary>
    /// 处理能量道具停用
    /// </summary>
    private void HandlePowerUpDeactivated(PowerUp powerUp)
    {
        OnPowerUpDeactivated?.Invoke(powerUp);

        // 更新当前效果
        if (powerUp is MagnetPowerUp)
        {
            HasMagnet = false;
        }
        else if (powerUp is ShieldPowerUp)
        {
            HasShield = false;
        }

        activePowerUps.Remove(powerUp);
    }

    /// <summary>
    /// 根据类型获取收集品预制件
    /// </summary>
    public GameObject GetCollectiblePrefab(CollectibleType type)
    {
        foreach (var prefab in collectiblePrefabs)
        {
            if (prefab.type == type)
            {
                return prefab.prefab;
            }
        }
        return null;
    }

    /// <summary>
    /// 生成随机收集品
    /// </summary>
    public Collectible SpawnRandomCollectible(Vector3 position, float difficulty)
    {
        if (collectiblePrefabs.Count == 0) return null;

        // 根据难度选择收集品
        List<CollectiblePrefab> suitableCollectibles = new List<CollectiblePrefab>();

        foreach (var prefab in collectiblePrefabs)
        {
            if (prefab.minDifficulty <= difficulty && difficulty <= prefab.maxDifficulty)
            {
                suitableCollectibles.Add(prefab);
            }
        }

        if (suitableCollectibles.Count == 0)
        {
            // 使用金币作为默认
            var coinPrefab = collectiblePrefabs.Find(p => p.type == CollectibleType.Coin);
            if (coinPrefab != null)
            {
                suitableCollectibles.Add(coinPrefab);
            }
            else
            {
                suitableCollectibles.AddRange(collectiblePrefabs);
            }
        }

        // 加权随机选择
        int totalWeight = 0;
        foreach (var collectible in suitableCollectibles)
        {
            totalWeight += collectible.spawnWeight;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        CollectiblePrefab selectedPrefab = null;

        foreach (var collectible in suitableCollectibles)
        {
            cumulativeWeight += collectible.spawnWeight;
            if (randomWeight < cumulativeWeight)
            {
                selectedPrefab = collectible;
                break;
            }
        }

        if (selectedPrefab == null)
        {
            selectedPrefab = suitableCollectibles[0];
        }

        // 生成收集品
        GameObject collectibleObj = Instantiate(selectedPrefab.prefab, position, Quaternion.identity);
        Collectible collectible = collectibleObj.GetComponent<Collectible>();

        if (collectible != null)
        {
            RegisterCollectible(collectible);
        }

        return collectible;
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Transform player)
    {
        playerTransform = player;

        // 更新所有活跃收集品的玩家引用
        foreach (var collectible in activeCollectibles)
        {
            if (collectible != null)
            {
                collectible.SetPlayer(player);
            }
        }
    }

    /// <summary>
    /// 销毁所有收集品
    /// </summary>
    public void DestroyAllCollectibles()
    {
        foreach (var collectible in activeCollectibles)
        {
            if (collectible != null)
            {
                Destroy(collectible.gameObject);
            }
        }

        activeCollectibles.Clear();
        activePowerUps.Clear();
    }

    /// <summary>
    /// 获取活跃收集品数量
    /// </summary>
    public int GetActiveCollectibleCount()
    {
        return activeCollectibles.Count;
    }

    /// <summary>
    /// 获取指定范围内的收集品
    /// </summary>
    public List<Collectible> GetCollectiblesInRange(Vector3 center, float radius)
    {
        List<Collectible> collectiblesInRange = new List<Collectible>();

        foreach (var collectible in activeCollectibles)
        {
            if (collectible == null || collectible.IsCollected) continue;

            float distance = Vector3.Distance(center, collectible.transform.position);
            if (distance <= radius)
            {
                collectiblesInRange.Add(collectible);
            }
        }

        return collectiblesInRange;
    }

    /// <summary>
    /// 设置分数倍率
    /// </summary>
    public void SetScoreMultiplier(float multiplier)
    {
        CurrentScoreMultiplier = multiplier;
        Debug.Log($"[CollectibleManager] Score multiplier set to {multiplier}x");
    }

#if UNITY_EDITOR
    /// <summary>
    /// 调试信息
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 1100, 300, 250));
        GUILayout.Box("Collectible Manager Debug");

        GUILayout.Label($"Active Collectibles: {activeCollectibles.Count}");
        GUILayout.Label($"Active PowerUps: {activePowerUps.Count}");
        GUILayout.Label($"Total Spawned: {TotalCollectiblesSpawned}");
        GUILayout.Label($"Total Coins: {TotalCoinsCollected}");
        GUILayout.Label($"Total PowerUps: {TotalPowerUpsCollected}");
        GUILayout.Label($"Score Multiplier: {CurrentScoreMultiplier}x");
        GUILayout.Label($"Has Magnet: {HasMagnet}");
        GUILayout.Label($"Has Shield: {HasShield}");

        GUILayout.EndArea();
    }

    /// <summary>
    /// 绘制收集品范围
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var collectible in activeCollectibles)
        {
            if (collectible == null || collectible.IsCollected) continue;

            Gizmos.DrawWireSphere(collectible.transform.position, 0.5f);
        }
    }
#endif
}

/// <summary>
/// 收集品预制件数据
/// </summary>
[System.Serializable]
public class CollectiblePrefab
{
    public string name;
    public CollectibleType type;
    public GameObject prefab;
    public float minDifficulty = 0f;
    public float maxDifficulty = 1f;
    [Range(1, 100)] public int spawnWeight = 10;
}
