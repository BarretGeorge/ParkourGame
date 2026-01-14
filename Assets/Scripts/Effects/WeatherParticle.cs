using UnityEngine;

/// <summary>
/// 天气特效系统 - 雨、雪、风等
/// </summary>
public class WeatherParticle : MonoBehaviour
{
    [Header("天气类型")]
    [SerializeField] private WeatherType currentWeather = WeatherType.Clear;

    [Header("雨特效")]
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private float rainIntensity = 1000f;
    [SerializeField] private float rainSpeed = 15f;
    [SerializeField] private Color rainColor = new Color(0.6f, 0.7f, 0.9f, 0.5f);

    [Header("雪特效")]
    [SerializeField] private ParticleSystem snowParticles;
    [SerializeField] private float snowIntensity = 200f;
    [SerializeField] private float snowSpeed = 2f;
    [SerializeField] private Color snowColor = new Color(1f, 1f, 1f, 0.8f);

    [Header("雾特效")]
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private float fogDensity = 0.5f;
    [SerializeField] private Color fogColor = new Color(0.7f, 0.7f, 0.7f, 0.3f);

    [Header("风力设置")]
    [SerializeField] private Vector3 windDirection = new Vector3(1f, 0f, 0f);
    [SerializeField] private float windStrength = 1f;

    private bool isActive;

    public enum WeatherType
    {
        Clear,
        Rain,
        Snow,
        Fog,
        Storm
    }

    private void Awake()
    {
        SetupWeatherSystems();
        SetWeather(currentWeather);
    }

    private void SetupWeatherSystems()
    {
        SetupRainSystem();
        SetupSnowSystem();
        SetupFogSystem();
    }

    private void SetupRainSystem()
    {
        if (rainParticles == null) return;

        var main = rainParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startSpeed = rainSpeed;
        main.startColor = rainColor;
        main.maxParticles = (int)rainIntensity;
        main.gravityModifier = 2f;

        var shape = rainParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20f, 0f, 20f);

        var emission = rainParticles.emission;
        emission.rateOverTime = rainIntensity;

        var velocityOverLifetime = rainParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(windDirection.x * windStrength, windDirection.x * windStrength * 1.5f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(windDirection.z * windStrength, windDirection.z * windStrength * 1.5f);

        var colorOverLifetime = rainParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(rainColor, 0f),
                new GradientColorKey(new Color(rainColor.r, rainColor.g, rainColor.b, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var trails = rainParticles.trails;
        trails.enabled = true;
        trails.lifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        trails.width = new ParticleSystem.MinMaxCurve(0.01f, 0.02f);
    }

    private void SetupSnowSystem()
    {
        if (snowParticles == null) return;

        var main = snowParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startSpeed = snowSpeed;
        main.startColor = snowColor;
        main.maxParticles = (int)snowIntensity;
        main.gravityModifier = 0.1f;

        var shape = snowParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20f, 10f, 20f);

        var emission = snowParticles.emission;
        emission.rateOverTime = snowIntensity;

        var velocityOverLifetime = snowParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(
            -windStrength * 0.5f,
            windStrength * 0.5f
        );
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(
            -windStrength * 0.5f,
            windStrength * 0.5f
        );

        var rotationOverLifetime = snowParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-30f, 30f);

        var colorOverLifetime = snowParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(snowColor, 0f),
                new GradientColorKey(new Color(snowColor.r, snowColor.g, snowColor.b, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.4f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
    }

    private void SetupFogSystem()
    {
        if (fogParticles == null) return;

        var main = fogParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startColor = fogColor;
        main.maxParticles = 100;

        var shape = fogParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30f, 5f, 30f);

        var emission = fogParticles.emission;
        emission.rateOverTime = 10;

        var colorOverLifetime = fogParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(fogColor, 0f),
                new GradientColorKey(new Color(fogColor.r, fogColor.g, fogColor.b, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.3f, 0f),
                new GradientAlphaKey(0.15f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = fogParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 2f);
    }

    public void SetWeather(WeatherType weather)
    {
        currentWeather = weather;

        // 停止所有天气
        StopAllWeather();

        // 启动对应天气
        switch (weather)
        {
            case WeatherType.Clear:
                break;
            case WeatherType.Rain:
                StartRain();
                break;
            case WeatherType.Snow:
                StartSnow();
                break;
            case WeatherType.Fog:
                StartFog();
                break;
            case WeatherType.Storm:
                StartRain();
                StartFog();
                break;
        }
    }

    private void StopAllWeather()
    {
        if (rainParticles != null) rainParticles.Stop();
        if (snowParticles != null) snowParticles.Stop();
        if (fogParticles != null) fogParticles.Stop();
    }

    private void StartRain()
    {
        if (rainParticles != null)
        {
            rainParticles.Clear();
            rainParticles.Play();
        }
    }

    private void StartSnow()
    {
        if (snowParticles != null)
        {
            snowParticles.Clear();
            snowParticles.Play();
        }
    }

    private void StartFog()
    {
        if (fogParticles != null)
        {
            fogParticles.Clear();
            fogParticles.Play();
        }
    }

    public void SetWind(Vector3 direction, float strength)
    {
        windDirection = direction.normalized;
        windStrength = strength;

        // 更新雨的风力
        if (rainParticles != null)
        {
            var velocityOverLifetime = rainParticles.velocityOverLifetime;
            if (velocityOverLifetime.enabled)
            {
                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(
                    windDirection.x * windStrength,
                    windDirection.x * windStrength * 1.5f
                );
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(
                    windDirection.z * windStrength,
                    windDirection.z * windStrength * 1.5f
                );
            }
        }

        // 更新雪的风力
        if (snowParticles != null)
        {
            var velocityOverLifetime = snowParticles.velocityOverLifetime;
            if (velocityOverLifetime.enabled)
            {
                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(
                    -windStrength * 0.5f,
                    windStrength * 0.5f
                );
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(
                    -windStrength * 0.5f,
                    windStrength * 0.5f
                );
            }
        }
    }

    public void SetRainIntensity(float intensity)
    {
        rainIntensity = intensity;
        if (rainParticles != null)
        {
            var emission = rainParticles.emission;
            emission.rateOverTime = intensity;
        }
    }

    public void SetSnowIntensity(float intensity)
    {
        snowIntensity = intensity;
        if (snowParticles != null)
        {
            var emission = snowParticles.emission;
            emission.rateOverTime = intensity;
        }
    }
}
