using UnityEngine;

/// <summary>
/// 收集品特效组件
/// </summary>
public class CollectibleParticle : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] private ParticleSystem burstParticles;
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private Light pointLight;

    [Header("金币颜色配置")]
    [SerializeField] private Color bronzeColor = new Color(0.72f, 0.45f, 0.2f);
    [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color platinumColor = new Color(0.86f, 0.98f, 1f);
    [SerializeField] private Color diamondColor = new Color(0.5f, 0.85f, 0.95f);

    [Header("特效设置")]
    [SerializeField] private float burstParticleCount = 30;
    [SerializeField] private float burstSpeed = 5f;
    [SerializeField] private float lightIntensity = 2f;
    [SerializeField] private float lightDuration = 0.5f;

    private CollectibleType currentType;

    private void Awake()
    {
        SetupParticleSystems();
    }

    private void SetupParticleSystems()
    {
        if (burstParticles == null)
        {
            burstParticles = GetComponentInChildren<ParticleSystem>();
        }

        if (burstParticles != null)
        {
            var main = burstParticles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = (int)burstParticleCount;
            main.startSpeed = burstSpeed;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = burstParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var emission = burstParticles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, burstParticleCount)
            });

            var colorOverLifetime = burstParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
        }

        if (pointLight != null)
        {
            pointLight.enabled = false;
        }
    }

    public void SetCollectibleType(CollectibleType type)
    {
        currentType = type;
        Color particleColor = GetColorForType(type);

        if (burstParticles != null)
        {
            var main = burstParticles.main;
            main.startColor = particleColor;
        }

        if (pointLight != null)
        {
            pointLight.color = particleColor;
        }
    }

    private Color GetColorForType(CollectibleType type)
    {
        switch (type)
        {
            case CollectibleType.BronzeCoin:
                return bronzeColor;
            case CollectibleType.SilverCoin:
                return silverColor;
            case CollectibleType.GoldCoin:
                return goldColor;
            case CollectibleType.PlatinumCoin:
                return platinumColor;
            case CollectibleType.DiamondCoin:
                return diamondColor;
            case CollectibleType.Magnet:
                return new Color(0.2f, 0.6f, 1f);
            case CollectibleType.Shield:
                return new Color(0.3f, 0.8f, 0.3f);
            case CollectibleType.SpeedBoost:
                return new Color(1f, 0.4f, 0.2f);
            case CollectibleType.DoubleScore:
                return new Color(1f, 0.2f, 0.8f);
            case CollectibleType.ScoreMultiplier:
                return new Color(0.8f, 0.8f, 0.2f);
            case CollectibleType.Invulnerability:
                return new Color(0.8f, 0.5f, 1f);
            default:
                return Color.white;
        }
    }

    public void PlayBurstEffect()
    {
        if (burstParticles != null)
        {
            burstParticles.Clear();
            burstParticles.Play();
        }

        if (pointLight != null)
        {
            StartCoroutine(FlashLight());
        }
    }

    private System.Collections.IEnumerator FlashLight()
    {
        pointLight.enabled = true;
        pointLight.intensity = lightIntensity;

        float elapsed = 0f;
        while (elapsed < lightDuration)
        {
            elapsed += Time.deltaTime;
            pointLight.intensity = Mathf.Lerp(lightIntensity, 0f, elapsed / lightDuration);
            yield return null;
        }

        pointLight.enabled = false;
    }

    public void PlayTrailEffect()
    {
        if (trailParticles != null)
        {
            trailParticles.Play();
        }
    }

    public void StopTrailEffect()
    {
        if (trailParticles != null)
        {
            trailParticles.Stop();
        }
    }
}
