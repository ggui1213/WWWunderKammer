using UnityEngine;

[ExecuteAlways]
public class MaintainWorldScale : MonoBehaviour
{
    [Header("期望的世界空间大小")]
    public Vector3 targetWorldScale = Vector3.one;

    private void LateUpdate()
    {
        // 如果没有父节点，就直接设置 localScale 为 target
        if (transform.parent == null)
        {
            transform.localScale = targetWorldScale;
            return;
        }

        // 取父节点的 LossyScale（世界缩放）
        Vector3 parentScale = transform.parent.lossyScale;

        // 计算补偿后的 localScale
        transform.localScale = new Vector3(
            targetWorldScale.x / parentScale.x,
            targetWorldScale.y / parentScale.y,
            targetWorldScale.z / parentScale.z
        );
    }
}