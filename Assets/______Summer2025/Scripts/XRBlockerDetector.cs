using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class XRBlockerDetector : MonoBehaviour
{
    public LayerMask checkLayers = ~0;
    public bool logEveryFrame = false;
    CharacterController cc;

    void Awake() => cc = GetComponent<CharacterController>();

    [ContextMenu("Log overlaps now")]
    public void LogOverlaps()
    {
        if (!cc) return;
        // 计算 CharacterController 胶囊两端点（世界坐标）
        var center = transform.TransformPoint(cc.center);
        float half = Mathf.Max(0f, (cc.height * 0.5f) - cc.radius);
        Vector3 p0 = center + Vector3.up * half;
        Vector3 p1 = center - Vector3.up * half;

        var hits = Physics.OverlapCapsule(p0, p1, cc.radius + cc.skinWidth,
            checkLayers, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) Debug.Log("[XRBlocker] No overlaps.");
        foreach (var h in hits)
            Debug.Log($"[XRBlocker] {h.name} (layer:{LayerMask.LayerToName(h.gameObject.layer)})",
                h.gameObject);
    }

    void Update(){ if (logEveryFrame) LogOverlaps(); }
}