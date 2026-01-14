using UnityEngine;

/// <summary>
/// 作弊码管理器 - 用于开发和测试
/// </summary>
public class CheatCodeManager : MonoBehaviour
{
    [Header("作弊设置")]
    [SerializeField] private bool enableCheats = false; // 发布时设为false

    // 作弊码字典
    private System.Collections.Generic.Dictionary<string, System.Action> cheatCodes =
        new System.Collections.Generic.Dictionary<string, System.Action>();

    private PlayerController player;
    private CollectibleManager collectibleManager;

    // 单例
    private static CheatCodeManager _instance;
    public static CheatCodeManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCheatCodes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FindManagers();
    }

    private void FindManagers()
    {
        player = FindObjectOfType<PlayerController>();
        collectibleManager = FindObjectOfType<CollectibleManager>();
    }

    private void InitializeCheatCodes()
    {
        // 金币作弊
        RegisterCheat("addcoins", AddCoinsCheat);
        RegisterCheat("infinitecoins", InfiniteCoinsCheat);

        // 分数作弊
        RegisterCheat("setscore", SetScoreCheat);
        RegisterCheat("addscore", AddScoreCheat);

        // 无敌模式
        RegisterCheat("godmode", GodModeCheat);
        RegisterCheat("invincible", InvincibleCheat);

        // 速度作弊
        RegisterCheat("superspeed", SuperSpeedCheat);
        RegisterCheat("slomo", SlowMotionCheat);

        // 道具作弊
        RegisterCheat("allpowerups", AllPowerUpsCheat);
        RegisterCheat("infinitepowerups", InfinitePowerUpsCheat);

        // 关卡作弊
        RegisterCheat("skiplevel", SkipLevelCheat);
        RegisterCheat("unlockall", UnlockAllCheat);

        // 调试作弊
        RegisterCheat("showfps", ShowFPSCheat);
        RegisterCheat("showcolliders", ShowCollidersCheat);

        // 测试作弊
        RegisterCheat("kill", KillCheat);
        RegisterCheat("respawn", RespawnCheat);
    }

    private void RegisterCheat(string code, System.Action action)
    {
        if (!cheatCodes.ContainsKey(code.ToLower()))
        {
            cheatCodes[code.ToLower()] = action;
        }
    }

    private void Update()
    {
        if (!enableCheats) return;

        // 检测作弊码输入（可以改为UI按钮或其他输入方式）
        // 这里只是一个示例，实际项目中可能需要更复杂的输入系统
    }

    /// <summary>
    /// 执行作弊码
    /// </summary>
    public void ExecuteCheat(string cheatCode)
    {
        if (!enableCheats)
        {
            Debug.LogWarning("作弊码已禁用");
            return;
        }

        string code = cheatCode.ToLower();

        if (cheatCodes.ContainsKey(code))
        {
            cheatCodes[code]?.Invoke();
            Debug.Log($"作弊码已执行: {cheatCode}");
        }
        else
        {
            Debug.LogWarning($"无效的作弊码: {cheatCode}");
        }
    }

    #region 作弊码实现

    private void AddCoinsCheat()
    {
        if (player != null)
        {
            // player.AddCoins(1000);
            Debug.Log("添加1000金币");
        }
    }

    private void InfiniteCoinsCheat()
    {
        Debug.Log("无限金币已激活");
    }

    private void SetScoreCheat()
    {
        if (player != null)
        {
            // player.SetScore(10000);
            Debug.Log("设置分数为10000");
        }
    }

    private void AddScoreCheat()
    {
        if (player != null)
        {
            // player.AddScore(5000);
            Debug.Log("添加5000分数");
        }
    }

    private void GodModeCheat()
    {
        Debug.Log("上帝模式已激活");
        // TODO: 实现无敌逻辑
    }

    private void InvincibleCheat()
    {
        if (collectibleManager != null)
        {
            // 激活无敌道具
            Debug.Log("无敌模式已激活");
        }
    }

    private void SuperSpeedCheat()
    {
        if (player != null)
        {
            // player.SetSpeedMultiplier(2f);
            Debug.Log("超级速度已激活");
        }
    }

    private void SlowMotionCheat()
    {
        Time.timeScale = 0.5f;
        Debug.Log("慢动作已激活");
    }

    private void AllPowerUpsCheat()
    {
        Debug.Log("所有道具已激活");
        // TODO: 激活所有道具
    }

    private void InfinitePowerUpsCheat()
    {
        Debug.Log("无限道具已激活");
        // TODO: 实现无限道具
    }

    private void SkipLevelCheat()
    {
        Debug.Log("跳过关卡");
        // TODO: 跳到下一关
    }

    private void UnlockAllCheat()
    {
        Debug.Log("解锁所有内容");
        // TODO: 解锁所有角色、皮肤、成就
    }

    private void ShowFPSCheat()
    {
        var monitor = FindObjectOfType<PerformanceMonitor>();
        if (monitor != null)
        {
            monitor.SetDisplayOptions(true, false, false);
        }
        Debug.Log("FPS显示已切换");
    }

    private void ShowCollidersCheat()
    {
        Debug.Log("碰撞体显示已切换");
        // TODO: 显示/隐藏所有碰撞体
    }

    private void KillCheat()
    {
        if (player != null)
        {
            // player.Die();
            Debug.Log("玩家已死亡");
        }
    }

    private void RespawnCheat()
    {
        if (player != null)
        {
            // player.Respawn();
            Debug.Log("玩家已重生");
        }
    }

    #endregion

    /// <summary>
    /// 启用/禁用作弊
    /// </summary>
    public void SetCheatsEnabled(bool enabled)
    {
        enableCheats = enabled;
        Debug.Log($"作弊码已{(enabled ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 检查作弊是否启用
    /// </summary>
    public bool IsCheatsEnabled()
    {
        return enableCheats;
    }

    /// <summary>
    /// 获取所有作弊码
    /// </summary>
    public string[] GetAllCheatCodes()
    {
        string[] codes = new string[cheatCodes.Count];
        cheatCodes.Keys.CopyTo(codes, 0);
        return codes;
    }
}
