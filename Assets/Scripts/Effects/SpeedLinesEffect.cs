using UnityEngine;

/// <summary>
/// 速度线特效 - 高速移动时的视觉反馈
/// </summary>
public class SpeedLinesEffect : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] private ParticleSystem speedLineParticles;

    [Header("速度线设置")]
    [SerializeField] private float minSpeedToShow = 10f;
    [SerializeField] private float maxSpeedForFullEffect = 25f;
    [SerializeField] private float lineLength = 5f;
    [SerializeField] private float lineSpeed = 20f;
    [SerializeField] private int maxLines = 100;

    [Header("颜色设置")]
    [SerializeField] private Color slowColor = new Color(0.8f, 0.8f, 1f, 0.3f);
    [SerializeField] private Color fastColor = new Color(1f, 1f, 1f, 0.8f);

    private float currentSpeed;
    private bool isActive;

    private void Awake()
    {
        SetupSpeedLines();
    }

    private void SetupSpeedLines()
    {
        if (speedLineParticles == null)
        {
            speedLineParticles = GetComponent<ParticleSystem>();
        }

        if (speedLineParticles == null) return;

        var main = speedLineParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startSpeed = lineSpeed;
        main.maxParticles = maxLines;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = speedLineParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(2f, 3f, 1f);

        var emission = speedLineParticles.emission;
        emission.rateOverTime = 0;
        emission.enabled = false;

        var trails = speedLineParticles.trails;
        trails.enabled = true;
        trails.lifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        trails.width = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);

        var colorOverLifetime = speedLineParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(slowColor, 0f),
                new GradientColorKey(fastColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.6f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        UpdateEffectIntensity();
    }

    private void UpdateEffectIntensity()
    {
        if (speedLineParticles == null) return;

        if (currentSpeed < minSpeedToShow)
        {
            if (isActive)
            {
                speedLineParticles.Stop();
                isActive = false;
            }
            return;
        }

        float speedRatio = Mathf.InverseLerp(minSpeedToShow, maxSpeedForFullEffect, currentSpeed);

        if (!isActive)
        {
            speedLineParticles.Play();
            isActive = true;
        }

        var emission = speedLineParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = speedRatio * 50;

        var main = speedLineParticles.main;
        main.startSpeed = lineSpeed * (0.5f + speedRatio * 0.5f);
    }

    public void ShowSpeedLines()
    {
        if (speedLineParticles != null && !isActive)
        {
            speedLineParticles.Play();
            isActive = true;
        }
    }

    public void HideSpeedLines()
    {
        if (speedLineParticles != null && isActive)
        {
            speedLineParticles.Stop();
            isActive = false;
        }
    }
}
