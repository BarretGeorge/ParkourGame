using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 障碍物管理器 - 管理场景中所有障碍物
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private LevelConfig levelConfig;

    [SerializeField] private Transform playerTransform;

    [Header("障碍物预制件库")]
    [SerializeField] private List<ObstaclePrefab> obstaclePrefabs = new List<ObstaclePrefab>();

    [Header("生成设置")]
    [SerializeField] private bool autoSpawnObstacles = true;

    [SerializeField] private float spawnCheckInterval = 0.5f;

    // 活跃障碍物
    private List<GameObject> activeObstacles = new List<GameObject>();

    // 掉落障碍物跟踪
    private List<FallingObstacle> fallingObstacles = new List<FallingObstacle>();

    // 统计
    public int TotalObstaclesSpawned { get; private set; }
    public int TotalObstaclesDestroyed { get; private set; }

    // 事件
    public System.Action<Obstacle> OnObstacleSpawned;
    public System.Action<Obstacle> OnObstacleHit;
    public System.Action<Obstacle> OnObstacleDestroyed;

    private float lastSpawnCheckTime = 0f;

    private void Update()
    {
        if (!autoSpawnObstacles || playerTransform == null) return;

        // 定期检查障碍物生成
        if (Time.time - lastSpawnCheckTime >= spawnCheckInterval)
        {
            lastSpawnCheckTime = Time.time;
            CheckAndSpawnObstacles();
        }

        // 更新掉落障碍物
        UpdateFallingObstacles();

        // 清理已销毁的障碍物
        CleanupDestroyedObstacles();
    }

    /// <summary>
    /// 检查并生成障碍物
    /// </summary>
    private void CheckAndSpawnObstacles()
    {
        // 这个方法通常由LevelGenerator调用
        // 这里只是跟踪掉落障碍物
    }

    /// <summary>
    /// 更新掉落障碍物
    /// </summary>
    private void UpdateFallingObstacles()
    {
        if (playerTransform == null) return;

        for (int i = fallingObstacles.Count - 1; i >= 0; i--)
        {
            FallingObstacle fallingObstacle = fallingObstacles[i];

            if (fallingObstacle == null)
            {
                fallingObstacles.RemoveAt(i);
                continue;
            }

            // 检查玩家是否接近
            bool triggered = fallingObstacle.CheckPlayerProximity(playerTransform.position);
            if (triggered)
            {
                Debug.Log("[ObstacleManager] Falling obstacle triggered!");
            }
        }
    }

    /// <summary>
    /// 清理已销毁的障碍物
    /// </summary>
    private void CleanupDestroyedObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 注册障碍物
    /// </summary>
    public void RegisterObstacle(GameObject obstacle)
    {
        if (obstacle == null) return;

        activeObstacles.Add(obstacle);
        TotalObstaclesSpawned++;

        // 检查是否是掉落障碍物
        FallingObstacle fallingObstacle = obstacle.GetComponent<FallingObstacle>();
        if (fallingObstacle != null)
        {
            fallingObstacles.Add(fallingObstacle);
        }

        // 触发事件
        Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
        if (obstacleComponent != null)
        {
            obstacleComponent.OnObstacleHit += (obs) => OnObstacleHit?.Invoke(obs);
            obstacleComponent.OnObstacleDestroyed += (obs) =>
            {
                TotalObstaclesDestroyed++;
                OnObstacleDestroyed?.Invoke(obs);
                UnregisterObstacle(obstacle);
            };

            OnObstacleSpawned?.Invoke(obstacleComponent);
        }
    }

    /// <summary>
    /// 注销障碍物
    /// </summary>
    public void UnregisterObstacle(GameObject obstacle)
    {
        if (obstacle == null) return;

        activeObstacles.Remove(obstacle);

        // 移除掉落障碍物
        FallingObstacle fallingObstacle = obstacle.GetComponent<FallingObstacle>();
        if (fallingObstacle != null)
        {
            fallingObstacles.Remove(fallingObstacle);
        }
    }

    /// <summary>
    /// 根据类型获取障碍物预制件
    /// </summary>
    public GameObject GetObstaclePrefab(ObstacleType type)
    {
        foreach (var prefab in obstaclePrefabs)
        {
            if (prefab.type == type)
            {
                return prefab.prefab;
            }
        }
        return null;
    }

    /// <summary>
    /// 生成随机障碍物
    /// </summary>
    public GameObject SpawnRandomObstacle(Vector3 position, float difficulty)
    {
        if (obstaclePrefabs.Count == 0) return null;

        // 根据难度选择障碍物
        List<ObstaclePrefab> suitableObstacles = new List<ObstaclePrefab>();

        foreach (var prefab in obstaclePrefabs)
        {
            if (prefab.minDifficulty <= difficulty && difficulty <= prefab.maxDifficulty)
            {
                suitableObstacles.Add(prefab);
            }
        }

        if (suitableObstacles.Count == 0)
        {
            suitableObstacles.AddRange(obstaclePrefabs);
        }

        // 加权随机选择
        int totalWeight = 0;
        foreach (var obstacle in suitableObstacles)
        {
            totalWeight += obstacle.spawnWeight;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        ObstaclePrefab selectedPrefab = null;

        foreach (var obstacle in suitableObstacles)
        {
            cumulativeWeight += obstacle.spawnWeight;
            if (randomWeight < cumulativeWeight)
            {
                selectedPrefab = obstacle;
                break;
            }
        }

        if (selectedPrefab == null)
        {
            selectedPrefab = suitableObstacles[0];
        }

        // 生成障碍物
        GameObject obstacle = Instantiate(selectedPrefab.prefab, position, Quaternion.identity);
        RegisterObstacle(obstacle);

        return obstacle;
    }

    /// <summary>
    /// 销毁所有障碍物
    /// </summary>
    public void DestroyAllObstacles()
    {
        foreach (var obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }

        activeObstacles.Clear();
        fallingObstacles.Clear();
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

    /// <summary>
    /// 获取活跃障碍物数量
    /// </summary>
    public int GetActiveObstacleCount()
    {
        return activeObstacles.Count;
    }

    /// <summary>
    /// 获取指定范围内的障碍物
    /// </summary>
    public List<GameObject> GetObstaclesInRange(Vector3 center, float radius)
    {
        List<GameObject> obstaclesInRange = new List<GameObject>();

        foreach (var obstacle in activeObstacles)
        {
            if (obstacle == null) continue;

            float distance = Vector3.Distance(center, obstacle.transform.position);
            if (distance <= radius)
            {
                obstaclesInRange.Add(obstacle);
            }
        }

        return obstaclesInRange;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 调试信息
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 900, 300, 200));
        GUILayout.Box("Obstacle Manager Debug");

        GUILayout.Label($"Active Obstacles: {activeObstacles.Count}");
        GUILayout.Label($"Falling Obstacles: {fallingObstacles.Count}");
        GUILayout.Label($"Total Spawned: {TotalObstaclesSpawned}");
        GUILayout.Label($"Total Destroyed: {TotalObstaclesDestroyed}");

        GUILayout.EndArea();
    }

    /// <summary>
    /// 绘制障碍物范围
    /// </summary>
    private void OnDrawGizmos()
    {
        // 绘制所有障碍物的碰撞盒
        Gizmos.color = Color.red;
        foreach (var obstacle in activeObstacles)
        {
            if (obstacle == null) continue;

            BoxCollider collider = obstacle.GetComponent<BoxCollider>();
            if (collider != null)
            {
                Gizmos.matrix = obstacle.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(collider.center, collider.size);
            }
        }
    }
#endif
}

/// <summary>
/// 障碍物预制件数据
/// </summary>
[System.Serializable]
public class ObstaclePrefab
{
    public string name;
    public ObstacleType type;
    public GameObject prefab;
    public float minDifficulty = 0f;
    public float maxDifficulty = 1f;
    [Range(1, 100)] public int spawnWeight = 10;
}
