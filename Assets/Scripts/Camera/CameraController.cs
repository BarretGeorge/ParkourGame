using UnityEngine;

/// <summary>
/// 相机跟随控制器 - 平滑跟随玩家并添加动态效果
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("目标设置")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindPlayer = true;

    [Header("位置偏移")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 5, -10);
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0, 2, 0);

    [Header("跟随设置")]
    [SerializeField] private float positionSmoothSpeed = 10f;
    [SerializeField] private float rotationSmoothSpeed = 5f;
    [SerializeField] private bool followOnX = true;
    [SerializeField] private bool followOnY = true;
    [SerializeField] private bool followOnZ = true;

    [Header("动态效果")]
    [SerializeField] private bool enableSpeedBasedFOV = true;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 80f;
    [SerializeField] private float fovChangeSpeed = 2f;

    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.3f;

    [SerializeField] private bool enableTilt = true;
    [SerializeField] private float maxTiltAngle = 5f;
    [SerializeField] private float tiltSmoothSpeed = 5f;

    [Header("死亡效果")]
    [SerializeField] private bool enableDeathEffect = true;
    [SerializeField] private float deathSlowMotionFactor = 0.3f;
    [SerializeField] private float deathRotationAmount = 180f;

    // 组件
    private Camera mainCamera;
    private PlayerController playerController;

    // 状态
    private Vector3 currentVelocity;
    private Quaternion targetRotation;
    private float currentFOV;
    private float currentTilt;
    private bool isDying = false;
    private float deathTimer = 0f;
    private Vector3 shakeOffset;

    // 目标属性
    public Transform Target
    {
        get => target;
        set => target = value;
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        currentFOV = minFOV;

        // 自动查找玩家
        if (autoFindPlayer && target == null)
        {
            FindPlayer();
        }

        if (target != null)
        {
            // 初始位置设置
            transform.position = target.position + positionOffset;
            transform.LookAt(target.position + lookAtOffset);
        }
    }

    private void Start()
    {
        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.OnPlayerDeath += OnPlayerDeath;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (isDying)
        {
            UpdateDeathEffect();
        }
        else
        {
            UpdateNormalFollow();
        }

        UpdateCameraEffects();
    }

    #region 正常跟随

    private void UpdateNormalFollow()
    {
        // 计算目标位置
        Vector3 targetPosition = target.position + positionOffset;

        // 应用相机震动
        targetPosition += shakeOffset;

        // 选择性跟随轴
        if (!followOnX) targetPosition.x = transform.position.x;
        if (!followOnY) targetPosition.y = transform.position.y;
        if (!followOnZ) targetPosition.z = transform.position.z;

        // 平滑移动
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / positionSmoothSpeed
        );

        // 平滑旋转
        Quaternion targetRotation = Quaternion.LookRotation(
            (target.position + lookAtOffset) - transform.position
        );

        // 应用倾斜
        if (enableTilt)
        {
            targetRotation *= Quaternion.Euler(0, 0, currentTilt);
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    #endregion

    #region 相机效果

    private void UpdateCameraEffects()
    {
        // 速度FOV
        if (enableSpeedBasedFOV && playerController != null && mainCamera != null)
        {
            float targetFOV = Mathf.Lerp(
                minFOV,
                maxFOV,
                (playerController.CurrentSpeed - playerController.PlayerData.baseSpeed) /
                (playerController.PlayerData.maxSpeed - playerController.PlayerData.baseSpeed)
            );

            currentFOV = Mathf.Lerp(
                currentFOV,
                targetFOV,
                fovChangeSpeed * Time.deltaTime
            );

            mainCamera.fieldOfView = currentFOV;
        }

        // 相机震动衰减
        if (shakeOffset.magnitude > 0.01f)
        {
            shakeOffset = Vector3.Lerp(
                shakeOffset,
                Vector3.zero,
                10f * Time.deltaTime
            );
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }

    #endregion

    #region 死亡效果

    private void OnPlayerDeath()
    {
        if (enableDeathEffect && !isDying)
        {
            isDying = true;
            deathTimer = 0f;
        }
    }

    private void UpdateDeathEffect()
    {
        deathTimer += Time.deltaTime;

        // 慢动作效果
        Time.timeScale = Mathf.Lerp(1f, deathSlowMotionFactor, deathTimer * 2f);

        // 相机旋转
        float rotationAmount = Mathf.Lerp(0, deathRotationAmount, deathTimer);
        transform.RotateAround(target.position, Vector3.up, rotationAmount * Time.deltaTime);

        // 相机下降
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, target.position.y + 2f, deathTimer * 0.5f);
        transform.position = pos;

        // 持续2秒后停止
        if (deathTimer > 2f)
        {
            isDying = false;
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 触发相机震动
    /// </summary>
    public void ShakeCamera(float intensity = -1f, float duration = -1f)
    {
        if (!enableCameraShake) return;

        float shakeIntensityActual = intensity > 0 ? intensity : shakeIntensity;
        shakeOffset = UnityEngine.Random.insideUnitSphere * shakeIntensityActual;
    }

    /// <summary>
    /// 设置相机倾斜
    /// </summary>
    public void SetTilt(float tiltAmount)
    {
        if (!enableTilt) return;

        currentTilt = Mathf.Clamp(tiltAmount, -maxTiltAngle, maxTiltAngle);
    }

    /// <summary>
    /// 设置新的目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.OnPlayerDeath += OnPlayerDeath;
            }
        }
    }

    /// <summary>
    /// 重置相机状态
    /// </summary>
    public void ResetCamera()
    {
        isDying = false;
        deathTimer = 0f;
        Time.timeScale = 1f;
        currentFOV = minFOV;
        currentTilt = 0f;
        shakeOffset = Vector3.zero;

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = minFOV;
        }

        if (target != null)
        {
            transform.position = target.position + positionOffset;
            transform.rotation = Quaternion.LookRotation(
                (target.position + lookAtOffset) - transform.position
            );
        }
    }

    #endregion

    #region 辅助方法

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("CameraController: No player found with tag 'Player'");
        }
    }

    #endregion

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中绘制相机视锥和跟随范围
    /// </summary>
    private void OnDrawGizmos()
    {
        if (target == null) return;

        // 绘制目标位置
        Gizmos.color = Color.yellow;
        Vector3 targetPos = target.position + positionOffset;
        Gizmos.DrawWireSphere(targetPos, 0.5f);

        // 绘制观察点
        Gizmos.color = Color.green;
        Vector3 lookAtPos = target.position + lookAtOffset;
        Gizmos.DrawWireSphere(lookAtPos, 0.3f);

        // 绘制视线
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(targetPos, lookAtPos);

        // 绘制当前相机位置
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
#endif
}
