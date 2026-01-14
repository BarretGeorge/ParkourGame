using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameObject对象池
/// </summary>
public class GameObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public string poolName;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 100;
        public bool expandable = true;
    }

    [Header("对象池配置")]
    [SerializeField] private List<PoolEntry> poolEntries = new List<PoolEntry>();

    [Header("调试")]
    [SerializeField] private bool showDebugInfo = false;

    // 对象池字典
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, PoolEntry> poolConfigs = new Dictionary<string, PoolEntry>();
    private Dictionary<string, Transform> poolContainers = new Dictionary<string, Transform>();

    // 统计
    private Dictionary<string, int> activeCounts = new Dictionary<string, int>();

    // 单例
    private static GameObjectPool _instance;
    public static GameObjectPool Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        foreach (var entry in poolEntries)
        {
            if (entry.prefab != null && !string.IsNullOrEmpty(entry.poolName))
            {
                CreatePool(entry);
            }
        }
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    public void CreatePool(string poolName, GameObject prefab, int initialSize, int maxSize = 100, bool expandable = true)
    {
        PoolEntry entry = new PoolEntry
        {
            poolName = poolName,
            prefab = prefab,
            initialSize = initialSize,
            maxSize = maxSize,
            expandable = expandable
        };

        CreatePool(entry);
    }

    private void CreatePool(PoolEntry entry)
    {
        if (pools.ContainsKey(entry.poolName))
        {
            Debug.LogWarning($"对象池 {entry.poolName} 已存在");
            return;
        }

        // 创建容器
        GameObject container = new GameObject($"Pool_{entry.poolName}");
        container.transform.SetParent(transform);

        Queue<GameObject> queue = new Queue<GameObject>();
        poolConfigs[entry.poolName] = entry;
        poolContainers[entry.poolName] = container.transform;
        activeCounts[entry.poolName] = 0;

        // 预创建对象
        for (int i = 0; i < entry.initialSize; i++)
        {
            GameObject obj = Instantiate(entry.prefab, container.transform);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }

        pools[entry.poolName] = queue;

        if (showDebugInfo)
        {
            Debug.Log($"创建对象池: {entry.poolName}, 初始大小: {entry.initialSize}");
        }
    }

    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在");
            return null;
        }

        Queue<GameObject> queue = pools[poolName];
        PoolEntry config = poolConfigs[poolName];
        GameObject obj;

        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else if (config.expandable)
        {
            obj = Instantiate(config.prefab, poolContainers[poolName].transform);

            if (showDebugInfo)
            {
                Debug.Log($"扩展对象池: {poolName}, 当前大小: {queue.Count + 1}");
            }
        }
        else
        {
            Debug.LogWarning($"对象池 {poolName} 已满且不可扩展");
            return null;
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        activeCounts[poolName]++;

        return obj;
    }

    /// <summary>
    /// 从对象池获取对象（使用默认位置和旋转）
    /// </summary>
    public GameObject Spawn(string poolName)
    {
        return Spawn(poolName, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 从对象池获取对象（使用父对象）
    /// </summary>
    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject obj = Spawn(poolName, position, rotation);
        if (obj != null && parent != null)
        {
            obj.transform.SetParent(parent);
        }
        return obj;
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        // 从对象名称提取池名称（假设名称格式为 "PoolName(Clone)"）
        string objName = obj.name.Replace("(Clone)", "");

        // 找到对应的对象池
        string poolName = null;
        foreach (var kvp in poolConfigs)
        {
            if (kvp.Value.prefab.name == objName)
            {
                poolName = kvp.Key;
                break;
            }
        }

        if (string.IsNullOrEmpty(poolName) || !pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"找不到对象 {obj.name} 对应的对象池，直接销毁");
            Destroy(obj);
            return;
        }

        Queue<GameObject> queue = pools[poolName];
        PoolEntry config = poolConfigs[poolName];

        if (queue.Count >= config.maxSize)
        {
            // 对象池已满，直接销毁
            Destroy(obj);
        }
        else
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolContainers[poolName].transform);
            queue.Enqueue(obj);
        }

        activeCounts[poolName]--;
    }

    /// <summary>
    /// 延迟回收对象
    /// </summary>
    public void Despawn(GameObject obj, float delay)
    {
        if (obj != null)
        {
            StartCoroutine(DespawnCoroutine(obj, delay));
        }
    }

    private System.Collections.IEnumerator DespawnCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(obj);
    }

    /// <summary>
    /// 预热对象池
    /// </summary>
    public void Prewarm(string poolName, int count)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"对象池 {poolName} 不存在");
            return;
        }

        Queue<GameObject> queue = pools[poolName];
        PoolEntry config = poolConfigs[poolName];

        for (int i = 0; i < count; i++)
        {
            if (queue.Count >= config.maxSize) break;

            GameObject obj = Instantiate(config.prefab, poolContainers[poolName].transform);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }

        if (showDebugInfo)
        {
            Debug.Log($"预热对象池: {poolName}, 预热数量: {count}");
        }
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearPool(string poolName)
    {
        if (!pools.ContainsKey(poolName)) return;

        Queue<GameObject> queue = pools[poolName];

        while (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        pools.Remove(poolName);
        poolConfigs.Remove(poolName);
        activeCounts.Remove(poolName);

        if (poolContainers.ContainsKey(poolName))
        {
            if (poolContainers[poolName] != null)
            {
                Destroy(poolContainers[poolName].gameObject);
            }
            poolContainers.Remove(poolName);
        }
    }

    /// <summary>
    /// 清空所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        List<string> poolNames = new List<string>(pools.Keys);
        foreach (string poolName in poolNames)
        {
            ClearPool(poolName);
        }
    }

    /// <summary>
    /// 获取对象池统计信息
    /// </summary>
    public PoolStats GetPoolStats(string poolName)
    {
        if (!pools.ContainsKey(poolName))
        {
            return new PoolStats();
        }

        return new PoolStats
        {
            poolName = poolName,
            totalObjects = pools[poolName].Count + activeCounts[poolName],
            activeObjects = activeCounts[poolName],
            inactiveObjects = pools[poolName].Count
        };
    }

    public Dictionary<string, PoolStats> GetAllPoolStats()
    {
        Dictionary<string, PoolStats> stats = new Dictionary<string, PoolStats>();

        foreach (string poolName in pools.Keys)
        {
            stats[poolName] = GetPoolStats(poolName);
        }

        return stats;
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.BeginVertical("Box");
        GUILayout.Label("对象池统计");

        foreach (string poolName in pools.Keys)
        {
            PoolStats stats = GetPoolStats(poolName);
            GUILayout.Label($"{poolName}: {stats.activeObjects}/{stats.totalObjects}");
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}

[System.Serializable]
public class PoolStats
{
    public string poolName;
    public int totalObjects;
    public int activeObjects;
    public int inactiveObjects;
}
