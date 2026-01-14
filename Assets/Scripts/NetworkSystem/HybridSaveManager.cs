using UnityEngine;
using System;

namespace NetworkSystem
{
    /// <summary>
    /// 混合存档管理器 - 自动在网络API和本地存档之间切换
    /// </summary>
    public class HybridSaveManager : MonoBehaviour
    {
        [Header("同步设置")]
        [SerializeField] private float autoSyncInterval = 30f;
        [SerializeField] private bool syncOnApplicationPause = true;
        [SerializeField] private bool syncOnApplicationQuit = true;

        [Header("冲突解决")]
        [SerializeField] private ConflictResolution conflictResolution = ConflictResolution.UseLatest;

        private SaveData localSaveData;
        private SaveData remoteSaveData;
        private NetworkConfig config;
        private APIClient apiClient;

        private float autoSyncTimer;
        private bool hasLocalChanges;
        private bool hasRemoteChanges;

        // 单例
        private static HybridSaveManager _instance;
        public static HybridSaveManager Instance => _instance;

        // 事件
        public event Action<bool> OnSyncCompleted;
        public event Action<string> OnSyncError;
        public event Action<ConflictResolution> OnConflictResolved;
        public event Action<SaveData, SaveData, System.Action<ConflictResolution>> OnConflictResolutionNeeded;

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
            apiClient = APIClient.Instance;
            config = apiClient.GetConfig();

            // 检查是否为网络模式
            if (IsNetworkMode())
            {
                Debug.Log("网络模式已启用 - 将同步存档到服务器");
                // 首次加载时从服务器下载存档
                DownloadFromServer(null);
            }
            else
            {
                Debug.Log("离线模式 - 仅使用本地存档");
            }
        }

        private void Update()
        {
            if (!IsNetworkMode()) return;

            autoSyncTimer += Time.deltaTime;

            if (autoSyncTimer >= autoSyncInterval)
            {
                autoSyncTimer = 0f;

                if (hasLocalChanges)
                {
                    UploadToServer(null);
                }
                else
                {
                    SyncWithServer(null);
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && syncOnApplicationPause)
            {
                SyncWithServer(null);
            }
        }

        private void OnApplicationQuit()
        {
            if (syncOnApplicationQuit)
            {
                SyncWithServer(null);
            }
        }

        #region 公共接口

        /// <summary>
        /// 保存游戏（自动选择本地或网络）
        /// </summary>
        public void SaveGame(SaveData saveData, System.Action<bool, string> callback = null)
        {
            localSaveData = saveData;
            hasLocalChanges = true;

            if (IsNetworkMode())
            {
                // 网络模式：先保存本地，然后上传到服务器
                SaveToLocal(saveData);
                UploadToServer(callback);
            }
            else
            {
                // 离线模式：仅保存本地
                SaveToLocal(saveData);
                callback?.Invoke(true, "保存成功（本地）");
            }
        }

        /// <summary>
        /// 加载游戏（自动选择本地或网络）
        /// </summary>
        public void LoadGame(System.Action<SaveData, string> callback)
        {
            if (IsNetworkMode())
            {
                // 网络模式：优先从服务器加载
                DownloadFromServer(callback);
            }
            else
            {
                // 离线模式：从本地加载
                SaveData data = LoadFromLocal();
                callback?.Invoke(data, "加载成功（本地）");
            }
        }

        /// <summary>
        /// 强制同步
        /// </summary>
        public void SyncWithServer(System.Action<bool, string> callback)
        {
            if (!IsNetworkMode())
            {
                callback?.Invoke(false, "网络模式未启用");
                return;
            }

            // 下载远程存档
            DownloadFromServer((remoteData, errorMsg) =>
            {
                if (remoteData != null)
                {
                    remoteSaveData = remoteData;

                    // 检查冲突
                    if (hasLocalChanges && localSaveData != null)
                    {
                        ResolveConflict((resolved) =>
                        {
                            if (resolved)
                            {
                                // 上传解决后的存档
                                UploadToServer(callback);
                            }
                            else
                            {
                                callback?.Invoke(false, "同步失败：冲突未解决");
                            }
                        });
                    }
                    else
                    {
                        // 无冲突，直接更新本地
                        SaveToLocal(remoteData);
                        callback?.Invoke(true, "同步成功");
                    }
                }
                else
                {
                    // 服务器无数据，上传本地数据
                    if (localSaveData != null)
                    {
                        UploadToServer(callback);
                    }
                    else
                    {
                        callback?.Invoke(false, errorMsg);
                    }
                }
            });
        }

        /// <summary>
        /// 仅上传到服务器
        /// </summary>
        public void UploadToServer(System.Action<bool, string> callback)
        {
            if (!IsNetworkMode())
            {
                callback?.Invoke(false, "网络模式未启用");
                return;
            }

            if (localSaveData == null)
            {
                callback?.Invoke(false, "没有本地存档数据");
                return;
            }

            apiClient.UploadSaveData(localSaveData, (success, message) =>
            {
                if (success)
                {
                    hasLocalChanges = false;
                    OnSyncCompleted?.Invoke(true);
                }
                else
                {
                    OnSyncError?.Invoke(message);
                }

                callback?.Invoke(success, message);
            });
        }

        /// <summary>
        /// 仅从服务器下载
        /// </summary>
        public void DownloadFromServer(System.Action<SaveData, string> callback)
        {
            if (!IsNetworkMode())
            {
                callback?.Invoke(null, "网络模式未启用");
                return;
            }

            apiClient.DownloadSaveData((saveData, message) =>
            {
                if (saveData != null)
                {
                    remoteSaveData = saveData;
                    SaveToLocal(saveData);
                }

                callback?.Invoke(saveData, message);
            });
        }

        #endregion

        #region 本地存档

        private void SaveToLocal(SaveData saveData)
        {
            // 使用现有的SaveManager保存到本地
            if (SaveManager.Instance != null)
            {
                // 更新SaveManager的存档数据
                // 注意：这里需要访问SaveManager的内部数据
                // 可能需要添加一个公共方法来设置存档数据
            }

            // 保存到PlayerPrefs作为备份
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = GetLocalSaveFilePath();
            System.IO.File.WriteAllText(filePath, json);
        }

        private SaveData LoadFromLocal()
        {
            // 优先从SaveManager加载
            if (SaveManager.Instance != null)
            {
                return SaveManager.Instance.GetSaveData();
            }

            // 从文件加载
            string filePath = GetLocalSaveFilePath();
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                return JsonUtility.FromJson<SaveData>(json);
            }

            return new SaveData();
        }

        private string GetLocalSaveFilePath()
        {
            string directory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            return System.IO.Path.Combine(directory, "local_save.dat");
        }

        #endregion

        #region 冲突解决

        private void ResolveConflict(System.Action<bool> callback)
        {
            ConflictResolution resolution = conflictResolution;

            switch (conflictResolution)
            {
                case ConflictResolution.UseLocal:
                    // 使用本地存档，覆盖服务器
                    localSaveData.UpdateTimestamp();
                    remoteSaveData = localSaveData;
                    Debug.Log("冲突解决：使用本地存档");
                    break;

                case ConflictResolution.UseRemote:
                    // 使用服务器存档，覆盖本地
                    SaveToLocal(remoteSaveData);
                    localSaveData = remoteSaveData;
                    Debug.Log("冲突解决：使用服务器存档");
                    break;

                case ConflictResolution.UseLatest:
                    // 使用最新的存档（比较时间戳）
                    DateTime localTime = DateTime.Parse(localSaveData.lastSaveTime);
                    DateTime remoteTime = DateTime.Parse(remoteSaveData.lastSaveTime);

                    if (localTime > remoteTime)
                    {
                        resolution = ConflictResolution.UseLocal;
                        localSaveData.UpdateTimestamp();
                        remoteSaveData = localSaveData;
                        Debug.Log("冲突解决：使用最新存档（本地）");
                    }
                    else
                    {
                        resolution = ConflictResolution.UseRemote;
                        SaveToLocal(remoteSaveData);
                        localSaveData = remoteSaveData;
                        Debug.Log("冲突解决：使用最新存档（服务器）");
                    }
                    break;

                case ConflictResolution.Manual:
                    // 手动解决（可以显示UI让用户选择）
                    Debug.LogWarning("需要手动解决存档冲突");
                    // 触发事件让UI系统处理
                    OnConflictResolutionNeeded?.Invoke(localSaveData, remoteSaveData, (resolution) =>
                    {
                        ApplyManualResolution(resolution, callback);
                    });
                    return; // 等待用户选择
            }

            OnConflictResolved?.Invoke(resolution);
            callback?.Invoke(true);
        }

        /// <summary>
        /// 应用手动选择的解决方案
        /// </summary>
        private void ApplyManualResolution(ConflictResolution resolution, System.Action<bool> callback)
        {
            switch (resolution)
            {
                case ConflictResolution.UseLocal:
                    localSaveData.UpdateTimestamp();
                    remoteSaveData = localSaveData;
                    Debug.Log("手动选择：使用本地存档");
                    break;

                case ConflictResolution.UseRemote:
                    SaveToLocal(remoteSaveData);
                    localSaveData = remoteSaveData;
                    Debug.Log("手动选择：使用服务器存档");
                    break;

                default:
                    callback?.Invoke(false);
                    return;
            }

            OnConflictResolved?.Invoke(resolution);
            callback?.Invoke(true);
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 是否为网络模式
        /// </summary>
        public bool IsNetworkMode()
        {
            return config != null && config.IsNetworkEnabled && apiClient.CheckConnection();
        }

        /// <summary>
        /// 是否允许离线模式
        /// </summary>
        public bool IsOfflineModeAllowed()
        {
            return config != null && config.IsOfflineModeAllowed();
        }

        /// <summary>
        /// 获取网络状态
        /// </summary>
        public NetworkStatus GetNetworkStatus()
        {
            if (!IsNetworkMode())
            {
                return NetworkStatus.OfflineOnly;
            }

            if (!apiClient.CheckConnection())
            {
                return NetworkStatus.Offline;
            }

            return NetworkStatus.Online;
        }

        /// <summary>
        /// 设置冲突解决策略
        /// </summary>
        public void SetConflictResolution(ConflictResolution resolution)
        {
            conflictResolution = resolution;
            Debug.Log($"冲突解决策略设置为: {resolution}");
        }

        #endregion
    }

    /// <summary>
    /// 冲突解决策略
    /// </summary>
    public enum ConflictResolution
    {
        UseLocal,        // 优先使用本地存档
        UseRemote,       // 优先使用服务器存档
        UseLatest,       // 使用最新的存档（比较时间戳）
        Manual           // 手动选择
    }

    /// <summary>
    /// 网络状态
    /// </summary>
    public enum NetworkStatus
    {
        Online,          // 在线
        Offline,         // 离线（网络不可用）
        OfflineOnly      // 仅离线模式（未配置后端）
    }
}
