using UnityEngine;

/// <summary>
/// 死亡特效组件
/// </summary>
public class DeathParticle : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private ParticleSystem sparkleParticles;
    [SerializeField] private Light flashLight;

    [Header("爆炸设置")]
    [SerializeField] private int explosionParticleCount = 100;
    [SerializeField] private float explosionForce = 15f;
    [SerializeField] private float explosionRadius = 2f;

    [Header("烟雾设置")]
    [SerializeField] private int smokeParticleCount = 50;
    [SerializeField] private float smokeDuration = 3f;

    [Header("闪光设置")]
    [SerializeField] private float flashIntensity = 3f;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private Color flashColor = new Color(1f, 0.5f, 0.2f);

    private void Awake()
    {
        SetupParticleSystems();
    }

    private void SetupParticleSystems()
    {
        SetupExplosionParticles();
        SetupSmokeParticles();
        SetupSparkleParticles();
    }

    private void SetupExplosionParticles()
    {
        if (explosionParticles == null) return;

        var main = explosionParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSpeed = explosionForce;
        main.startColor = new Color(1f, 0.6f, 0.2f);
        main.maxParticles = explosionParticleCount;
        main.gravityModifier = 1f;

        var shape = explosionParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var emission = explosionParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, explosionParticleCount)
        });

        var colorOverLifetime = explosionParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.8f, 0.4f), 0f),
                new GradientColorKey(new Color(1f, 0.4f, 0.1f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.2f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var rotationOverLifetime = explosionParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);
    }

    private void SetupSmokeParticles()
    {
        if (smokeParticles == null) return;

        var main = smokeParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1f, smokeDuration);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        main.maxParticles = smokeParticleCount;
        main.gravityModifier = 0.2f;

        var shape = smokeParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.5f;

        var emission = smokeParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, smokeParticleCount)
        });

        var colorOverLifetime = smokeParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.3f, 0.3f, 0.3f, 0.5f), 0f),
                new GradientColorKey(new Color(0.2f, 0.2f, 0.2f, 0.2f), 0.5f),
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = smokeParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 3f);
    }

    private void SetupSparkleParticles()
    {
        if (sparkleParticles == null) return;

        var main = sparkleParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        main.startColor = new Color(1f, 0.9f, 0.5f);
        main.maxParticles = 80;

        var shape = sparkleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var emission = sparkleParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 50, 80)
        });

        var colorOverLifetime = sparkleParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0.3f),
                new GradientColorKey(new Color(1f, 0.5f, 0.2f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
    }

    public void PlayDeathEffect()
    {
        if (explosionParticles != null)
        {
            explosionParticles.Clear();
            explosionParticles.Play();
        }

        if (smokeParticles != null)
        {
            smokeParticles.Clear();
            smokeParticles.Play();
        }

        if (sparkleParticles != null)
        {
            sparkleParticles.Clear();
            sparkleParticles.Play();
        }

        if (flashLight != null)
        {
            StartCoroutine(FlashEffect());
        }
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        flashLight.enabled = true;
        flashLight.color = flashColor;
        flashLight.intensity = flashIntensity;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(flashIntensity, 0f, elapsed / flashDuration);
            yield return null;
        }

        flashLight.enabled = false;
    }
}
