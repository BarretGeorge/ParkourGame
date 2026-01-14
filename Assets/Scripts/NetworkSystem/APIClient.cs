using UnityEngine;
using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

namespace NetworkSystem
{
    /// <summary>
    /// API响应基类
    /// </summary>
    [Serializable]
    public class APIResponse
    {
        public bool success;
        public string message;
        public int code;
        public string timestamp;
    }

    /// <summary>
    /// 存档API响应
    /// </summary>
    [Serializable]
    public class SaveDataResponse : APIResponse
    {
        public SaveData data;
    }

    /// <summary>
    /// 排行榜API响应
    /// </summary>
    [Serializable]
    public class LeaderboardResponse : APIResponse
    {
        public LeaderboardEntryData[] entries;
    }

    [Serializable]
    public class LeaderboardEntryData
    {
        public string playerName;
        public int score;
        public float distance;
        public int coins;
        public int rank;
    }

    /// <summary>
    /// 成就API响应
    /// </summary>
    [Serializable]
    public class AchievementResponse : APIResponse
    {
        public string[] unlockedAchievements;
    }

    /// <summary>
    /// 登录请求
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
        public string deviceId;
    }

    /// <summary>
    /// 登录响应
    /// </summary>
    [Serializable]
    public class LoginResponse : APIResponse
    {
        public string token;
        public string refreshToken;
        public UserData user;
    }

    [Serializable]
    public class UserData
    {
        public string id;
        public string username;
        public string email;
    }

    /// <summary>
    /// API客户端 - 提供所有API接口
    /// </summary>
    public class APIClient : MonoBehaviour
    {
        [Header("设置")]
        [SerializeField] private bool debugMode = false;

        private NetworkConfig config;
        private HTTPClient httpClient;

        // 单例
        private static APIClient _instance;
        public static APIClient Instance => _instance;

        // 事件
        public event Action<string> OnAPIError;
        public event Action OnLoginSuccess;
        public event Action OnLogoutSuccess;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            httpClient = HTTPClient.Instance;
            config = httpClient.GetConfig();

            if (config == null)
            {
                Debug.LogWarning("未找到网络配置，使用离线模式");
            }
        }

        #region 认证

        /// <summary>
        /// 登录
        /// </summary>
        public async void Login(string username, string password, System.Action<bool, string> callback)
        {
            if (config == null || !config.IsNetworkEnabled)
            {
                Debug.LogWarning("网络功能未启用，使用本地登录");
                callback?.Invoke(false, "网络功能未启用");
                return;
            }

            string url = config.GetLoginUrl();
            string deviceId = SystemInfo.deviceUniqueIdentifier;

            LoginRequest request = new LoginRequest
            {
                username = username,
                password = password,
                deviceId = deviceId
            };

            try
            {
                LoginResponse response = await httpClient.Post<LoginResponse>(url, request);

                if (response != null && response.success)
                {
                    // 保存token
                    httpClient.SetAuthToken(response.token);
                    PlayerPrefs.SetString("refresh_token", response.refreshToken);
                    PlayerPrefs.Save();

                    OnLoginSuccess?.Invoke();
                    callback?.Invoke(true, "登录成功");

                    if (debugMode) Debug.Log($"登录成功: {response.user.username}");
                }
                else
                {
                    string errorMsg = response != null ? response.message : "登录失败";
                    OnAPIError?.Invoke(errorMsg);
                    callback?.Invoke(false, errorMsg);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"登录异常: {e.Message}");
                OnAPIError?.Invoke(e.Message);
                callback?.Invoke(false, "网络错误");
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void Logout(System.Action<bool> callback)
        {
            // 清除本地token
            httpClient.ClearAuthToken();
            PlayerPrefs.DeleteKey("refresh_token");
            PlayerPrefs.Save();

            OnLogoutSuccess?.Invoke();
            callback?.Invoke(true);

            if (debugMode) Debug.Log("已登出");
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        public async void RefreshToken(System.Action<bool> callback)
        {
            string refreshToken = PlayerPrefs.GetString("refresh_token", "");

            if (string.IsNullOrEmpty(refreshToken))
            {
                callback?.Invoke(false);
                return;
            }

            if (config == null || !config.IsNetworkEnabled)
            {
                callback?.Invoke(false);
                return;
            }

            string url = config.GetRefreshTokenUrl();

            try
            {
                var refreshRequest = new
                {
                    refreshToken = refreshToken
                };

                LoginResponse response = await httpClient.Post<LoginResponse>(url, refreshRequest);

                if (response != null && response.success)
                {
                    // 保存新token
                    httpClient.SetAuthToken(response.token);
                    PlayerPrefs.SetString("refresh_token", response.refreshToken);
                    PlayerPrefs.Save();

                    if (debugMode) Debug.Log("Token刷新成功");
                    callback?.Invoke(true);
                }
                else
                {
                    string errorMsg = response != null ? response.message : "Token刷新失败";
                    OnAPIError?.Invoke(errorMsg);
                    callback?.Invoke(false);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Token刷新异常: {e.Message}");
                OnAPIError?.Invoke(e.Message);
                callback?.Invoke(false);
            }
        }

        #endregion

        #region 存档系统

        /// <summary>
        /// 上传存档到服务器
        /// </summary>
        public async void UploadSaveData(SaveData saveData, System.Action<bool, string> callback)
        {
            if (config == null || !config.IsNetworkEnabled)
            {
                callback?.Invoke(false, "网络功能未启用");
                return;
            }

            if (!httpClient.IsNetworkAvailable())
            {
                callback?.Invoke(false, "无网络连接");
                return;
            }

            string url = config.GetSaveUrl();

            try
            {
                bool success = await httpClient.Put(url, saveData);

                if (success)
                {
                    if (debugMode) Debug.Log("存档上传成功");
                    callback?.Invoke(true, "保存成功");
                }
                else
                {
                    OnAPIError?.Invoke("存档上传失败");
                    callback?.Invoke(false, "保存失败");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"上传存档异常: {e.Message}");
                OnAPIError?.Invoke(e.Message);
                callback?.Invoke(false, "网络错误");
            }
        }

        /// <summary>
        /// 从服务器下载存档
        /// </summary>
        public async void DownloadSaveData(System.Action<SaveData, string> callback)
        {
            if (config == null || !config.IsNetworkEnabled)
            {
                callback?.Invoke(null, "网络功能未启用");
                return;
            }

            if (!httpClient.IsNetworkAvailable())
            {
                callback?.Invoke(null, "无网络连接");
                return;
            }

            string url = config.GetLoadUrl();

            try
            {
                SaveDataResponse response = await httpClient.Get<SaveDataResponse>(url);

                if (response != null && response.success && response.data != null)
                {
                    if (debugMode) Debug.Log("存档下载成功");
                    callback?.Invoke(response.data, "下载成功");
                }
                else
                {
                    string errorMsg = response != null ? response.message : "下载失败";
                    callback?.Invoke(null, errorMsg);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"下载存档异常: {e.Message}");
                OnAPIError?.Invoke(e.Message);
                callback?.Invoke(null, "网络错误");
            }
        }

        #endregion

        #region 排行榜

        /// <summary>
        /// 上传分数到排行榜
        /// </summary>
        public async void SubmitScore(int score, float distance, int coins, System.Action<bool, string> callback)
        {
            if (config == null || !config.IsNetworkEnabled)
            {
                callback?.Invoke(false, "网络功能未启用");
                return;
            }

            string url = config.GetLeaderboardUrl();

            // 构造提交数据
            var scoreData = new
            {
                score = score,
                distance = distance,
                coins = coins,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            try
            {
                bool success = await httpClient.Post(url, scoreData);

                if (success)
                {
                    if (debugMode) Debug.Log($"分数提交成功: {score}");
                    callback?.Invoke(true, "提交成功");
                }
                else
                {
                    callback?.Invoke(false, "提交失败");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"提交分数异常: {e.Message}");
                callback?.Invoke(false, "网络错误");
            }
        }

        /// <summary>
        /// 获取排行榜
        /// </summary>
        public async void GetLeaderboard(int limit, System.Action<LeaderboardEntryData[], string> callback)
        {
            if (config == null || !config.IsNetworkEnabled)
            {
                callback?.Invoke(null, "网络功能未启用");
                return;
            }

            string url = $"{config.GetLeaderboardUrl()}?limit={limit}";

            try
            {
                LeaderboardResponse response = await httpClient.Get<LeaderboardResponse>(url);

                if (response != null && response.success && response.entries != null)
                {
                    if (debugMode) Debug.Log($"获取排行榜成功: {response.entries.Length}条记录");
                    callback?.Invoke(response.entries, "获取成功");
                }
                else
                {
                    callback?.Invoke(null, "获取失败");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"获取排行榜异常: {e.Message}");
                callback?.Invoke(null, "网络错误");
            }
        }

        #endregion

        #region 成就系统

        /// <summary>
        /// 同步成就到服务器
        /// </summary>
        public async void SyncAchievements(string[] unlockedAchievements, System.Action<bool, string> callback)
        {
            if (config == null || !config.IsNetworkEnabled)
            {
                callback?.Invoke(false, "网络功能未启用");
                return;
            }

            string url = config.GetAchievementUrl();

            var achievementData = new
            {
                achievements = unlockedAchievements
            };

            try
            {
                bool success = await httpClient.Post(url, achievementData);

                if (success)
                {
                    if (debugMode) Debug.Log("成就同步成功");
                    callback?.Invoke(true, "同步成功");
                }
                else
                {
                    callback?.Invoke(false, "同步失败");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"同步成就异常: {e.Message}");
                callback?.Invoke(false, "网络错误");
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 检查是否为网络模式
        /// </summary>
        public bool IsNetworkMode()
        {
            return config != null && config.IsNetworkEnabled && httpClient.IsNetworkAvailable();
        }

        /// <summary>
        /// 检查网络连接
        /// </summary>
        public bool CheckConnection()
        {
            return httpClient.IsNetworkAvailable();
        }

        #endregion
    }
}
