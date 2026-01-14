using UnityEngine;
using System.Collections;

namespace NetworkSystem
{
    /// <summary>
    /// 网络配置 - ScriptableObject配置后端URL
    /// </summary>
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "Game/Network Config")]
    public class NetworkConfig : ScriptableObject
    {
        [Header("后端API配置")]
        [SerializeField] private string backendBaseUrl = "";
        [SerializeField] private string apiVersion = "v1";
        [SerializeField] private int timeout = 10;
        [SerializeField] private int maxRetries = 3;

        [Header("API端点")]
        [SerializeField] private string loginEndpoint = "/auth/login";
        [SerializeField] private string refreshTokenEndpoint = "/auth/refresh";
        [SerializeField] private string saveEndpoint = "/save/data";
        [SerializeField] private string loadEndpoint = "/save/data";
        [SerializeField] private string leaderboardEndpoint = "/leaderboard";
        [SerializeField] private string achievementEndpoint = "/achievements";

        [Header("功能开关")]
        [SerializeField] private bool enableNetworkFeatures = true;
        [SerializeField] private bool allowOfflineMode = true;
        [SerializeField] private bool syncLocalSave = true;

        /// <summary>
        /// 是否启用网络功能
        /// </summary>
        public bool IsNetworkEnabled => !string.IsNullOrEmpty(backendBaseUrl) && enableNetworkFeatures;

        /// <summary>
        /// 获取完整的API URL
        /// </summary>
        public string GetFullUrl(string endpoint)
        {
            if (string.IsNullOrEmpty(backendBaseUrl))
            {
                return "";
            }
            return $"{backendBaseUrl}/api/{apiVersion}{endpoint}";
        }

        /// <summary>
        /// 获取登录URL
        /// </summary>
        public string GetLoginUrl() => GetFullUrl(loginEndpoint);

        /// <summary>
        /// 获取刷新Token URL
        /// </summary>
        public string GetRefreshTokenUrl() => GetFullUrl(refreshTokenEndpoint);

        /// <summary>
        /// 获取存档URL
        /// </summary>
        public string GetSaveUrl() => GetFullUrl(saveEndpoint);

        /// <summary>
        /// 获取加载URL
        /// </summary>
        public string GetLoadUrl() => GetFullUrl(loadEndpoint);

        /// <summary>
        /// 获取排行榜URL
        /// </summary>
        public string GetLeaderboardUrl() => GetFullUrl(leaderboardEndpoint);

        /// <summary>
        /// 获取成就URL
        /// </summary>
        public string GetAchievementUrl() => GetFullUrl(achievementEndpoint);

        /// <summary>
        /// 获取超时时间
        /// </summary>
        public int GetTimeout() => timeout;

        /// <summary>
        /// 获取最大重试次数
        /// </summary>
        public int GetMaxRetries() => maxRetries;

        /// <summary>
        /// 是否允许离线模式
        /// </summary>
        public bool IsOfflineModeAllowed() => allowOfflineMode;

        /// <summary>
        /// 是否同步本地存档
        /// </summary>
        public bool ShouldSyncLocalSave() => syncLocalSave;
    }
}
