using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class MenuModeSwitcher : MonoBehaviour
{
    [Header("右手近距交互器（Direct/Near 那个）")]
    public XRBaseInteractor rightNear;   // XRDirectInteractor 或 Near 子组件
    [Header("右手远距射线交互器（Ray 那个）")]
    public XRRayInteractor rightRay;
    [Header("线段可视化（不是必须，作为保险）")]
    public XRInteractorLineVisual rightLineVisual;

    public void OnMenuOpen()
    {
        if (rightNear) rightNear.enabled = false; // 关键：禁用 Near，避免压制 Far
        if (rightRay)  rightRay.enabled  = true;

        // 保险：如果之前被关过，这里拉起来一次
        if (rightLineVisual && rightLineVisual.TryGetComponent<LineRenderer>(out var lr))
        {
            rightLineVisual.enabled = true;
            lr.enabled = true;
#if UNITY_2021_3_OR_NEWER
            lr.forceRenderingOff = false;
#endif
        }
    }

    public void OnMenuClose()
    {
        if (rightNear) rightNear.enabled = true;  // 恢复 Near
    }
}