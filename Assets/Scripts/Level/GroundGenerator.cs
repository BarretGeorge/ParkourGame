using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 地面生成器 - 生成无限延伸的地面
/// </summary>
public class GroundGenerator : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private LevelConfig levelConfig;

    [Header("玩家引用")]
    [SerializeField] private Transform playerTransform;

    [Header("生成设置")]
    [SerializeField] private bool autoGenerate = true;

    [SerializeField] private bool showDebugInfo = false;

    // 活跃的地面段
    private Queue<GroundSegment> activeSegments = new Queue<GroundSegment>();

    // 当前状态
    private float nextSpawnZPosition = 0f;
    private bool isInitialized = false;

    // 统计
    public int TotalSegmentsSpawned { get; private set; }

    /// <summary>
    /// 地面段数据
    /// </summary>
    private class GroundSegment
    {
        public GameObject instanceObject;
        public float startZPosition;
        public float endZPosition;

        public GroundSegment(GameObject obj, float start, float end)
        {
            instanceObject = obj;
            startZPosition = start;
            endZPosition = end;
        }
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (levelConfig == null || levelConfig.groundPrefab == null)
        {
            Debug.LogError("[GroundGenerator] No ground prefab in LevelConfig!");
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
        if (!isInitialized || !autoGenerate || playerTransform == null) return;

        UpdateGeneration();
    }

    /// <summary>
    /// 开始生成地面
    /// </summary>
    public void StartGeneration()
    {
        if (!isInitialized) return;

        ClearGround();

        // 生成初始地面
        float initialDistance = levelConfig.spawnAheadDistance;
        int initialSegments = Mathf.CeilToInt(initialDistance / levelConfig.groundSegmentLength);

        for (int i = 0; i < initialSegments; i++)
        {
            SpawnNextSegment();
        }

        Debug.Log($"[GroundGenerator] Ground generation started with {initialSegments} segments");
    }

    /// <summary>
    /// 更新生成状态
    /// </summary>
    private void UpdateGeneration()
    {
        float playerZ = playerTransform.position.z;

        // 生成前方的新地面
        float spawnThreshold = nextSpawnZPosition - levelConfig.spawnAheadDistance;
        if (playerZ > spawnThreshold)
        {
            SpawnNextSegment();
        }

        // 销毁后方已通过的地面
        float destroyThreshold = playerZ - levelConfig.destroyBehindDistance;

        while (activeSegments.Count > 0)
        {
            GroundSegment segment = activeSegments.Peek();

            if (segment.endZPosition < destroyThreshold)
            {
                activeSegments.Dequeue();
                Destroy(segment.instanceObject);
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 生成下一个地面段
    /// </summary>
    private void SpawnNextSegment()
    {
        // 计算生成位置
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZPosition);
        Quaternion spawnRotation = Quaternion.identity;

        // 生成地面
        GameObject groundSegment = Instantiate(levelConfig.groundPrefab, spawnPosition, spawnRotation);

        // 设置缩放以匹配配置的宽度和长度
        Vector3 scale = groundSegment.transform.localScale;
        scale.x = levelConfig.groundWidth;
        scale.z = levelConfig.groundSegmentLength;
        groundSegment.transform.localScale = scale;

        // 添加到活跃列表
        float endZ = nextSpawnZPosition + levelConfig.groundSegmentLength;
        GroundSegment segment = new GroundSegment(groundSegment, nextSpawnZPosition, endZ);
        activeSegments.Enqueue(segment);

        // 更新状态
        nextSpawnZPosition += levelConfig.groundSegmentLength;
        TotalSegmentsSpawned++;

        if (showDebugInfo)
        {
            Debug.Log($"[GroundGenerator] Spawned ground segment at {spawnPosition.z}m");
        }
    }

    /// <summary>
    /// 清空所有地面
    /// </summary>
    public void ClearGround()
    {
        while (activeSegments.Count > 0)
        {
            GroundSegment segment = activeSegments.Dequeue();
            if (segment.instanceObject != null)
            {
                Destroy(segment.instanceObject);
            }
        }

        nextSpawnZPosition = 0f;
        TotalSegmentsSpawned = 0;

        Debug.Log("[GroundGenerator] Ground cleared");
    }

    /// <summary>
    /// 重置生成器
    /// </summary>
    public void ResetGenerator()
    {
        ClearGround();

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
    /// 获取当前活跃段数量
    /// </summary>
    public int GetActiveSegmentCount()
    {
        return activeSegments.Count;
    }

#if UNITY_EDITOR
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
        Gizmos.DrawLine(new Vector3(-50, -0.1f, spawnZ), new Vector3(50, -0.1f, spawnZ));

        // 绘制销毁线
        Gizmos.color = Color.red;
        float destroyZ = playerZ - levelConfig.destroyBehindDistance;
        Gizmos.DrawLine(new Vector3(-50, -0.1f, destroyZ), new Vector3(50, -0.1f, destroyZ));

        // 绘制活跃地面段
        Gizmos.color = Color.gray;
        foreach (var segment in activeSegments)
        {
            Vector3 center = new Vector3(0, -0.1f, (segment.startZPosition + segment.endZPosition) * 0.5f);
            Vector3 size = new Vector3(levelConfig.groundWidth, 0.1f, levelConfig.groundSegmentLength);
            Gizmos.DrawWireCube(center, size);
        }
    }
#endif
}
