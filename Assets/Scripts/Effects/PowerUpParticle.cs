using UnityEngine;

/// <summary>
/// 道具特效组件
/// </summary>
public class PowerUpParticle : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] private ParticleSystem auraParticles;
    [SerializeField] private ParticleSystem pickupParticles;
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private Light glowLight;

    [Header("道具颜色")]
    [SerializeField] private Color magnetColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color shieldColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color speedBoostColor = new Color(1f, 0.4f, 0.2f);
    [SerializeField] private Color invincibilityColor = new Color(1f, 0.8f, 0.2f);

    private PowerUpType currentType;

    private void Awake()
    {
        SetupParticleSystems();
    }

    private void SetupParticleSystems()
    {
        SetupAuraParticles();
        SetupPickupParticles();
        SetupTrailParticles();
    }

    private void SetupAuraParticles()
    {
        if (auraParticles == null) return;

        var main = auraParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = auraParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var emission = auraParticles.emission;
        emission.rateOverTime = 20;

        var colorOverLifetime = auraParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 1f, 1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.6f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var rotationOverLifetime = auraParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(30f, 60f);
    }

    private void SetupPickupParticles()
    {
        if (pickupParticles == null) return;

        var main = pickupParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.maxParticles = 50;

        var shape = pickupParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var emission = pickupParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30, 50)
        });
    }

    private void SetupTrailParticles()
    {
        if (trailParticles == null) return;

        var main = trailParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.maxParticles = 100;

        var shape = trailParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;

        var emission = trailParticles.emission;
        emission.rateOverTime = 30;
    }

    public void SetPowerUpType(PowerUpType type)
    {
        currentType = type;
        Color color = GetColorForType(type);

        if (auraParticles != null)
        {
            var main = auraParticles.main;
            main.startColor = color;
        }

        if (pickupParticles != null)
        {
            var main = pickupParticles.main;
            main.startColor = color;
        }

        if (trailParticles != null)
        {
            var main = trailParticles.main;
            main.startColor = color;
        }

        if (glowLight != null)
        {
            glowLight.color = color;
        }
    }

    private Color GetColorForType(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Magnet:
                return magnetColor;
            case PowerUpType.Shield:
                return shieldColor;
            case PowerUpType.SpeedBoost:
                return speedBoostColor;
            case PowerUpType.Invulnerability:
                return invincibilityColor;
            default:
                return Color.white;
        }
    }

    public void PlayAuraEffect()
    {
        if (auraParticles != null)
        {
            auraParticles.Clear();
            auraParticles.Play();
        }

        if (glowLight != null)
        {
            glowLight.enabled = true;
        }
    }

    public void StopAuraEffect()
    {
        if (auraParticles != null)
        {
            auraParticles.Stop();
        }

        if (glowLight != null)
        {
            glowLight.enabled = false;
        }
    }

    public void PlayPickupEffect()
    {
        if (pickupParticles != null)
        {
            pickupParticles.Clear();
            pickupParticles.Play();
        }
    }

    public void PlayTrailEffect()
    {
        if (trailParticles != null)
        {
            trailParticles.Clear();
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
