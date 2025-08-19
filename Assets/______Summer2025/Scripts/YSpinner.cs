using UnityEngine;

public class YSpinner : MonoBehaviour
{
    [Tooltip("每秒旋转多少度")]
    public float degreesPerSecond = 90f;

    [Tooltip("是否沿世界坐标的Y轴旋转（否则为自身局部Y轴）")]
    public bool useWorldY = false;

    void Update()
    {
        float angle = degreesPerSecond * Time.deltaTime;

        if (useWorldY)
            transform.Rotate(Vector3.up, angle, Space.World); // 世界Y
        else
            transform.Rotate(0f, angle, 0f, Space.Self);      // 自身局部Y
    }
}