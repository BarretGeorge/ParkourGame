using UnityEngine;

/// <summary>
/// 脚步尘土特效
/// </summary>
public class FootstepParticle : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] private ParticleSystem dustParticles;

    [Header("尘土设置")]
    [SerializeField] private float minLifetime = 0.5f;
    [SerializeField] private float maxLifetime = 1.5f;
    [SerializeField] private float minSize = 0.1f;
    [SerializeField] private float maxSize = 0.3f;
    [SerializeField] private int minParticles = 5;
    [SerializeField] private int maxParticles = 15;

    private void Awake()
    {
        if (dustParticles == null)
        {
            dustParticles = GetComponent<ParticleSystem>();
        }

        SetupParticleSystem();
    }

    private void SetupParticleSystem()
    {
        if (dustParticles == null) return;

        var main = dustParticles.main;
        main.playOnAwake = false;
        main.loop = false;

        main.startLifetime = new ParticleSystem.MinMaxCurve(minLifetime, maxLifetime);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.maxParticles = maxParticles;

        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = dustParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, minParticles, maxParticles)
        });

        var shape = dustParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.2f;

        var colorOverLifetime = dustParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.8f, 0.7f, 0.5f, 0.8f), 0f),
                new GradientColorKey(new Color(0.6f, 0.5f, 0.3f, 0.4f), 0.5f),
                new GradientColorKey(new Color(0.4f, 0.3f, 0.2f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.4f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = dustParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);
    }

    public void PlayFootstep()
    {
        if (dustParticles != null)
        {
            dustParticles.Clear();
            dustParticles.Play();
        }
    }

    public void PlayLandEffect(float intensity = 1f)
    {
        if (dustParticles != null)
        {
            var main = dustParticles.main;
            int burstCount = Mathf.RoundToInt(maxParticles * intensity);
            var emission = dustParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, minParticles, burstCount)
            });

            dustParticles.Clear();
            dustParticles.Play();
        }
    }
}
