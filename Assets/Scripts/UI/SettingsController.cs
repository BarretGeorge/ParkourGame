using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 设置界面控制器
/// </summary>
public class SettingsController : MonoBehaviour
{
    [Header("音频设置")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("图形设置")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle shadowsToggle;
    [SerializeField] private Toggle particlesToggle;

    [Header("游戏设置")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityText;
    [SerializeField] private Toggle easyModeToggle;
    [SerializeField] private Toggle showTutorialToggle;

    [Header("控制设置")]
    [SerializeField] private TMP_Dropdown controlSchemeDropdown;

    [Header("按钮")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;

    // 设置键名
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string QualityLevelKey = "QualityLevel";
    private const string VSyncKey = "VSync";
    private const string ShadowsKey = "Shadows";
    private const string ParticlesKey = "Particles";
    private const string SensitivityKey = "Sensitivity";
    private const string EasyModeKey = "EasyMode";
    private const string ShowTutorialKey = "ShowTutorial";
    private const string ControlSchemeKey = "ControlScheme";

    private void Start()
    {
        LoadSettings();
        SetupButtons();
        SetupListeners();
    }

    private void SetupButtons()
    {
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(SaveSettings);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetToDefaults);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
    }

    private void SetupListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    private void LoadSettings()
    {
        // 音频设置
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
            OnMasterVolumeChanged(masterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            OnMusicVolumeChanged(musicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            OnSFXVolumeChanged(sfxVolume);
        }

        // 图形设置
        int qualityLevel = PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());
        if (qualityDropdown != null)
        {
            qualityDropdown.value = qualityLevel;
        }

        bool vSync = PlayerPrefs.GetInt(VSyncKey, 1) == 1;
        if (vSyncToggle != null)
        {
            vSyncToggle.isOn = vSync;
        }

        bool shadows = PlayerPrefs.GetInt(ShadowsKey, 1) == 1;
        if (shadowsToggle != null)
        {
            shadowsToggle.isOn = shadows;
        }

        bool particles = PlayerPrefs.GetInt(ParticlesKey, 1) == 1;
        if (particlesToggle != null)
        {
            particlesToggle.isOn = particles;
        }

        // 游戏设置
        float sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivity;
            OnSensitivityChanged(sensitivity);
        }

        bool easyMode = PlayerPrefs.GetInt(EasyModeKey, 0) == 1;
        if (easyModeToggle != null)
        {
            easyModeToggle.isOn = easyMode;
        }

        bool showTutorial = PlayerPrefs.GetInt(ShowTutorialKey, 1) == 1;
        if (showTutorialToggle != null)
        {
            showTutorialToggle.isOn = showTutorial;
        }

        // 控制方案
        int controlScheme = PlayerPrefs.GetInt(ControlSchemeKey, 0);
        if (controlSchemeDropdown != null)
        {
            controlSchemeDropdown.value = controlScheme;
        }
    }

    private void SaveSettings()
    {
        // 音频设置
        if (masterVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolumeSlider.value);
        }

        if (musicVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolumeSlider.value);
        }

        if (sfxVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolumeSlider.value);
        }

        // 图形设置
        if (qualityDropdown != null)
        {
            PlayerPrefs.SetInt(QualityLevelKey, qualityDropdown.value);
            QualitySettings.SetQualityLevel(qualityDropdown.value);
        }

        if (vSyncToggle != null)
        {
            PlayerPrefs.SetInt(VSyncKey, vSyncToggle.isOn ? 1 : 0);
            QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
        }

        if (shadowsToggle != null)
        {
            PlayerPrefs.SetInt(ShadowsKey, shadowsToggle.isOn ? 1 : 0);
        }

        if (particlesToggle != null)
        {
            PlayerPrefs.SetInt(ParticlesKey, particlesToggle.isOn ? 1 : 0);
        }

        // 游戏设置
        if (sensitivitySlider != null)
        {
            PlayerPrefs.SetFloat(SensitivityKey, sensitivitySlider.value);
        }

        if (easyModeToggle != null)
        {
            PlayerPrefs.SetInt(EasyModeKey, easyModeToggle.isOn ? 1 : 0);
        }

        if (showTutorialToggle != null)
        {
            PlayerPrefs.SetInt(ShowTutorialKey, showTutorialToggle.isOn ? 1 : 0);
        }

        // 控制方案
        if (controlSchemeDropdown != null)
        {
            PlayerPrefs.SetInt(ControlSchemeKey, controlSchemeDropdown.value);
        }

        PlayerPrefs.Save();
        Debug.Log("设置已保存");
    }

    private void ResetToDefaults()
    {
        // 重置为默认值
        if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
        if (musicVolumeSlider != null) musicVolumeSlider.value = 0.8f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
        if (qualityDropdown != null) qualityDropdown.value = QualitySettings.GetQualityLevel();
        if (vSyncToggle != null) vSyncToggle.isOn = true;
        if (shadowsToggle != null) shadowsToggle.isOn = true;
        if (particlesToggle != null) particlesToggle.isOn = true;
        if (sensitivitySlider != null) sensitivitySlider.value = 1f;
        if (easyModeToggle != null) easyModeToggle.isOn = false;
        if (showTutorialToggle != null) showTutorialToggle.isOn = true;
        if (controlSchemeDropdown != null) controlSchemeDropdown.value = 0;

        SaveSettings();
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (masterVolumeText != null)
        {
            masterVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
        AudioListener.volume = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    private void OnSensitivityChanged(float value)
    {
        if (sensitivityText != null)
        {
            sensitivityText.text = $"{value:F1}";
        }
    }

    private void CloseSettings()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMainMenu();
        }
    }

    public float GetMasterVolume()
    {
        return masterVolumeSlider != null ? masterVolumeSlider.value : 1f;
    }

    public float GetMusicVolume()
    {
        return musicVolumeSlider != null ? musicVolumeSlider.value : 0.8f;
    }

    public float GetSFXVolume()
    {
        return sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f;
    }

    public bool IsEasyMode()
    {
        return easyModeToggle != null && easyModeToggle.isOn;
    }

    public bool ShouldShowTutorial()
    {
        return showTutorialToggle != null && showTutorialToggle.isOn;
    }
}
