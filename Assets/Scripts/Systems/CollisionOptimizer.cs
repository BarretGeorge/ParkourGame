using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 碰撞优化器 - 优化碰撞检测性能
/// </summary>
public class CollisionOptimizer : MonoBehaviour
{
    [Header("优化设置")]
    [Tooltip("启用空间划分")]
    [SerializeField] private bool enableSpatialPartitioning = true;

    [Tooltip("空间网格大小")]
    [SerializeField] private float gridSize = 10f;

    [Tooltip("启用碰撞剔除")]
    [SerializeField] private bool enableCollisionCulling = true;

    [Tooltip("剔除距离（玩家前方多远开始检测）")]
    [SerializeField] private float cullingDistance = 50f;

    [Tooltip("启用碰撞过滤")]
    [SerializeField] private bool enableCollisionFiltering = true;

    [Header("玩家引用")]
    [SerializeField] private Transform playerTransform;

    // 空间划分
    private Dictionary<Vector2Int, List<Collider>> spatialGrid = new Dictionary<Vector2Int, List<Collider>>();

    // 跟踪的碰撞体
    private List<Collider> trackedColliders = new List<Collider>();

    // 统计
    public int TotalCollidersTracked { get; private set; }
    public int ActiveCollidersCount { get; private set; }
    public int CulledCollidersCount { get; private set; }

    private void Update()
    {
        if (enableSpatialPartitioning)
        {
            UpdateSpatialGrid();
        }

        if (enableCollisionCulling)
        {
            UpdateCulling();
        }
    }

    /// <summary>
    /// 更新空间网格
    /// </summary>
    private void UpdateSpatialGrid()
    {
        spatialGrid.Clear();

        foreach (var collider in trackedColliders)
        {
            if (collider == null) continue;

            Vector2Int gridPos = WorldToGridPosition(collider.transform.position);

            if (!spatialGrid.ContainsKey(gridPos))
            {
                spatialGrid[gridPos] = new List<Collider>();
            }

            spatialGrid[gridPos].Add(collider);
        }
    }

    /// <summary>
    /// 世界坐标转网格坐标
    /// </summary>
    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / gridSize);
        int z = Mathf.FloorToInt(worldPos.z / gridSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// 获取附近的碰撞体
    /// </summary>
    public List<Collider> GetNearbyColliders(Vector3 position, float radius)
    {
        if (!enableSpatialPartitioning)
        {
            // 返回所有碰撞体
            return new List<Collider>(trackedColliders);
        }

        List<Collider> nearbyColliders = new List<Collider>();
        Vector2Int centerGrid = WorldToGridPosition(position);
        int gridRadius = Mathf.CeilToInt(radius / gridSize);

        // 检查周围的网格
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                Vector2Int gridPos = new Vector2Int(centerGrid.x + x, centerGrid.y + z);

                if (spatialGrid.ContainsKey(gridPos))
                {
                    nearbyColliders.AddRange(spatialGrid[gridPos]);
                }
            }
        }

        return nearbyColliders;
    }

    /// <summary>
    /// 更新剔除
    /// </summary>
    private void UpdateCulling()
    {
        if (playerTransform == null) return;

        CulledCollidersCount = 0;
        ActiveCollidersCount = 0;

        float playerZ = playerTransform.position.z;

        foreach (var collider in trackedColliders)
        {
            if (collider == null) continue;

            // 检查是否在检测范围内
            if (collider.transform.position.z > playerZ + cullingDistance)
            {
                // 在前方太远，禁用
                collider.enabled = false;
                CulledCollidersCount++;
            }
            else if (collider.transform.position.z < playerZ - 10f)
            {
                // 在后方太远，禁用
                collider.enabled = false;
                CulledCollidersCount++;
            }
            else
            {
                // 在范围内，启用
                collider.enabled = true;
                ActiveCollidersCount++;
            }
        }
    }

    /// <summary>
    /// 注册碰撞体
    /// </summary>
    public void RegisterCollider(Collider collider)
    {
        if (collider == null) return;

        if (!trackedColliders.Contains(collider))
        {
            trackedColliders.Add(collider);
            TotalCollidersTracked++;
        }
    }

    /// <summary>
    /// 注销碰撞体
    /// </summary>
    public void UnregisterCollider(Collider collider)
    {
        trackedColliders.Remove(collider);
        TotalCollidersTracked = Mathf.Max(0, TotalCollidersTracked - 1);
    }

    /// <summary>
    /// 清除所有跟踪的碰撞体
    /// </summary>
    public void ClearTrackedColliders()
    {
        trackedColliders.Clear();
        spatialGrid.Clear();
        TotalCollidersTracked = 0;
        ActiveCollidersCount = 0;
        CulledCollidersCount = 0;
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制空间网格
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enableSpatialPartitioning || !Application.isPlaying) return;

        Gizmos.color = new Color(0, 1f, 0, 0.3f);

        foreach (var gridPos in spatialGrid.Keys)
        {
            Vector3 worldPos = new Vector3(
                gridPos.x * gridSize,
                0,
                gridPos.y * gridSize
            );

            Gizmos.DrawWireCube(worldPos + Vector3.one * (gridSize * 0.5f), new Vector3(gridSize, 1f, gridSize));
        }
    }

    /// <summary>
    /// 调试信息
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 1750, 300, 200));
        GUILayout.Box("Collision Optimizer Debug");

        GUILayout.Label($"Tracked Colliders: {TotalCollidersTracked}");
        GUILayout.Label($"Active: {ActiveCollidersCount}");
        GUILayout.Label($"Culled: {CulledCollidersCount}");
        GUILayout.Label($"Grid Cells: {spatialGrid.Count}");
        GUILayout.Label($"Grid Size: {gridSize}m");

        GUILayout.EndArea();
    }
#endif
}
