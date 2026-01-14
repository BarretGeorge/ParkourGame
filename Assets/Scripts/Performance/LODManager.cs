using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// LOD (Level of Detail) 管理器
/// </summary>
public class LODManager : MonoBehaviour
{
    [System.Serializable]
    public class LODGroup
    {
        public string groupName;
        public List<GameObject> highDetailObjects = new List<GameObject>();
        public List<GameObject> mediumDetailObjects = new List<GameObject>();
        public List<GameObject> lowDetailObjects = new List<GameObject>();
    }

    [Header("LOD设置")]
    [SerializeField] private List<LODGroup> lodGroups = new List<LODGroup>();

    [Header("距离设置")]
    [SerializeField] private float highDetailDistance = 50f;
    [SerializeField] private float mediumDetailDistance = 100f;
    [SerializeField] private float lowDetailDistance = 200f;

    [Header("更新频率")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("相机")]
    [SerializeField] private bool useMainCamera = true;
    [SerializeField] private Camera targetCamera;

    private Transform cameraTransform;
    private float updateTimer;

    // 当前LOD状态
    private Dictionary<GameObject, int> currentLODStates = new Dictionary<GameObject, int>();

    private void Start()
    {
        if (useMainCamera)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            cameraTransform = targetCamera.transform;
        }

        InitializeLODGroups();
    }

    private void InitializeLODGroups()
    {
        foreach (var group in lodGroups)
        {
            // 初始时只显示高细节对象
            SetLODLevel(group, 0);
        }
    }

    private void Update()
    {
        if (cameraTransform == null) return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateLODs();
        }
    }

    private void UpdateLODs()
    {
        foreach (var group in lodGroups)
        {
            foreach (var obj in group.highDetailObjects)
            {
                if (obj != null)
                {
                    UpdateObjectLOD(obj, group, highDetailDistance);
                }
            }

            foreach (var obj in group.mediumDetailObjects)
            {
                if (obj != null)
                {
                    UpdateObjectLOD(obj, group, mediumDetailDistance);
                }
            }

            foreach (var obj in group.lowDetailObjects)
            {
                if (obj != null)
                {
                    UpdateObjectLOD(obj, group, lowDetailDistance);
                }
            }
        }
    }

    private void UpdateObjectLOD(GameObject obj, LODGroup group, float maxDistance)
    {
        float distance = Vector3.Distance(obj.transform.position, cameraTransform.position);

        if (distance > maxDistance)
        {
            // 超出最大距离，隐藏对象
            if (obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            // 在距离内，根据LOD级别显示
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
            }

            // 更新LOD级别
            int lodLevel = CalculateLODLevel(distance);
            SetObjectLOD(obj, lodLevel);
        }
    }

    private int CalculateLODLevel(float distance)
    {
        if (distance < highDetailDistance)
        {
            return 0; // 高细节
        }
        else if (distance < mediumDetailDistance)
        {
            return 1; // 中细节
        }
        else
        {
            return 2; // 低细节
        }
    }

    private void SetLODLevel(LODGroup group, int level)
    {
        // 隐藏所有对象
        foreach (var obj in group.highDetailObjects)
        {
            if (obj != null) obj.SetActive(level == 0);
        }

        foreach (var obj in group.mediumDetailObjects)
        {
            if (obj != null) obj.SetActive(level == 1);
        }

        foreach (var obj in group.lowDetailObjects)
        {
            if (obj != null) obj.SetActive(level == 2);
        }
    }

    private void SetObjectLOD(GameObject obj, int level)
    {
        // 可以在这里实现更复杂的LOD逻辑
        // 例如切换材质、减少粒子数量等

        // 简单实现：根据LOD级别调整对象
        currentLODStates[obj] = level;
    }

    /// <summary>
    /// 添加LOD组
    /// </summary>
    public void AddLODGroup(string groupName, List<GameObject> high, List<GameObject> medium, List<GameObject> low)
    {
        LODGroup group = new LODGroup
        {
            groupName = groupName,
            highDetailObjects = high,
            mediumDetailObjects = medium,
            lowDetailObjects = low
        };

        lodGroups.Add(group);
    }

    /// <summary>
    /// 设置LOD距离
    /// </summary>
    public void SetLODDistances(float high, float medium, float low)
    {
        highDetailDistance = high;
        mediumDetailDistance = medium;
        lowDetailDistance = low;
    }

    /// <summary>
    /// 获取当前LOD级别
    /// </summary>
    public int GetObjectLOD(GameObject obj)
    {
        if (currentLODStates.ContainsKey(obj))
        {
            return currentLODStates[obj];
        }
        return 0;
    }
}
