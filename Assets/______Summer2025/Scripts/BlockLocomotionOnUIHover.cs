using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

// 把它挂到 UI 面板（有 Raycast Target 的 Image/TMP）上
public class BlockLocomotionOnUIHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("把 XR Origin 上的 Provider 拖进来（按你项目实际用到的填）")]
    public ContinuousMoveProvider moveProvider;
    public ContinuousTurnProvider turnProvider;
    public SnapTurnProvider snapTurnProvider;      // 用了才填
    public XRRayInteractor teleportRay;                       // 有独立传送射线才填
    public Behaviour[] extraBehavioursToDisable;              // 其他需要一并禁用的组件

    int _hoverOrDragCount = 0;

    void OnEnable() => Apply();
    void OnDisable() { _hoverOrDragCount = 0; Apply(); }

    public void OnPointerEnter(PointerEventData e) { _hoverOrDragCount++; Apply(); }
    public void OnPointerExit(PointerEventData  e) { _hoverOrDragCount = Mathf.Max(0, _hoverOrDragCount - 1); Apply(); }
    public void OnBeginDrag(PointerEventData e)   { _hoverOrDragCount++; Apply(); }
    public void OnEndDrag(PointerEventData e)     { _hoverOrDragCount = Mathf.Max(0, _hoverOrDragCount - 1); Apply(); }

    void Apply()
    {
        bool block = _hoverOrDragCount > 0;

        if (moveProvider)    moveProvider.enabled    = !block;
        if (turnProvider)    turnProvider.enabled    = !block;
        if (snapTurnProvider)snapTurnProvider.enabled= !block;
        if (teleportRay)     teleportRay.enabled     = !block;

        if (extraBehavioursToDisable != null)
            foreach (var b in extraBehavioursToDisable)
                if (b) b.enabled = !block;
    }
}