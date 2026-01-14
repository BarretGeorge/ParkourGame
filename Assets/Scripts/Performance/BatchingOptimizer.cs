using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 批处理优化器 - 自动合并静态几何体
/// </summary>
public class BatchingOptimizer : MonoBehaviour
{
    [Header("批处理设置")]
    [SerializeField] private bool enableStaticBatching = true;
    [SerializeField] private bool enableDynamicBatching = true;
    [SerializeField] private int maxVerticesPerBatch = 30000;

    [Header("自动优化")]
    [SerializeField] private bool optimizeOnStart = false;
    [SerializeField] private float optimizeInterval = 5f;

    [Header("优化目标")]
    [SerializeField] private string[] targetTags = { "Environment", "Obstacle", "Decor" };

    private float optimizeTimer;

    private void Start()
    {
        if (enableDynamicBatching)
        {
            EnableDynamicBatching();
        }

        if (optimizeOnStart)
        {
            OptimizeScene();
        }
    }

    private void Update()
    {
        if (!optimizeOnStart)
        {
            optimizeTimer += Time.deltaTime;

            if (optimizeTimer >= optimizeInterval)
            {
                optimizeTimer = 0f;
                OptimizeScene();
            }
        }
    }

    /// <summary>
    /// 启用动态批处理
    /// </summary>
    private void EnableDynamicBatching()
    {
        // Unity 2020.3+ 不需要在代码中启用动态批处理
        // 可以在Player Settings中设置
        Debug.Log("动态批处理已启用（在Player Settings中配置）");
    }

    /// <summary>
    /// 优化场景
    /// </summary>
    public void OptimizeScene()
    {
        if (!enableStaticBatching) return;

        foreach (string tag in targetTags)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

            if (taggedObjects.Length > 0)
            {
                // 按材质分组
                Dictionary<Material, List<MeshFilter>> materialGroups = new Dictionary<Material, List<MeshFilter>>();

                foreach (GameObject obj in taggedObjects)
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    MeshFilter filter = obj.GetComponent<MeshFilter>();

                    if (renderer != null && filter != null && renderer.sharedMaterial != null)
                    {
                        if (!materialGroups.ContainsKey(renderer.sharedMaterial))
                        {
                            materialGroups[renderer.sharedMaterial] = new List<MeshFilter>();
                        }

                        materialGroups[renderer.sharedMaterial].Add(filter);
                    }
                }

                // 对每个材质组进行静态批处理
                foreach (var kvp in materialGroups)
                {
                    if (kvp.Value.Count > 1)
                    {
                        List<MeshFilter> filters = kvp.Value;
                        List<CombineInstance> combineInstances = new List<CombineInstance>();

                        foreach (MeshFilter filter in filters)
                        {
                            if (filter.sharedMesh != null)
                            {
                                CombineInstance ci = new CombineInstance();
                                ci.mesh = filter.sharedMesh;
                                ci.transform = filter.transform.localToWorldMatrix;
                                combineInstances.Add(ci);
                            }
                        }

                        // 检查顶点数
                        int totalVertices = 0;
                        foreach (var ci in combineInstances)
                        {
                            totalVertices += ci.mesh.vertexCount;
                        }

                        if (totalVertices <= maxVerticesPerBatch)
                        {
                            // 创建合并后的网格
                            Mesh combinedMesh = new Mesh();
                            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

                            // 创建新的GameObject来承载合并后的网格
                            GameObject batchedObject = new GameObject($"Batched_{kvp.Key.name}_{tag}");
                            batchedObject.isStatic = true;

                            MeshFilter batchFilter = batchedObject.AddComponent<MeshFilter>();
                            batchFilter.sharedMesh = combinedMesh;

                            MeshRenderer batchRenderer = batchedObject.AddComponent<MeshRenderer>();
                            batchRenderer.sharedMaterial = kvp.Key;

                            // 禁用原始对象
                            foreach (var filter in filters)
                            {
                                if (filter != null)
                                {
                                    filter.gameObject.SetActive(false);
                                }
                            }

                            Debug.Log($"批处理完成: {tag} - {kvp.Key.name}, 合并对象数: {filters.Count}, 顶点数: {totalVertices}");
                        }
                        else
                        {
                            Debug.LogWarning($"跳过批处理: {tag} - {kvp.Key.name}, 顶点数超限: {totalVertices}");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 优化指定GameObject下的所有子对象
    /// </summary>
    public void OptimizeGameObject(GameObject root)
    {
        if (root == null) return;

        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length > 1)
        {
            // 按材质分组
            Dictionary<Material, List<MeshFilter>> materialGroups = new Dictionary<Material, List<MeshFilter>>();

            foreach (MeshRenderer renderer in renderers)
            {
                MeshFilter filter = renderer.GetComponent<MeshFilter>();

                if (filter != null && renderer.sharedMaterial != null)
                {
                    if (!materialGroups.ContainsKey(renderer.sharedMaterial))
                    {
                        materialGroups[renderer.sharedMaterial] = new List<MeshFilter>();
                    }

                    materialGroups[renderer.sharedMaterial].Add(filter);
                }
            }

            // 对每个材质组进行批处理
            foreach (var kvp in materialGroups)
            {
                if (kvp.Value.Count > 1)
                {
                    CombineMeshes(kvp.Value, kvp.Key, root.transform);
                }
            }
        }
    }

    private void CombineMeshes(List<MeshFilter> filters, Material material, Transform parent)
    {
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (MeshFilter filter in filters)
        {
            if (filter.sharedMesh != null)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = filter.sharedMesh;
                ci.transform = filter.transform.localToWorldMatrix;
                combineInstances.Add(ci);
            }
        }

        int totalVertices = 0;
        foreach (var ci in combineInstances)
        {
            totalVertices += ci.mesh.vertexCount;
        }

        if (totalVertices <= maxVerticesPerBatch)
        {
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            GameObject batchedObject = new GameObject($"Batched_{material.name}");
            batchedObject.transform.SetParent(parent);
            batchedObject.isStatic = true;

            MeshFilter batchFilter = batchedObject.AddComponent<MeshFilter>();
            batchFilter.sharedMesh = combinedMesh;

            MeshRenderer batchRenderer = batchedObject.AddComponent<MeshRenderer>();
            batchRenderer.sharedMaterial = material;

            foreach (var filter in filters)
            {
                if (filter != null)
                {
                    filter.gameObject.SetActive(false);
                }
            }

            Debug.Log($"批处理完成: {material.name}, 合并对象数: {filters.Count}, 顶点数: {totalVertices}");
        }
    }

    /// <summary>
    /// 清理优化结果
    /// </summary>
    public void CleanupOptimization()
    {
        // 重新启用所有被禁用的对象
        foreach (string tag in targetTags)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in taggedObjects)
            {
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                }
            }
        }

        // 删除批处理对象
        Transform[] batchedObjects = transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in batchedObjects)
        {
            if (t.name.StartsWith("Batched_"))
            {
                if (t != transform)
                {
                    DestroyImmediate(t.gameObject);
                }
            }
        }

        Debug.Log("批处理优化已清理");
    }

    /// <summary>
    /// 设置静态标志
    /// </summary>
    public void SetStaticFlagRecursively(GameObject obj, bool isStatic)
    {
        if (obj == null) return;

        obj.isStatic = isStatic;

        foreach (Transform child in obj.transform)
        {
            SetStaticFlagRecursively(child.gameObject, isStatic);
        }
    }
}
