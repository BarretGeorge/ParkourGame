using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 关卡块 - 单个地图单元
/// </summary>
[CreateAssetMenu(fileName = "LevelChunk", menuName = "Parkour/Level Chunk")]
public class LevelChunk : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("关卡块名称")]
    public string chunkName;

    [Tooltip("关卡块长度（Z轴）")]
    [Range(10f, 100f)] public float chunkLength = 50f;

    [Tooltip("关卡块难度")]
    [Range(0f, 1f)] public float difficulty = 0.5f;

    [Header("预制件")]
    [Tooltip("关卡块预制件（包含地面、障碍物、收集品等）")]
    public GameObject chunkPrefab;

    [Tooltip("关卡块类型")]
    public ChunkType chunkType = ChunkType.Normal;

    [Header("障碍物布局")]
    [Tooltip("障碍物生成点（局部坐标）")]
    public List<ObstacleSpawnPoint> obstacleSpawnPoints = new List<ObstacleSpawnPoint>();

    [Tooltip("金币生成点（局部坐标）")]
    public List<CoinSpawnPoint> coinSpawnPoints = new List<CoinSpawnPoint>();

    [Header("特殊设置")]
    [Tooltip("是否可以连接相同类型的块")]
    public bool allowSameTypeChain = false;

    [Tooltip("最小出现距离（米）")]
    public float minSpawnDistance = 0f;

    [Tooltip("最大出现距离（米）")]
    public float maxSpawnDistance = 1000f;

    [Tooltip（"权重（越高越容易生成）"）]
    [Range(1, 100)] public int spawnWeight = 10;

    // 额外数据
    [System.NonSerialized]
    public GameObject instantiatedObject;

    [System.NonSerialized]
    public bool isActive = false;

    /// <summary>
    /// 获取世界坐标中的结束位置
    /// </summary>
    public Vector3 GetEndPosition(Vector3 spawnPosition)
    {
        return spawnPosition + Vector3.forward * chunkLength;
    }

    /// <summary>
    /// 计算适应当前难度的权重
    /// </summary>
    public int GetAdjustedWeight(float currentDifficulty)
    {
        // 难度越接近，权重越高
        float difficultyDiff = Mathf.Abs(difficulty - currentDifficulty);
        float difficultyMultiplier = 1f - (difficultyDiff * 0.5f); // 0.5 ~ 1.0
        return Mathf.RoundToInt(spawnWeight * difficultyMultiplier);
    }
}

/// <summary>
/// 关卡块类型
/// </summary>
public enum ChunkType
{
    Normal,      // 普通路段
    Obstacle,    // 障碍密集
    Collectible, // 收集品密集
    Parkour,     // 跑酷挑战
    Transition,  // 过渡路段
    Boss,        // Boss战（预留）
    Tutorial     // 教学（预留）
}

/// <summary>
/// 障碍物生成点
/// </summary>
[System.Serializable]
public class ObstacleSpawnPoint
{
    [Tooltip("局部位置")]
    public Vector3 localPosition;

    [Tooltip("局部旋转")]
    public Vector3 localRotation = Vector3.zero;

    [Tooltip("障碍物预制件")]
    public GameObject obstaclePrefab;

    [Tooltip("生成概率")]
    [Range(0f, 1f)] public float spawnProbability = 1f;

    [Tooltip("车道偏移（-1=左, 0=中, 1=右）")]
    [Range(-1, 1)] public int laneOffset = 0;

    [Tooltip("是否必须生成")]
    public bool mandatory = false;
}

/// <summary>
/// 金币生成点
/// </summary>
[System.Serializable]
public class CoinSpawnPoint
{
    [Tooltip("局部位置")]
    public Vector3 localPosition;

    [Tooltip("金币类型")]
    public CoinType coinType = CoinType.Normal;

    [Tooltip("金币数量")]
    [Range(1, 10)] public int coinCount = 1;

    [Tooltip("排列方式")]
    public CoinPattern pattern = CoinPattern.Single;

    [Tooltip("间隔距离")]
    public float spacing = 2f;
}

/// <summary>
/// 金币类型
/// </summary>
public enum CoinType
{
    Normal,  // 普通金币
    Special, // 特殊金币（分值更高）
    Magnet   // 磁铁道具
}

/// <summary>
/// 金币排列模式
/// </summary>
public enum CoinPattern
{
    Single,      // 单个
    Line,        // 直线
    Arc,         // 弧线
    Zigzag,      // 之字形
    Circle       // 圆形
}
