using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 关卡生成器 - 无限程序化生成关卡
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private LevelConfig levelConfig;

    [Header("玩家引用")]
    [SerializeField] private Transform playerTransform;

    [Header("生成设置")]
    [SerializeField] private bool autoGenerate = true;

    [SerializeField] private bool showDebugInfo = true;

    // 组件
    private ChunkPool chunkPool;

    // 活跃的关卡块
    private LinkedList<ActiveChunk> activeChunks = new LinkedList<ActiveChunk>();

    // 当前状态
    private float nextSpawnZPosition = 0f;
    private bool isInitialized = false;
    private ChunkType lastChunkType = ChunkType.Normal;

    // 统计
    public int TotalChunksSpawned { get; private set; }
    public float CurrentDifficulty { get; private set; }
    public float TotalDistance => nextSpawnZPosition;

    // 事件
    public System.Action<LevelChunk> OnChunkSpawned;
    public System.Action<LevelChunk> OnChunkDespawned;

    /// <summary>
    /// 活跃关卡块数据
    /// </summary>
    private class ActiveChunk
    {
        public LevelChunk chunkData;
        public GameObject instanceObject;
        public float spawnZPosition;
        public float endZPosition;

        public ActiveChunk(LevelChunk data, GameObject obj, float spawnPos)
        {
            chunkData = data;
            instanceObject = obj;
            spawnZPosition = spawnPos;
            endZPosition = spawnPos + data.chunkLength;
        }
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 获取或创建组件
        if (chunkPool == null)
        {
            chunkPool = GetComponent<ChunkPool>();
            if (chunkPool == null)
            {
                chunkPool = gameObject.AddComponent<ChunkPool>();
            }
        }

        // 设置配置
        if (chunkPool.levelConfig == null && levelConfig != null)
        {
            chunkPool.levelConfig = levelConfig;
        }

        // 验证配置
        if (levelConfig != null && !levelConfig.ValidateConfig())
        {
            Debug.LogError("[LevelGenerator] Invalid level config!");
            return;
        }

        isInitialized = true;
    }

    private void Start()
    {
        if (autoGenerate)
        {
            StartGeneration();
        }
    }

    private void Update()
    {
        if (!isInitialized || !autoGenerate) return;

        UpdateGeneration();
    }

    /// <summary>
    /// 开始生成关卡
    /// </summary>
    public void StartGeneration()
    {
        if (!isInitialized)
        {
            Debug.LogError("[LevelGenerator] Not initialized!");
            return;
        }

        ClearLevel();

        // 生成初始块
        for (int i = 0; i < levelConfig.initialChunkCount; i++)
        {
            SpawnNextChunk();
        }

        Debug.Log($"[LevelGenerator] Level generation started with {levelConfig.initialChunkCount} chunks");
    }

    /// <summary>
    /// 更新生成状态
    /// </summary>
    private void UpdateGeneration()
    {
        if (playerTransform == null) return;

        float playerZ = playerTransform.position.z;

        // 1. 生成前方的新块
        float spawnThreshold = nextSpawnZPosition - levelConfig.spawnAheadDistance;
        if (playerZ > spawnThreshold)
        {
            SpawnNextChunk();
        }

        // 2. 销毁后方已通过的块
        float destroyThreshold = playerZ - levelConfig.destroyBehindDistance;

        var node = activeChunks.First;
        while (node != null)
        {
            var next = node.Next;
            ActiveChunk chunk = node.Value;

            if (chunk.endZPosition < destroyThreshold)
            {
                DespawnChunk(node);
            }

            node = next;
        }
    }

    /// <summary>
    /// 生成下一个块
    /// </summary>
    private void SpawnNextChunk()
    {
        // 计算当前难度
        CurrentDifficulty = levelConfig.GetDifficultyAtDistance(nextSpawnZPosition);

        // 选择合适的块
        LevelChunk chunkData = SelectChunk();

        if (chunkData == null)
        {
            Debug.LogWarning("[LevelGenerator] No suitable chunk found!");
            return;
        }

        // 计算生成位置
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZPosition);
        Quaternion spawnRotation = Quaternion.identity;

        // 从对象池获取
        GameObject chunkObject = chunkPool.GetObject(chunkData, spawnPosition, spawnRotation);

        if (chunkObject == null)
        {
            Debug.LogError($"[LevelGenerator] Failed to spawn chunk: {chunkData.chunkName}");
            return;
        }

        // 生成障碍物和收集品
        PopulateChunk(chunkObject, chunkData);

        // 添加到活跃列表
        ActiveChunk activeChunk = new ActiveChunk(chunkData, chunkObject, nextSpawnZPosition);
        activeChunks.AddLast(activeChunk);

        // 更新状态
        nextSpawnZPosition += chunkData.chunkLength;
        lastChunkType = chunkData.chunkType;
        TotalChunksSpawned++;

        // 触发事件
        OnChunkSpawned?.Invoke(chunkData);

        // Debug log
        if (showDebugInfo)
        {
            Debug.Log($"[LevelGenerator] Spawned '{chunkData.chunkName}' at {spawnPosition.z}m (Difficulty: {CurrentDifficulty:F2})");
        }
    }

    /// <summary>
    /// 选择合适的关卡块
    /// </summary>
    private LevelChunk SelectChunk()
    {
        // 获取随机块
        LevelChunk selectedChunk = levelConfig.GetRandomChunk(CurrentDifficulty, lastChunkType);

        // 验证选择
        if (selectedChunk == null)
        {
            // 回退到第一个可用块
            selectedChunk = levelConfig.availableChunks[0];
        }

        return selectedChunk;
    }

    /// <summary>
    /// 填充关卡块（生成障碍物和收集品）
    /// </summary>
    private void PopulateChunk(GameObject chunkObject, LevelChunk chunkData)
    {
        // 生成障碍物
        foreach (var spawnPoint in chunkData.obstacleSpawnPoints)
        {
            if (Random.value > spawnPoint.spawnProbability && !spawnPoint.mandatory) continue;

            // 计算世界位置
            Vector3 worldPos = chunkObject.transform.position + spawnPoint.localPosition;

            // 应用车道偏移
            if (spawnPoint.laneOffset != 0)
            {
                float laneOffset = spawnPoint.laneOffset * 3f; // 假设车道宽度3米
                worldPos.x += laneOffset;
            }

            // 生成障碍物
            if (spawnPoint.obstaclePrefab != null)
            {
                GameObject obstacle = Instantiate(spawnPoint.obstaclePrefab, worldPos, Quaternion.Euler(spawnPoint.localRotation));
                obstacle.transform.SetParent(chunkObject.transform);
            }
        }

        // 生成收集品
        foreach (var spawnPoint in chunkData.coinSpawnPoints)
        {
            Vector3 worldPos = chunkObject.transform.position + spawnPoint.localPosition;

            // 根据模式生成金币
            SpawnCoins(chunkObject.transform, worldPos, spawnPoint);
        }
    }

    /// <summary>
    /// 生成金币
    /// </summary>
    private void SpawnCoins(Transform parent, Vector3 position, CoinSpawnPoint spawnPoint)
    {
        // TODO: 在Phase 5实现完整收集品系统
        // 这里只是简单创建占位符

        for (int i = 0; i < spawnPoint.coinCount; i++)
        {
            Vector3 coinPos = position;

            switch (spawnPoint.pattern)
            {
                case CoinPattern.Single:
                    // 单个金币，不移动
                    break;

                case CoinPattern.Line:
                    // 直线排列
                    coinPos += Vector3.forward * (i * spawnPoint.spacing);
                    break;

                case CoinPattern.Arc:
                    // 弧线排列
                    float angle = (i / (float)spawnPoint.coinCount) * Mathf.PI;
                    coinPos += new Vector3(Mathf.Sin(angle) * 2f, 0, i * spawnPoint.spacing);
                    break;

                case CoinPattern.Zigzag:
                    // 之字形
                    coinPos += new Vector3((i % 2 == 0 ? 1 : -1) * 1.5f, 0, i * spawnPoint.spacing);
                    break;

                case CoinPattern.Circle:
                    // 圆形排列
                    float circleAngle = (i / (float)spawnPoint.coinCount) * Mathf.PI * 2;
                    coinPos += new Vector3(Mathf.Cos(circleAngle) * 2f, 0, Mathf.Sin(circleAngle) * 2f);
                    break;
            }

            // 创建金币占位符
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.transform.SetParent(parent);
            coin.transform.position = coinPos;
            coin.transform.localScale = Vector3.one * 0.5f;
            coin.tag = "Coin"; // 用于收集检测
            coin.name = $"Coin_{i}";

            // 金币材质（黄色）
            var renderer = coin.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
        }
    }

    /// <summary>
    /// 销毁关卡块
    /// </summary>
    private void DespawnChunk(LinkedListNode<ActiveChunk> chunkNode)
    {
        ActiveChunk chunk = chunkNode.Value;

        // 返回到对象池
        chunkPool.ReturnObject(chunk.chunkData, chunk.instanceObject);

        // 从列表中移除
        activeChunks.Remove(chunkNode);

        // 触发事件
        OnChunkDespawned?.Invoke(chunk.chunkData);

        if (showDebugInfo)
        {
            Debug.Log($"[LevelGenerator] Despawned '{chunk.chunkData.chunkName}' at {chunk.spawnZPosition}m");
        }
    }

    /// <summary>
    /// 清空整个关卡
    /// </summary>
    public void ClearLevel()
    {
        // 销毁所有活跃块
        var node = activeChunks.First;
        while (node != null)
        {
            var next = node.Next;
            DespawnChunk(node);
            node = next;
        }

        // 重置状态
        nextSpawnZPosition = 0f;
        CurrentDifficulty = levelConfig?.initialDifficulty ?? 0f;
        lastChunkType = ChunkType.Normal;
        TotalChunksSpawned = 0;

        Debug.Log("[LevelGenerator] Level cleared");
    }

    /// <summary>
    /// 重置生成器
    /// </summary>
    public void ResetGenerator()
    {
        ClearLevel();

        if (autoGenerate)
        {
            StartGeneration();
        }
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

    /// <summary>
    /// 暂停生成
    /// </summary>
    public void PauseGeneration()
    {
        autoGenerate = false;
    }

    /// <summary>
    /// 恢复生成
    /// </summary>
    public void ResumeGeneration()
    {
        autoGenerate = true;
    }

    /// <summary>
    /// 获取当前活跃块数量
    /// </summary>
    public int GetActiveChunkCount()
    {
        return activeChunks.Count;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 调试信息
    /// </summary>
    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 520, 300, 200));
        GUILayout.Box("Level Generator Debug");

        GUILayout.Label($"Total Spawned: {TotalChunksSpawned}");
        GUILayout.Label($"Active Chunks: {activeChunks.Count}");
        GUILayout.Label($"Distance: {nextSpawnZPosition:F1}m");
        GUILayout.Label($"Difficulty: {CurrentDifficulty:F2}");
        GUILayout.Label($"Last Type: {lastChunkType}");
        GUILayout.Label($"Pool: {chunkPool.TotalPooledObjects} total, {chunkPool.ActiveObjects} active");

        GUILayout.EndArea();
    }

    /// <summary>
    /// 可视化
    /// </summary>
    private void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        float playerZ = playerTransform.position.z;

        // 绘制生成线
        Gizmos.color = Color.green;
        float spawnZ = nextSpawnZPosition - levelConfig.spawnAheadDistance;
        Gizmos.DrawLine(new Vector3(-50, 0, spawnZ), new Vector3(50, 0, spawnZ));

        // 绘制销毁线
        Gizmos.color = Color.red;
        float destroyZ = playerZ - levelConfig.destroyBehindDistance;
        Gizmos.DrawLine(new Vector3(-50, 0, destroyZ), new Vector3(50, 0, destroyZ));

        // 绘制活跃块
        Gizmos.color = Color.cyan;
        foreach (var chunk in activeChunks)
        {
            Vector3 center = new Vector3(0, 0, (chunk.spawnZPosition + chunk.endZPosition) * 0.5f);
            Vector3 size = new Vector3(30, 1, chunk.chunkData.chunkLength);
            Gizmos.DrawWireCube(center, size);
        }
    }
#endif
}
