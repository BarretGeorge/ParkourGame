using UnityEngine;

/// <summary>
/// 编辑器测试工具 - 用于快速测试和调试
/// </summary>
public class EditorTestTools : MonoBehaviour
{
    [Header("测试功能")]
    [SerializeField] private bool enableTeleport = true;
    [SerializeField] private bool enableSpeedControl = true;
    [SerializeField] private bool enableInvincibility = false;

    [Header("传送设置")]
    [SerializeField] private int teleportLane = 1;
    [SerializeField] private float teleportDistance = 100f;

    [Header("速度控制")]
    [SerializeField] private float testSpeed = 20f;
    [SerializeField] private bool useTestSpeed = false;

    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // 仅在编辑器中可用
        if (!Application.isEditor) return;

        // F1 - 传送
        if (enableTeleport && Input.GetKeyDown(KeyCode.F1))
        {
            TeleportPlayer();
        }

        // F2 - 设置速度
        if (enableSpeedControl && Input.GetKeyDown(KeyCode.F2))
        {
            if (useTestSpeed)
            {
                playerController.SetSpeed(testSpeed);
                Debug.Log($"Speed set to {testSpeed}");
            }
            else
            {
                playerController.SetSpeed(playerController.PlayerData.baseSpeed);
                Debug.Log("Speed reset to base");
            }
            useTestSpeed = !useTestSpeed;
        }

        // F3 - 切换无敌模式
        if (Input.GetKeyDown(KeyCode.F3))
        {
            enableInvincibility = !enableInvincibility;
            Debug.Log($"Invincibility: {enableInvincibility}");
        }

        // F5 - 重置玩家
        if (Input.GetKeyDown(KeyCode.F5))
        {
            playerController.ResetPlayer();
            Debug.Log("Player reset");
        }

        // F10 - 暂停/继续
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (Time.timeScale == 0)
            {
                playerController.ResumeGame();
                Debug.Log("Game resumed");
            }
            else
            {
                playerController.PauseGame();
                Debug.Log("Game paused");
            }
        }
    }

    private void TeleportPlayer()
    {
        // 传送到指定距离和车道
        if (playerController != null)
        {
            Vector3 pos = transform.position;
            pos.z = teleportDistance;
            // 注意：这需要访问LaneManager，这里只是示例
            Debug.Log($"Teleporting to distance {teleportDistance}");
        }
    }

    private void OnGUI()
    {
        if (!Application.isEditor) return;

        GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 200, 300));
        GUILayout.Box("Debug Tools");

        GUILayout.Label("F1: Teleport");
        GUILayout.Label("F2: Toggle Speed");
        GUILayout.Label("F3: Toggle Invincibility");
        GUILayout.Label("F5: Reset Player");
        GUILayout.Label("F10: Pause/Resume");

        GUILayout.Label($"Invincibility: {enableInvincibility}");

        GUILayout.EndArea();
    }
}
