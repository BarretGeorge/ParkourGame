using UnityEngine;

namespace NetworkSystem
{
    /// <summary>
    /// 网络状态指示器 - 显示网络连接状态
    /// </summary>
    public class NetworkStatusIndicator : MonoBehaviour
    {
        [Header("UI设置")]
        [SerializeField] private GameObject onlineIndicator;
        [SerializeField] private GameObject offlineIndicator;
        [SerializeField] private GameObject syncingIndicator;

        [Header("位置")]
        [SerializeField] private Vector2 screenPosition = new Vector2(10, 10);
        [SerializeField] private Vector2 indicatorSize = new Vector2(20, 20);

        [Header("颜色")]
        [SerializeField] private Color onlineColor = Color.green;
        [SerializeField] private Color offlineColor = Color.red;
        [SerializeField] private Color syncingColor = Color.yellow;

        [Header("显示设置")]
        [SerializeField] private bool showOnNetworkOnly = true;
        [SerializeField] private float autoHideDelay = 3f;

        private Canvas canvas;
        private Image statusImage;
        private NetworkStatus currentStatus = NetworkStatus.OfflineOnly;
        private float hideTimer;

        private void Start()
        {
            CreateUI();
            UpdateNetworkStatus();

            // 监听网络状态变化
            if (HTTPClient.Instance != null)
            {
                HTTPClient.Instance.OnNetworkStatusChanged += HandleNetworkStatusChange;
            }

            if (HybridSaveManager.Instance != null)
            {
                HybridSaveManager.Instance.OnSyncCompleted += HandleSyncCompleted;
            }
        }

        private void CreateUI()
        {
            // 查找或创建Canvas
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("NetworkStatusCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.sortingOrder = 9999; // 确保在最上层
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<UIRaycaster>(); // 虽然可能不需要，但保持一致性
            }

            // 创建状态图像
            GameObject statusObj = new GameObject("NetworkStatusImage");
            statusObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = statusObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = screenPosition;
            rect.sizeDelta = indicatorSize;

            statusImage = statusObj.AddComponent<Image>();
            statusImage.color = offlineColor;

            // 添加按钮组件（用于点击查看详细信息）
            UnityEngine.UI.Button button = statusObj.AddComponent<UnityEngine.UI.Button>();
        }

        private void Update()
        {
            NetworkStatus newStatus = HybridSaveManager.Instance?.GetNetworkStatus() ?? NetworkStatus.OfflineOnly;

            if (newStatus != currentStatus)
            {
                currentStatus = newStatus;
                UpdateNetworkStatus();
            }

            // 自动隐藏逻辑
            if (showOnNetworkOnly && currentStatus == NetworkStatus.OfflineOnly)
            {
                HideIndicator();
            }
            else
            {
                ShowIndicator();

                // 如果不是同步状态，启动隐藏计时器
                if (currentStatus != NetworkStatus.Syncing)
                {
                    hideTimer += Time.deltaTime;
                    if (hideTimer >= autoHideDelay)
                    {
                        HideIndicator();
                    }
                }
            }
        }

        private void HandleNetworkStatusChange(bool isConnected)
        {
            // 网络状态变化时更新
            UpdateNetworkStatus();
        }

        private void HandleSyncCompleted(bool success)
        {
            if (success)
            {
                ShowSyncing();
                StartCoroutine(HideSyncingAfterDelay(1f));
            }
        }

        private System.Collections.IEnumerator HideSyncingAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateNetworkStatus();
        }

        private void UpdateNetworkStatus()
        {
            if (statusImage == null) return;

            switch (currentStatus)
            {
                case NetworkStatus.Online:
                    statusImage.color = onlineColor;
                    break;
                case NetworkStatus.Offline:
                    statusImage.color = offlineColor;
                    break;
                case NetworkStatus.OfflineOnly:
                    statusImage.color = offlineColor;
                    break;
                case NetworkStatus.Syncing:
                    statusImage.color = syncingColor;
                    break;
            }
        }

        private void ShowIndicator()
        {
            if (statusImage != null && statusImage.gameObject != null)
            {
                statusImage.gameObject.SetActive(true);
            }
        }

        private void HideIndicator()
        {
            if (statusImage != null && statusImage.gameObject != null)
            {
                statusImage.gameObject.SetActive(false);
            }
        }

        public void ShowSyncing()
        {
            currentStatus = NetworkStatus.Syncing;
            ShowIndicator();
        }

        private void OnDestroy()
        {
            if (HTTPClient.Instance != null)
            {
                HTTPClient.Instance.OnNetworkStatusChanged -= HandleNetworkStatusChange;
            }

            if (HybridSaveManager.Instance != null)
            {
                HybridSaveManager.Instance.OnSyncCompleted -= HandleSyncCompleted;
            }
        }

        public void SetPosition(Vector2 position)
        {
            screenPosition = position;
            if (statusImage != null)
            {
                RectTransform rect = statusImage.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = position;
                }
            }
        }

        public void SetSize(Vector2 size)
        {
            indicatorSize = size;
            if (statusImage != null)
            {
                RectTransform rect = statusImage.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = size;
                }
            }
        }
    }
}
