using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 粒子特效管理器 - 统一管理所有粒子特效
/// </summary>
public class ParticleEffectsManager : MonoBehaviour
{
    [Header("粒子特效预设")]
    [SerializeField] private GameObject footstepParticlePrefab;
    [SerializeField] private GameObject collectibleParticlePrefab;
    [SerializeField] private GameObject collisionParticlePrefab;
    [SerializeField] private GameObject speedLinesPrefab;
    [SerializeField] private GameObject trailParticlePrefab;
    [SerializeField] private GameObject powerUpParticlePrefab;
    [SerializeField] private GameObject deathParticlePrefab;
    [SerializeField] private GameObject levelCompleteParticlePrefab;

    [Header("对象池设置")]
    [SerializeField] private int poolSize = 20;

    // 对象池
    private Dictionary<string, Queue<GameObject>> particlePools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, Transform> poolContainers = new Dictionary<string, Transform>();

    // 单例
    private static ParticleEffectsManager _instance;
    public static ParticleEffectsManager Instance => _instance;

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
        InitializePool("Footstep", footstepParticlePrefab);
        InitializePool("Collectible", collectibleParticlePrefab);
        InitializePool("Collision", collisionParticlePrefab);
        InitializePool("SpeedLines", speedLinesPrefab);
        InitializePool("Trail", trailParticlePrefab);
        InitializePool("PowerUp", powerUpParticlePrefab);
        InitializePool("Death", deathParticlePrefab);
        InitializePool("LevelComplete", levelCompleteParticlePrefab);
    }

    private void InitializePool(string poolName, GameObject prefab)
    {
        if (prefab == null) return;

        // 创建容器
        GameObject container = new GameObject($"{poolName}Pool");
        container.transform.SetParent(transform);
        poolContainers[poolName] = container.transform;

        // 创建对象池
        Queue<GameObject> pool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, container.transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        particlePools[poolName] = pool;
    }

    public GameObject SpawnParticle(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!particlePools.ContainsKey(poolName) || particlePools[poolName].Count == 0)
        {
            Debug.LogWarning($"粒子池 {poolName} 不存在或已空");
            return null;
        }

        GameObject particle = particlePools[poolName].Dequeue();
        particle.transform.position = position;
        particle.transform.rotation = rotation;
        particle.SetActive(true);

        // 自动回收到对象池
        StartCoroutine(ReturnToPoolAfterDelay(poolName, particle));

        return particle;
    }

    public GameObject SpawnParticle(string poolName, Vector3 position)
    {
        return SpawnParticle(poolName, position, Quaternion.identity);
    }

    private System.Collections.IEnumerator ReturnToPoolAfterDelay(string poolName, GameObject particle)
    {
        yield return new WaitForSeconds(2f);

        if (particle != null)
        {
            particle.SetActive(false);
            particle.transform.SetParent(poolContainers[poolName]);
            particlePools[poolName].Enqueue(particle);
        }
    }

    public void PlayFootstepEffect(Vector3 position)
    {
        SpawnParticle("Footstep", position);
    }

    public void PlayCollectibleEffect(Vector3 position, CollectibleType type)
    {
        GameObject effect = SpawnParticle("Collectible", position);
        if (effect != null)
        {
            ParticleCollector collector = effect.GetComponent<ParticleCollector>();
            if (collector != null)
            {
                collector.SetCollectibleType(type);
            }
        }
    }

    public void PlayCollisionEffect(Vector3 position, Vector3 normal)
    {
        GameObject effect = SpawnParticle("Collision", position);
        if (effect != null)
        {
            effect.transform.rotation = Quaternion.LookRotation(normal);
        }
    }

    public void PlayPowerUpEffect(Vector3 position, PowerUpType type)
    {
        GameObject effect = SpawnParticle("PowerUp", position);
        if (effect != null)
        {
            PowerUpParticle particle = effect.GetComponent<PowerUpParticle>();
            if (particle != null)
            {
                particle.SetPowerUpType(type);
            }
        }
    }

    public void PlayDeathEffect(Vector3 position)
    {
        SpawnParticle("Death", position);
    }

    public void ShowSpeedLines(bool show)
    {
        Transform poolContainer = poolContainers.ContainsKey("SpeedLines") ? poolContainers["SpeedLines"] : null;
        if (poolContainer != null)
        {
            poolContainer.gameObject.SetActive(show);
        }
    }

    public GameObject AttachTrailTo(Transform target)
    {
        GameObject trail = SpawnParticle("Trail", target.position);
        if (trail != null)
        {
            trail.transform.SetParent(target);
            TrailEmitter emitter = trail.GetComponent<TrailEmitter>();
            if (emitter != null)
            {
                emitter.SetTarget(target);
            }
        }
        return trail;
    }

    public void ClearAllEffects()
    {
        foreach (var pair in particlePools)
        {
            while (pair.Value.Count > 0)
            {
                GameObject obj = pair.Value.Dequeue();
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
