using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 关卡块对象池 - 管理关卡块的复用
/// </summary>
public class ChunkPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public LevelChunk chunkData;
        public Stack<GameObject> pool = new Stack<GameObject>();
        public int spawnedCount = 0;
    }

    [Header("配置")]
    [SerializeField] private LevelConfig levelConfig;

    // 对象池数据
    private Dictionary<LevelChunk, PoolEntry> poolDictionary = new Dictionary<LevelChunk, PoolEntry>();

    // 转换层（隐藏未使用的对象）
    private Transform poolContainer;

    // 统计
    public int TotalPooledObjects { get; private set; }
    public int ActiveObjects { get; private set; }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 创建容器对象
        poolContainer = new GameObject("ChunkPoolContainer").transform;
        poolContainer.SetParent(transform);
        poolContainer.gameObject.SetActive(false);

        // 如果启用了对象池，预创建一些对象
        if (levelConfig != null && levelConfig.useObjectPooling)
        {
            PrePoolObjects();
        }
    }

    /// <summary>
    /// 预创建对象
    /// </summary>
    private void PrePoolObjects()
    {
        if (levelConfig == null || levelConfig.availableChunks.Count == 0) return;

        // 为每个块预创建一些实例
        foreach (var chunk in levelConfig.availableChunks)
        {
            if (chunk == null || chunk.chunkPrefab == null) continue;

            int prePoolCount = Mathf.Min(levelConfig.poolInitialSize, 5); // 每种块预创建5个

            for (int i = 0; i < prePoolCount; i++)
            {
                GameObject obj = Instantiate(chunk.chunkPrefab, poolContainer);
                obj.SetActive(false);
                AddToPool(chunk, obj);
            }
        }

        Debug.Log($"[ChunkPool] Pre-pooled {TotalPooledObjects} objects");
    }

    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    public GameObject GetObject(LevelChunk chunkData, Vector3 position, Quaternion rotation)
    {
        if (chunkData == null || chunkData.chunkPrefab == null)
        {
            Debug.LogError($"[ChunkPool] Invalid chunk data!");
            return null;
        }

        GameObject obj = null;

        // 检查是否使用对象池
        bool usePool = levelConfig != null && levelConfig.useObjectPooling;

        if (usePool)
        {
            // 尝试从池中获取
            obj = GetFromPool(chunkData);
        }

        // 如果池中没有或未启用池，创建新对象
        if (obj == null)
        {
            obj = Instantiate(chunkData.chunkPrefab, position, rotation);
            TotalPooledObjects++;
        }
        else
        {
            // 重置对象状态
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }

        ActiveObjects++;
        return obj;
    }

    /// <summary>
    /// 从池中获取对象
    /// </summary>
    private GameObject GetFromPool(LevelChunk chunkData)
    {
        if (!poolDictionary.ContainsKey(chunkData))
        {
            poolDictionary[chunkData] = new PoolEntry { chunkData = chunkData };
        }

        PoolEntry entry = poolDictionary[chunkData];

        if (entry.pool.Count > 0)
        {
            GameObject obj = entry.pool.Pop();
            return obj;
        }

        return null;
    }

    /// <summary>
    /// 将对象返回到池中
    /// </summary>
    public void ReturnObject(LevelChunk chunkData, GameObject obj)
    {
        if (obj == null) return;

        ActiveObjects--;

        // 检查是否使用对象池
        bool usePool = levelConfig != null && levelConfig.useObjectPooling;
        bool withinPoolLimit = TotalPooledObjects < levelConfig?.poolMaxSize;

        if (usePool && withinPoolLimit)
        {
            // 返回到池中
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);
            AddToPool(chunkData, obj);
        }
        else
        {
            // 销毁对象
            TotalPooledObjects--;
            Destroy(obj);
        }
    }

    /// <summary>
    /// 添加对象到池
    /// </summary>
    private void AddToPool(LevelChunk chunkData, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(chunkData))
        {
            poolDictionary[chunkData] = new PoolEntry { chunkData = chunkData };
        }

        poolDictionary[chunkData].pool.Push(obj);
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearPool()
    {
        foreach (var entry in poolDictionary.Values)
        {
            foreach (var obj in entry.pool)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            entry.pool.Clear();
        }

        poolDictionary.Clear();
        TotalPooledObjects = 0;
        ActiveObjects = 0;

        Debug.Log("[ChunkPool] Pool cleared");
    }

    /// <summary>
    /// 预热对象池
    /// </summary>
    public void WarmPool(LevelChunk chunkData, int count)
    {
        if (chunkData == null || chunkData.chunkPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(chunkData.chunkPrefab, poolContainer);
            obj.SetActive(false);
            AddToPool(chunkData, obj);
            TotalPooledObjects++;
        }

        Debug.Log($"[ChunkPool] Warmed {count} objects for {chunkData.chunkName}");
    }

    /// <summary>
    /// 获取池状态信息
    /// </summary>
    public string GetPoolInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== Chunk Pool Info ===");
        sb.AppendLine($"Total Pooled: {TotalPooledObjects}");
        sb.AppendLine($"Active: {ActiveObjects}");
        sb.AppendLine($"Inactive: {TotalPooledObjects - ActiveObjects}");
        sb.AppendLine($"Pool Entries: {poolDictionary.Count}");

        foreach (var entry in poolDictionary.Values)
        {
            sb.AppendLine($"  {entry.chunkData?.chunkName ?? "Unknown"}: {entry.pool.Count} in pool");
        }

        return sb.ToString();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中调试池状态
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 200, 300));
        GUILayout.Box("Chunk Pool Debug");

        GUILayout.Label($"Total: {TotalPooledObjects}");
        GUILayout.Label($"Active: {ActiveObjects}");
        GUILayout.Label($"Pooled: {TotalPooledObjects - ActiveObjects}");
        GUILayout.Label($"Entries: {poolDictionary.Count}");

        GUILayout.EndArea();
    }
#endif
}
