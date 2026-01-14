using UnityEngine;

/// <summary>
/// 碰撞特效
/// </summary>
public class CollisionParticle : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] private ParticleSystem impactParticles;
    [SerializeField] private ParticleSystem debrisParticles;
    [SerializeField] private ParticleSystem sparkParticles;

    [Header("碰撞设置")]
    [SerializeField] private float impactForce = 10f;
    [SerializeField] private int debrisCount = 20;
    [SerializeField] private float debrisSizeMin = 0.05f;
    [SerializeField] private float debrisSizeMax = 0.15f;
    [SerializeField] private float sparkCount = 10;

    [Header("颜色设置")]
    [SerializeField] private Color impactColor = new Color(0.8f, 0.6f, 0.4f);
    [SerializeField] private Color debrisColor = new Color(0.5f, 0.4f, 0.3f);
    [SerializeField] private Color sparkColor = new Color(1f, 0.8f, 0.3f);

    private void Awake()
    {
        SetupParticleSystems();
    }

    private void SetupParticleSystems()
    {
        SetupImpactParticles();
        SetupDebrisParticles();
        SetupSparkParticles();
    }

    private void SetupImpactParticles()
    {
        if (impactParticles == null) return;

        var main = impactParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startColor = impactColor;
        main.maxParticles = 50;

        var shape = impactParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.3f;

        var emission = impactParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15, 25)
        });

        var colorOverLifetime = impactParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(impactColor, 0f),
                new GradientColorKey(new Color(impactColor.r, impactColor.g, impactColor.b, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
    }

    private void SetupDebrisParticles()
    {
        if (debrisParticles == null) return;

        var main = debrisParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(debrisSizeMin, debrisSizeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startColor = debrisColor;
        main.maxParticles = (int)debrisCount;
        main.gravityModifier = 2f;

        var shape = debrisParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.2f, 0.2f, 0.2f);

        var emission = debrisParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, debrisCount * 0.8f, debrisCount)
        });

        var rotationOverLifetime = debrisParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);
    }

    private void SetupSparkParticles()
    {
        if (sparkParticles == null) return;

        var main = sparkParticles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startColor = sparkColor;
        main.maxParticles = (int)sparkCount;

        var shape = sparkParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.1f;

        var emission = sparkParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, sparkCount * 0.8f, sparkCount)
        });

        var colorOverLifetime = sparkParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(sparkColor, 0.3f),
                new GradientColorKey(new Color(sparkColor.r, sparkColor.g, sparkColor.b, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
    }

    public void PlayCollisionEffect(Vector3 collisionNormal, float force = 1f)
    {
        // 设置粒子系统方向
        if (impactParticles != null)
        {
            impactParticles.transform.rotation = Quaternion.LookRotation(collisionNormal);
            impactParticles.Clear();
            impactParticles.Play();
        }

        if (debrisParticles != null)
        {
            debrisParticles.transform.rotation = Quaternion.LookRotation(collisionNormal);
            debrisParticles.Clear();
            debrisParticles.Play();
        }

        if (sparkParticles != null)
        {
            sparkParticles.transform.rotation = Quaternion.LookRotation(collisionNormal);
            sparkParticles.Clear();
            sparkParticles.Play();
        }
    }

    public void PlayShieldBreakEffect()
    {
        // 护盾破碎特效 - 蓝色碎片
        if (debrisParticles != null)
        {
            var main = debrisParticles.main;
            main.startColor = new Color(0.3f, 0.6f, 1f);
            debrisParticles.Clear();
            debrisParticles.Play();
        }
    }

    public void PlayDeathEffect()
    {
        // 死亡特效 - 更多的碎片和火花
        if (impactParticles != null)
        {
            var emission = impactParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 30, 50)
            });
            impactParticles.Clear();
            impactParticles.Play();
        }

        if (debrisParticles != null)
        {
            var emission = debrisParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, debrisCount * 2, debrisCount * 3)
            });
            debrisParticles.Clear();
            debrisParticles.Play();
        }
    }
}
