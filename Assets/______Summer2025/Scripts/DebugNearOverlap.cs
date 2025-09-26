using UnityEngine;

public class DebugNearOverlap : MonoBehaviour
{
    public float radius = 0.25f;          // 设成和你的 Near Caster 半径差不多
    public LayerMask mask = ~0;           // 暂时全开
    public Transform probe;               // 为空就用当前物体
    void Update()
    {
        var p = probe ? probe.position : transform.position;
        var hits = Physics.OverlapSphere(p, radius, mask, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
            Debug.Log($"[NearHit] {h.name}  layer={LayerMask.LayerToName(h.gameObject.layer)}  path={h.transform.GetHierarchyPath()}");
    }
}

static class TFPathExt {
    public static string GetHierarchyPath(this Transform t){
        string path = t.name;
        while (t.parent){ t = t.parent; path = t.name + "/" + path; }
        return path;
    }
}