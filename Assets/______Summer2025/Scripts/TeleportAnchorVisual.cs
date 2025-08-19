using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor))]
public class TeleportAnchorVisual : MonoBehaviour
{
    public Transform anchorPoint;     // AnchorPoint
    public Transform ring;            // Visuals/Ring
    public Transform arrow;           // Visuals/Arrow
    public Transform ripple;          // Visuals/Ripple
    public Renderer ringRend, arrowRend, rippleRend;

    [Header("Colors")]
    public Color idleColor = new Color(0.3f, 0.9f, 1f, 0.55f);
    public Color hoverColor = new Color(0.1f, 1f, 0.6f, 0.9f);
    public Color selectColor = new Color(1f, 1f, 1f, 1f);

    [Header("Pulse")]
    public float idleScale = 1.0f;
    public float hoverScale = 1.10f;
    public float pulseAmp = 0.06f;
    public float pulseSpeed = 2.2f;

    [Header("Facing")]
    public bool arrowFacesCameraYaw = true; // 箭头水平朝相机
    Transform cam;

    UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor anchor;
    bool isHover;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        anchor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>();
        mpb = new MaterialPropertyBlock();
        cam = Camera.main ? Camera.main.transform : null;

        // 稍微抬高视觉以防 Z-fighting
        if (ring)   ring.localPosition   += Vector3.up * 0.01f;
        if (arrow)  arrow.localPosition  += Vector3.up * 0.011f;
        if (ripple) ripple.localPosition += Vector3.up * 0.009f;

        // 事件
        anchor.hoverEntered.AddListener(OnHoverEntered);
        anchor.hoverExited.AddListener(OnHoverExited);
        anchor.selectEntered.AddListener(OnSelectEntered);

        SetColorAll(idleColor);
    }

    void OnDestroy()
    {
        anchor.hoverEntered.RemoveListener(OnHoverEntered);
        anchor.hoverExited.RemoveListener(OnHoverExited);
        anchor.selectEntered.RemoveListener(OnSelectEntered);
    }

    void Update()
    {
        float t = Time.time * pulseSpeed;
        float s = (isHover ? hoverScale : idleScale) * (1f + Mathf.Sin(t) * pulseAmp);

        if (ring)   ring.localScale = new Vector3(s, 1f, s);
        if (ripple) ripple.localScale = new Vector3(s * 1.05f, 1f, s * 1.05f);

        // 箭头水平对齐到相机朝向（或改成 anchorPoint.forward）
        if (arrowFacesCameraYaw && arrow)
        {
            if (!cam && Camera.main) cam = Camera.main.transform;
            if (cam)
            {
                var f = cam.forward; f.y = 0f; if (f.sqrMagnitude < 1e-4f) f = transform.forward;
                arrow.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);
            }
        }
    }

    void OnHoverEntered(HoverEnterEventArgs _)
    {
        isHover = true;
        SetColorAll(hoverColor);
    }

    void OnHoverExited(HoverExitEventArgs _)
    {
        isHover = false;
        SetColorAll(idleColor);
    }

    void OnSelectEntered(SelectEnterEventArgs _)
    {
        // 简单闪一下
        Flash(selectColor, 0.08f);
    }

    void Flash(Color c, float dur)
    {
        StopAllCoroutines();
        StartCoroutine(CoFlash(c, dur));
    }

    System.Collections.IEnumerator CoFlash(Color c, float dur)
    {
        SetColorAll(c);
        yield return new WaitForSeconds(dur);
        SetColorAll(isHover ? hoverColor : idleColor);
    }

    void SetColor(Renderer r, Color c)
    {
        if (!r) return;
        r.GetPropertyBlock(mpb);
        // URP Unlit 的主色是 _BaseColor；内置 Unlit 则是 _Color（两者都写最保险）
        mpb.SetColor("_BaseColor", c);
        mpb.SetColor("_Color", c);
        r.SetPropertyBlock(mpb);
    }
    void SetColorAll(Color c){ SetColor(ringRend, c); SetColor(arrowRend, c); SetColor(rippleRend, c); }
}
