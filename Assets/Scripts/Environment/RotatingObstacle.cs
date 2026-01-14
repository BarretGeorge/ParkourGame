using UnityEngine;

/// <summary>
/// 旋转障碍物 - 旋转的障碍物（如旋转的柱子、刀片等）
/// </summary>
[RequireComponent(typeof(Obstacle))]
public class RotatingObstacle : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("旋转轴")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Tooltip("旋转速度（度/秒）")]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("旋转方式")]
    [SerializeField] private RotationType rotationType = RotationType.Continuous;

    [Header("摆动设置")]
    [Tooltip("摆动角度范围")]
    [SerializeField] private Vector2 swingAngleRange = new Vector2(-45f, 45f);

    [Tooltip("摆动速度")]
    [SerializeField] private float swingSpeed = 2f;

    // 状态
    private float currentRotation = 0f;
    private Quaternion initialRotation;

    private void Awake()
    {
        initialRotation = transform.localRotation;
    }

    protected void OnUpdate()
    {
        switch (rotationType)
        {
            case RotationType.Continuous:
                RotateContinuously();
                break;

            case RotationType.Swing:
                RotateSwing();
                break;
        }
    }

    /// <summary>
    /// 持续旋转
    /// </summary>
    private void RotateContinuously()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }

    /// <summary>
    /// 摆动旋转
    /// </summary>
    private void RotateSwing()
    {
        // 使用正弦波计算摆动角度
        currentRotation = Mathf.Lerp(swingAngleRange.x, swingAngleRange.y,
            (Mathf.Sin(Time.time * swingSpeed) + 1f) * 0.5f);

        transform.localRotation = initialRotation * Quaternion.AngleAxis(currentRotation, rotationAxis);
    }

    /// <summary>
    /// 获取当前旋转速度
    /// </summary>
    public float GetCurrentRotationSpeed()
    {
        if (rotationType == RotationType.Continuous)
        {
            return rotationSpeed;
        }
        else // Swing
        {
            return Mathf.Cos(Time.time * swingSpeed) * swingSpeed * (swingAngleRange.y - swingAngleRange.x) * 0.5f;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制旋转轴和范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制旋转轴
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, rotationAxis * 2f);

        // 绘制摆动范围
        if (rotationType == RotationType.Swing)
        {
            Gizmos.color = Color.cyan;
            Vector3 basePos = Application.isPlaying ? transform.position : transform.position;
            Quaternion baseRot = Application.isPlaying ? initialRotation : transform.localRotation;

            Quaternion minRot = baseRot * Quaternion.AngleAxis(swingAngleRange.x, rotationAxis);
            Quaternion maxRot = baseRot * Quaternion.AngleAxis(swingAngleRange.y, rotationAxis);

            Gizmos.DrawRay(basePos, minRot * Vector3.forward * 2f);
            Gizmos.DrawRay(basePos, maxRot * Vector3.forward * 2f);
        }
    }
#endif
}

/// <summary>
/// 旋转类型
/// </summary>
public enum RotationType
{
    Continuous, // 持续旋转
    Swing       // 摆动
}
