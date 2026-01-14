using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 关卡配置 - 定义关卡生成的所有参数
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Parkour/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("基本设置")]
    [Tooltip("初始可见块数量")]
    [Range(3, 10)] public int initialChunkCount = 5;

    [Tooltip("最大同时存在块数量")]
    [Range(10, 50)] public int maxActiveChunks = 20;

    [Tooltip("块生成提前量（在玩家到达之前多少米生成）")]
    [Range(50f, 200f)] public float spawnAheadDistance = 100f;

    [Tooltip("块销毁滞后量（在玩家离开之后多少米销毁）")]
    [Range(20f, 100f)] public float destroyBehindDistance = 50f;

    [Header("难度设置")]
    [Tooltip("初始难度")]
    [Range(0f, 1f)] public float initialDifficulty = 0.1f;

    [Tooltip("最大难度")]
    [Range(0f, 1f)] public float maxDifficulty = 1f;

    [Tooltip("难度增长速度（每100米增加的难度）")]
    [Range(0.01f, 0.2f)] public float difficultyGrowthRate = 0.05f;

    [Tooltip("难度变化曲线")]
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("关卡块库")]
    [Tooltip("可用关卡块列表")]
    public List<LevelChunk> availableChunks = new List<LevelChunk>();

    [Tooltip("每个难度段最少块数")]
    [Range(3, 10)] public int minChunksPerDifficulty = 5;

    [Tooltip("相同类型块的最大连续出现次数")]
    [Range(1, 5)] public int maxSameTypeChain = 3;

    [Header("障碍物设置")]
    [Tooltip("障碍物生成概率")]
    [Range(0f, 1f)] public float obstacleSpawnProbability = 0.7f;

    [Tooltip("障碍物密度")]
    [Range(0.1f, 2f)] public float obstacleDensity = 1f;

    [Tooltip("最小障碍物间距")]
    [Range(5f, 30f)] public float minObstacleDistance = 10f;

    [Header("收集品设置")]
    [Tooltip("金币生成概率")]
    [Range(0f, 1f)] public float coinSpawnProbability = 0.8f;

    [Tooltip("金币密度")]
    [Range(0.1f, 3f)] public float coinDensity = 1f;

    [Tooltip("特殊金币出现概率")]
    [Range(0f, 0.5f)] public float specialCoinChance = 0.1f;

    [Header("环境设置")]
    [Tooltip("地面预制件")]
    public GameObject groundPrefab;

    [Tooltip("地面宽度（覆盖所有车道）")]
    [Range(10f, 50f)] public float groundWidth = 30f;

    [Tooltip("地面段长度")]
    [Range(10f, 100f)] public float groundSegmentLength = 50f;

    [Header("性能优化")]
    [Tooltip("使用对象池")]
    public bool useObjectPooling = true;

    [Tooltip("对象池初始大小")]
    [Range(5, 20)] public int poolInitialSize = 10;

    [Tooltip("对象池最大大小")]
    [Range(20, 100)] public int poolMaxSize = 50;

    /// <summary>
    /// 根据距离计算难度
    /// </summary>
    public float GetDifficultyAtDistance(float distance)
    {
        // 归一化距离（假设每1000米达到最大难度）
        float normalizedDistance = Mathf.Clamp01(distance / 1000f);

        // 应用曲线
        float curveValue = difficultyCurve.Evaluate(normalizedDistance);

        // 计算难度
        float difficulty = initialDifficulty + (maxDifficulty - initialDifficulty) * curveValue;

        return Mathf.Clamp(difficulty, initialDifficulty, maxDifficulty);
    }

    /// <summary>
    /// 获取指定难度的可用块
    /// </summary>
    public List<LevelChunk> GetChunksForDifficulty(float targetDifficulty)
    {
        List<LevelChunk> validChunks = new List<LevelChunk>();
        float tolerance = 0.2f; // 难度容差

        foreach (var chunk in availableChunks)
        {
            if (Mathf.Abs(chunk.difficulty - targetDifficulty) <= tolerance)
            {
                validChunks.Add(chunk);
            }
        }

        return validChunks;
    }

    /// <summary>
    /// 获取随机块（权重基于难度匹配度）
    /// </summary>
    public LevelChunk GetRandomChunk(float targetDifficulty, ChunkType lastType = ChunkType.Normal)
    {
        if (availableChunks.Count == 0) return null;

        // 构建权重列表
        List<int> weights = new List<int>();
        int totalWeight = 0;

        for (int i = 0; i < availableChunks.Count; i++)
        {
            LevelChunk chunk = availableChunks[i];

            // 检查类型限制
            if (!chunk.allowSameTypeChain && chunk.chunkType == lastType)
            {
                weights.Add(0);
                continue;
            }

            // 计算权重
            int weight = chunk.GetAdjustedWeight(targetDifficulty);
            weights.Add(weight);
            totalWeight += weight;
        }

        if (totalWeight == 0) return availableChunks[Random.Range(0, availableChunks.Count)];

        // 加权随机选择
        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < availableChunks.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue < cumulativeWeight)
            {
                return availableChunks[i];
            }
        }

        return availableChunks[0];
    }

    /// <summary>
    /// 验证配置有效性
    /// </summary>
    public bool ValidateConfig()
    {
        if (availableChunks.Count == 0)
        {
            Debug.LogError("LevelConfig: No chunks available!");
            return false;
        }

        if (groundPrefab == null)
        {
            Debug.LogError("LevelConfig: No ground prefab assigned!");
            return false;
        }

        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中预览难度曲线
    /// </summary>
    public void PreviewDifficultyCurve()
    {
        Debug.Log("Difficulty Curve Preview:");
        for (int i = 0; i <= 10; i++)
        {
            float distance = i * 100f;
            float difficulty = GetDifficultyAtDistance(distance);
            Debug.Log($"Distance: {distance}m, Difficulty: {difficulty:F2}");
        }
    }
#endif
}
