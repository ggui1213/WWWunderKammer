using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[DefaultExecutionOrder(10000)] // 尽量最后执行
public class LineRendererWatchdog : MonoBehaviour
{
    public XRInteractorLineVisual lineVisual;
    LineRenderer lr;

    void Awake()
    {
        if (!lineVisual) lineVisual = GetComponent<XRInteractorLineVisual>();
        if (lineVisual) lr = lineVisual.GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        if (!lineVisual || !lr) return;

        // Far 开着但线被关，说明外部逻辑在关
        var ray = lineVisual.GetComponent<XRRayInteractor>();
        bool farEnabled = !ray || (ray && ray.enabled);
        if (farEnabled && lineVisual.enabled && !lr.enabled)
        {
            lr.enabled = true;
#if UNITY_2021_3_OR_NEWER
            lr.forceRenderingOff = false;
#endif
            Debug.LogWarning("[LineRendererWatchdog] LineRenderer was disabled externally during play. Re-enabled this frame.");
        }
    }
}