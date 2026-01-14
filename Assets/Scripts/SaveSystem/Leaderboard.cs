using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 排行榜系统
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

    // 单例
    private static Leaderboard _instance;
    public static Leaderboard Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLeaderboard();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 添加分数

    public void AddScore(int score, float distance, int coins, string playerName = null)
    {
        string name = playerName ?? defaultPlayerName;
        LeaderboardEntry entry = new LeaderboardEntry(name, score, distance, coins);

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
