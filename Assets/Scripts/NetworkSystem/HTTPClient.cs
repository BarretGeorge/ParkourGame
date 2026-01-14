using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetworkSystem
{
    /// <summary>
    /// HTTP客户端 - 封装UnityWebRequest
    /// </summary>
    public class HTTPClient : MonoBehaviour
    {
        [Header("设置")]
        [SerializeField] private int defaultTimeout = 10;

        private HttpClient httpClient;
        private NetworkConfig config;

        // 单例
        private static HTTPClient _instance;
        public static HTTPClient Instance => _instance;

        // 事件
        public event Action<bool> OnNetworkStatusChanged;
        public event Action<string> OnRequestError;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // 加载网络配置
            LoadNetworkConfig();

            // 初始化HttpClient
            if (config != null)
            {
                httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(config.GetTimeout())
                };

                // 设置默认headers
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Unity3DRunner/1.0");

                // 如果有token，添加到headers
                string token = GetAuthToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }
            }

            // 检查网络状态
            StartCoroutine(CheckNetworkStatus());
        }

        private void LoadNetworkConfig()
        {
            // 从Resources加载网络配置
            NetworkConfig[] configs = Resources.LoadAll<NetworkConfig>("");
            if (configs != null && configs.Length > 0)
            {
                config = configs[0];
            }
        }

        private IEnumerator CheckNetworkStatus()
        {
            while (true)
            {
                bool hasConnection = Application.internetReachability != NetworkReachability.NotReachable;
                OnNetworkStatusChanged?.Invoke(hasConnection);
                yield return new WaitForSeconds(5f);
            }
        }

        /// <summary>
        /// GET请求
        /// </summary>
        public async Task<T> Get<T>(string url, bool requireAuth = true)
        {
            if (!config.IsNetworkEnabled)
            {
                Debug.LogWarning("网络功能未启用");
                return default(T);
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("无网络连接");
                return default(T);
            }

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonResponse);
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"GET请求失败: {e.Message}");
                OnRequestError?.Invoke(e.Message);
                return default(T);
            }
            catch (Exception e)
            {
                Debug.LogError($"GET请求异常: {e.Message}");
                OnRequestError?.Invoke(e.Message);
                return default(T);
            }
        }

        /// <summary>
        /// POST请求
        /// </summary>
        public async Task<T> Post<T>(string url, object data, bool requireAuth = true)
        {
            if (!config.IsNetworkEnabled)
            {
                Debug.LogWarning("网络功能未启用");
                return default(T);
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("无网络连接");
                return default(T);
            }

            try
            {
                string json = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonResponse);
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"POST请求失败: {e.Message}");
                OnRequestError?.Invoke(e.Message);
                return default(T);
            }
            catch (Exception e)
            {
                Debug.LogError($"POST请求异常: {e.Message}");
                OnRequestError?.Invoke(e.Message);
                return default(T);
            }
        }

        /// <summary>
        /// PUT请求
        /// </summary>
        public async Task<bool> Put(string url, object data, bool requireAuth = true)
        {
            if (!config.IsNetworkEnabled)
            {
                Debug.LogWarning("网络功能未启用");
                return false;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("无网络连接");
                return false;
            }

            try
            {
                string json = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PutAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"PUT请求失败: {e.Message}");
                OnRequestError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// DELETE请求
        /// </summary>
        public async Task<bool> Delete(string url, bool requireAuth = true)
        {
            if (!config.IsNetworkEnabled)
            {
                Debug.LogWarning("网络功能未启用");
                return false;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("无网络连接");
                return false;
            }

            try
            {
                HttpResponseMessage response = await httpClient.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"DELETE请求失败: {e.Message}");
                OnRequestError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查网络连接
        /// </summary>
        public bool IsNetworkAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>
        /// 设置认证Token
        /// </summary>
        public void SetAuthToken(string token)
        {
            PlayerPrefs.SetString("auth_token", token);
            PlayerPrefs.Save();

            if (httpClient != null)
            {
                if (string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }
            }
        }

        /// <summary>
        /// 获取认证Token
        /// </summary>
        public string GetAuthToken()
        {
            return PlayerPrefs.GetString("auth_token", "");
        }

        /// <summary>
        /// 清除认证Token
        /// </summary>
        public void ClearAuthToken()
        {
            SetAuthToken("");
        }

        /// <summary>
        /// 获取网络配置
        /// </summary>
        public NetworkConfig GetConfig()
        {
            return config;
        }
    }
}
