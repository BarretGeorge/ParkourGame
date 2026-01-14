using UnityEngine;

/// <summary>
/// 护盾道具 - 抵挡一次伤害
/// </summary>
public class ShieldPowerUp : PowerUp
{
    [Header("护盾设置")]
    [Tooltip("护盾颜色")]
    [SerializeField] private Color shieldColor = Color.cyan;

    [Tooltip("护盾强度（可抵挡伤害次数）")]
    [SerializeField] private int shieldStrength = 1;

    // 状态
    private int currentStrength;
    private GameObject shieldVisual;

    protected override void Initialize()
    {
        base.Initialize();
        collectibleType = CollectibleType.Shield;

        // 设置颜色
        if (collectibleRenderer != null)
        {
            collectibleRenderer.material.color = shieldColor;
        }
    }

    protected override void ActivatePowerUp(PlayerController player)
    {
        isActive = true;
        remainingTime = duration;
        currentStrength = shieldStrength;

        // 创建护盾视觉效果
        CreateShieldVisual(player.transform);

        // 启用碰撞保护
        EnableShieldProtection(player);

        Debug.Log($"[ShieldPowerUp] Shield activated! Strength: {currentStrength}");
    }

    protected override void DeactivatePowerUp(PlayerController player)
    {
        isActive = false;

        // 移除护盾视觉效果
        DestroyShieldVisual();

        // 禁用碰撞保护
        DisableShieldProtection(player);

        Debug.Log("[ShieldPowerUp] Shield deactivated");
    }

    /// <summary>
    /// 创建护盾视觉效果
    /// </summary>
    private void CreateShieldVisual(Transform playerTransform)
    {
        // 创建护盾球体
        shieldVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shieldVisual.name = "ShieldVisual";
        shieldVisual.transform.SetParent(playerTransform);
        shieldVisual.transform.localPosition = Vector3.zero;
        shieldVisual.transform.localScale = Vector3.one * 2f;

        // 设置材质
        Renderer renderer = shieldVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 创建半透明材质
            Material shieldMaterial = new Material(Shader.Find("Standard"));
            shieldMaterial.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, 0.3f);
            shieldMaterial.SetFloat("_Mode", 3); // Transparent mode
            shieldMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            shieldMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            shieldMaterial.SetInt("_ZWrite", 0);
            renderer.material = shieldMaterial;
        }

        // 移除碰撞体
        Destroy(shieldVisual.GetComponent<Collider>());
    }

    /// <summary>
    /// 销毁护盾视觉效果
    /// </summary>
    private void DestroyShieldVisual()
    {
        if (shieldVisual != null)
        {
            Destroy(shieldVisual);
            shieldVisual = null;
        }
    }

    /// <summary>
    /// 启用护盾保护
    /// </summary>
    private void EnableShieldProtection(PlayerController player)
    {
        // TODO: 这需要修改PlayerController的碰撞逻辑
        // 在Phase 6完善碰撞检测系统时实现

        // 现在通过设置标志来模拟
        // player.SetShieldActive(true);
    }

    /// <summary>
    /// 禁用护盾保护
    /// </summary>
    private void DisableShieldProtection(PlayerController player)
    {
        // player.SetShieldActive(false);
    }

    /// <summary>
    /// 抵挡伤害
    /// </summary>
    public bool TryAbsorbDamage()
    {
        if (!isActive || currentStrength <= 0) return false;

        currentStrength--;

        // 更新护盾视觉效果（改变透明度）
        UpdateShieldVisual();

        Debug.Log($"[ShieldPowerUp] Damage absorbed! Remaining strength: {currentStrength}");

        // 如果护盾破裂，立即停用
        if (currentStrength <= 0)
        {
            // 查找PlayerController并停用护盾
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                DeactivatePowerUp(player);
            }
        }

        return true;
    }

    /// <summary>
    /// 更新护盾视觉效果
    /// </summary>
    private void UpdateShieldVisual()
    {
        if (shieldVisual == null) return;

        Renderer renderer = shieldVisual.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color color = renderer.material.color;
            color.a = 0.3f * (currentStrength / (float)shieldStrength);
            renderer.material.color = color;
        }
    }

    /// <summary>
    /// 获取当前护盾强度
    /// </summary>
    public int GetCurrentStrength()
    {
        return currentStrength;
    }

    private void Update()
    {
        base.Update();

        // 持续检查护盾效果
        if (isActive)
        {
            UpdatePowerUpEffect();
        }
    }

    /// <summary>
    /// 更新道具效果
    /// </summary>
    private void UpdatePowerUpEffect()
    {
        // 持续更新护盾视觉效果
        if (shieldVisual != null)
        {
            // 旋转护盾
            shieldVisual.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }
    }
}
