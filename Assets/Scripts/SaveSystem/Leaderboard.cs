using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 排行榜系统
/// 支持本地排行榜和在线排行榜混合模式
/// </summary>
[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public float distance;
    public int coins;
    public string timestamp;

    public LeaderboardEntry(string name, int scoreValue, float distanceValue, int coinsValue)
    {
        playerName = name;
        score = scoreValue;
        distance = distanceValue;
        coins = coinsValue;
        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

public class Leaderboard : MonoBehaviour
{
    [Header("排行榜设置")]
    [SerializeField] private int maxEntries = 100;
    [SerializeField] private string defaultPlayerName = "Player";

    // 本地排行榜
    private List<LeaderboardEntry> localLeaderboard = new List<LeaderboardEntry>();

    // 在线排行榜
    private List<LeaderboardEntry> onlineLeaderboard = new List<LeaderboardEntry>();
    private bool onlineLeaderboardLoaded = false;

    // API客户端（可选）
    private APIClient apiClient;

    // 单例
    private static Leaderboard _instance;
    public static Leaderboard Instance => _instance;

    /// <summary>
    /// 是否启用在线排行榜
    /// </summary>
    public bool IsOnlineEnabled => apiClient != null && SaveManager.Instance != null && SaveManager.Instance.IsNetworkMode;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeOnlineLeaderboard();
            LoadLeaderboard();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化在线排行榜
    /// </summary>
    private void InitializeOnlineLeaderboard()
    {
        // 查找APIClient
        apiClient = FindObjectOfType<APIClient>();

        // 如果启用在线模式，自动加载在线排行榜
        if (IsOnlineEnabled)
        {
            RefreshOnlineLeaderboard();
        }
    }

    #region 添加分数

    public void AddScore(int score, float distance, int coins, string playerName = null)
    {
        string name = playerName ?? defaultPlayerName;
        LeaderboardEntry entry = new LeaderboardEntry(name, score, distance, coins);

        // 添加到本地排行榜
        localLeaderboard.Add(entry);

        // 按分数排序
        localLeaderboard = localLeaderboard
            .OrderByDescending(e => e.score)
            .ToList();

        // 限制条目数量
        if (localLeaderboard.Count > maxEntries)
        {
            localLeaderboard = localLeaderboard.Take(maxEntries).ToList();
        }

        SaveLeaderboard();

        // 如果启用在线模式，提交到服务器
        if (IsOnlineEnabled && apiClient != null)
        {
            SubmitToOnlineLeaderboard(score, distance, coins, name);
        }
    }

    public bool IsHighScore(int score)
    {
        if (localLeaderboard.Count < maxEntries)
        {
            return true;
        }

        return score > localLeaderboard[lastPlaceIndex].score;
    }

    #endregion

    #region 获取排行榜

    public List<LeaderboardEntry> GetTopScores(int count)
    {
        return localLeaderboard.Take(count).ToList();
    }

    public List<LeaderboardEntry> GetTopScores(int count, int startIndex)
    {
        if (startIndex < 0 || startIndex >= localLeaderboard.Count)
        {
            return new List<LeaderboardEntry>();
        }

        return localLeaderboard
            .Skip(startIndex)
            .Take(count)
            .ToList();
    }

    public LeaderboardEntry GetEntry(int index)
    {
        if (index >= 0 && index < localLeaderboard.Count)
        {
            return localLeaderboard[index];
        }
        return null;
    }

    public int GetRank(int score)
    {
        for (int i = 0; i < localLeaderboard.Count; i++)
        {
            if (score >= localLeaderboard[i].score)
            {
                return i + 1;
            }
        }
        return localLeaderboard.Count + 1;
    }

    public int GetCount()
    {
        return localLeaderboard.Count;
    }

    private int lastPlaceIndex => Mathf.Max(0, localLeaderboard.Count - 1);

    #endregion

    #region 保存和加载

    private void SaveLeaderboard()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(new LeaderboardData(localLeaderboard), true);
            string filePath = GetLeaderboardFilePath();
            System.IO.File.WriteAllText(filePath, jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存排行榜失败: {e.Message}");
        }
    }

    private void LoadLeaderboard()
    {
        try
        {
            string filePath = GetLeaderboardFilePath();
            if (System.IO.File.Exists(filePath))
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(jsonData);
                if (data != null && data.entries != null)
                {
                    localLeaderboard = data.entries;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载排行榜失败: {e.Message}");
            localLeaderboard = new List<LeaderboardEntry>();
        }
    }

    private string GetLeaderboardFilePath()
    {
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        return System.IO.Path.Combine(directory, "leaderboard.dat");
    }

    #endregion

    #region 清空和重置

    public void ClearLeaderboard()
    {
        localLeaderboard.Clear();
        SaveLeaderboard();
    }

    public void ResetEntry(int index)
    {
        if (index >= 0 && index < localLeaderboard.Count)
        {
            localLeaderboard.RemoveAt(index);
            SaveLeaderboard();
        }
    }

    #endregion

    #region 统计

    public int GetAverageScore()
    {
        if (localLeaderboard.Count == 0) return 0;
        return (int)localLeaderboard.Average(e => e.score);
    }

    public int GetHighestScore()
    {
        if (localLeaderboard.Count == 0) return 0;
        return localLeaderboard[0].score;
    }

    public float GetAverageDistance()
    {
        if (localLeaderboard.Count == 0) return 0f;
        return localLeaderboard.Average(e => e.distance);
    }

    #endregion

    #region 在线排行榜

    /// <summary>
    /// 提交分数到在线排行榜
    /// </summary>
    private void SubmitToOnlineLeaderboard(int score, float distance, int coins, string playerName)
    {
        if (apiClient == null) return;

        apiClient.SubmitScore(playerName, score, distance, coins, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"分数已提交到在线排行榜: {score}");
            }
            else
            {
                Debug.LogWarning($"提交分数失败: {message}");
            }
        });
    }

    /// <summary>
    /// 刷新在线排行榜数据
    /// </summary>
    public void RefreshOnlineLeaderboard(System.Action<bool, string> callback = null)
    {
        if (apiClient == null || !IsOnlineEnabled)
        {
            callback?.Invoke(false, "在线排行榜未启用");
            return;
        }

        apiClient.GetLeaderboard(0, 100, (success, entries, message) =>
        {
            if (success && entries != null)
            {
                onlineLeaderboard = entries;
                onlineLeaderboardLoaded = true;
                Debug.Log($"在线排行榜已更新，共 {entries.Count} 条记录");
            }
            else
            {
                Debug.LogWarning($"获取在线排行榜失败: {message}");
            }
            callback?.Invoke(success, message);
        });
    }

    /// <summary>
    /// 获取在线排行榜
    /// </summary>
    public List<LeaderboardEntry> GetOnlineLeaderboard()
    {
        return onlineLeaderboard;
    }

    /// <summary>
    /// 获取混合排行榜（在线+本地）
    /// </summary>
    public List<LeaderboardEntry> GetHybridLeaderboard(int count)
    {
        List<LeaderboardEntry> result = new List<LeaderboardEntry>();

        // 如果有在线排行榜，优先使用
        if (onlineLeaderboardLoaded && onlineLeaderboard.Count > 0)
        {
            result.AddRange(onlineLeaderboard.Take(count));
        }
        else
        {
            // 否则使用本地排行榜
            result.AddRange(localLeaderboard.Take(count));
        }

        return result;
    }

    /// <summary>
    /// 获取玩家在全球排行榜中的排名
    /// </summary>
    public void GetPlayerGlobalRank(string playerName, System.Action<int> callback)
    {
        if (apiClient == null || !IsOnlineEnabled)
        {
            callback?.Invoke(-1);
            return;
        }

        apiClient.GetLeaderboard(0, 1000, (success, entries, message) =>
        {
            if (success && entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].playerName == playerName)
                    {
                        callback?.Invoke(i + 1);
                        return;
                    }
                }
                callback?.Invoke(-1); // 未找到
            }
            else
            {
                callback?.Invoke(-1);
            }
        });
    }

    #endregion
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries;

    public LeaderboardData(List<LeaderboardEntry> entriesList)
    {
        entries = entriesList;
    }
}
