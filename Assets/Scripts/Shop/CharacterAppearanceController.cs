using UnityEngine;

/// <summary>
/// 角色外观控制器 - 应用角色和皮肤
/// </summary>
public class CharacterAppearanceController : MonoBehaviour
{
    [Header("渲染器")]
    [SerializeField] private SkinnedMeshRenderer mainRenderer;
    [SerializeField] private MeshRenderer meshRenderer;

    [Header("粒子特效")]
    [SerializeField] private ParticleSystem trailEffect;
    [SerializeField] private Light glowLight;

    private CharacterData currentCharacter;
    private SkinData currentSkin;

    private void Start()
    {
        LoadCurrentAppearance();
    }

    private void LoadCurrentAppearance()
    {
        if (ShopManager.Instance != null)
        {
            currentCharacter = ShopManager.Instance.GetCurrentCharacter();
            currentSkin = ShopManager.Instance.GetCurrentSkin();

            ApplyCharacter(currentCharacter);
            ApplySkin(currentSkin);
        }
    }

    public void ApplyCharacter(CharacterData character)
    {
        if (character == null) return;

        currentCharacter = character;

        // 应用角色材质
        if (mainRenderer != null && character.characterMaterial != null)
        {
            mainRenderer.material = character.characterMaterial;
        }

        if (meshRenderer != null && character.characterMaterial != null)
        {
            meshRenderer.material = character.characterMaterial;
        }

        // 应用角色动画控制器
        Animator animator = GetComponent<Animator>();
        if (animator != null && character.animatorController != null)
        {
            animator.runtimeAnimatorController = character.animatorController;
        }

        // 应用属性修正
        ApplyCharacterModifiers();
    }

    public void ApplySkin(SkinData skin)
    {
        if (skin == null) return;

        currentSkin = skin;

        // 应用皮肤材质
        if (mainRenderer != null && skin.skinMaterial != null)
        {
            mainRenderer.material = skin.skinMaterial;
        }

        if (meshRenderer != null && skin.skinMaterial != null)
        {
            meshRenderer.material = skin.skinMaterial;
        }

        // 应用皮肤颜色
        if (mainRenderer != null)
        {
            mainRenderer.material.color = skin.skinColor;
        }

        if (meshRenderer != null)
        {
            meshRenderer.material.color = skin.skinColor;
        }

        // 应用拖尾特效
        if (skin.hasTrailEffect && trailEffect != null)
        {
            trailEffect.gameObject.SetActive(true);
        }
        else if (trailEffect != null)
        {
            trailEffect.gameObject.SetActive(false);
        }

        // 应用发光效果
        if (skin.hasGlowEffect && glowLight != null)
        {
            glowLight.color = skin.glowColor;
            glowLight.enabled = true;
        }
        else if (glowLight != null)
        {
            glowLight.enabled = false;
        }
    }

    private void ApplyCharacterModifiers()
    {
        if (currentCharacter == null) return;

        // 应用角色属性修正到PlayerController
        PlayerController player = GetComponent<PlayerController>();
        if (player != null)
        {
            // 速度修正
            if (currentCharacter.speedModifier != 1f)
            {
                player.SetSpeed(player.PlayerData.baseSpeed * currentCharacter.speedModifier);
            }

            // 跳跃修正
            if (currentCharacter.jumpModifier != 1f)
            {
                PlayerJump playerJump = player.GetComponent<PlayerJump>();
                if (playerJump != null)
                {
                    playerJump.SetJumpForceModifier(currentCharacter.jumpModifier);
                }
            }

            // 磁铁修正
            if (currentCharacter.coinMagnetModifier != 1f)
            {
                // 通过升级管理器应用磁铁范围修正
                if (UpgradeManager.Instance != null)
                {
                    float baseMultiplier = UpgradeManager.Instance.GetMagnetRangeMultiplier();
                    float finalMultiplier = baseMultiplier * currentCharacter.coinMagnetModifier;
                }
            }
        }
    }

    public void RefreshAppearance()
    {
        LoadCurrentAppearance();
    }
}
