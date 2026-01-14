using UnityEngine;

/// <summary>
/// 拖尾特效 - 角色移动时的拖尾效果
/// </summary>
public class TrailParticle : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("拖尾设置")]
    [SerializeField] private float emissionRate = 20f;
    [SerializeField] private float trailLifetime = 1f;
    [SerializeField] private float minSize = 0.1f;
    [SerializeField] private float maxSize = 0.3f;
    [SerializeField] private float trailWidth = 0.3f;

    [Header("颜色")]
    [SerializeField] private Gradient trailGradient;

    private Transform target;
    private bool isActive;

    private void Awake()
    {
        SetupTrail();
    }

    private void SetupTrail()
    {
        SetupParticleSystem();
        SetupTrailRenderer();
    }

    private void SetupParticleSystem()
    {
        if (trailParticles == null)
        {
            trailParticles = GetComponent<ParticleSystem>();
        }

        if (trailParticles == null) return;

        var main = trailParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = trailLifetime;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = trailParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var emission = trailParticles.emission;
        emission.rateOverTime = emissionRate;

        var colorOverLifetime = trailParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        if (trailGradient == null)
        {
            trailGradient = CreateDefaultGradient();
        }
        colorOverLifetime.color = trailGradient;

        var sizeOverLifetime = trailParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);
    }

    private void SetupTrailRenderer()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }

        if (trailRenderer == null) return;

        trailRenderer.time = trailLifetime;
        trailRenderer.startWidth = trailWidth;
        trailRenderer.endWidth = 0f;
        trailRenderer.numCapVertices = 10;
        trailRenderer.numCornerVertices = 10;

        if (trailGradient == null)
        {
            trailGradient = CreateDefaultGradient();
        }

        trailRenderer.colorGradient = trailGradient;
    }

    private Gradient CreateDefaultGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 1f, 1f, 0.5f), 0.5f),
                new GradientColorKey(new Color(1f, 1f, 1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.4f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return gradient;
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void SetColor(Color color)
    {
        if (trailParticles != null)
        {
            var main = trailParticles.main;
            main.startColor = color;
        }

        if (trailRenderer != null)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(new Color(color.r, color.g, color.b, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trailRenderer.colorGradient = gradient;
        }
    }

    public void PlayTrail()
    {
        if (trailParticles != null)
        {
            trailParticles.Play();
        }

        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
        isActive = true;
    }

    public void StopTrail()
    {
        if (trailParticles != null)
        {
            trailParticles.Stop();
        }

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
        isActive = false;
    }

    public void SetEmissionRate(float rate)
    {
        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            emission.rateOverTime = rate;
        }
    }

    private void Update()
    {
        if (target != null && isActive)
        {
            transform.position = target.position;
        }
    }
}
