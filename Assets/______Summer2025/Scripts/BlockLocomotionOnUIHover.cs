using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class BlockLocomotionOnUIHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("XR Origin 上实际在用到的 Provider")]
    public ContinuousMoveProvider moveProvider;
    public ContinuousTurnProvider turnProvider;
    public SnapTurnProvider snapTurnProvider;

    [Header("可选：阻止传送（禁 Provider，不禁 Ray）")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportProvider;
    public bool blockTeleport = true;

    [Header("其他要禁用的组件（不要放 Ray/LineVisual/LineRenderer）")]
    public Behaviour[] extraBehavioursToDisable;

    int _hoverOrDragCount;

    void OnEnable()  => Apply();
    void OnDisable() { _hoverOrDragCount = 0; Apply(); }

    public void OnPointerEnter(PointerEventData e) { _hoverOrDragCount++; Apply(); }
    public void OnPointerExit (PointerEventData e) { if (_hoverOrDragCount > 0) _hoverOrDragCount--; Apply(); }
    public void OnBeginDrag   (PointerEventData e) { _hoverOrDragCount++; Apply(); }
    public void OnEndDrag     (PointerEventData e) { if (_hoverOrDragCount > 0) _hoverOrDragCount--; Apply(); }

    void Apply()
    {
        bool block = _hoverOrDragCount > 0;

        if (moveProvider)      moveProvider.enabled      = !block;
        if (turnProvider)      turnProvider.enabled      = !block;
        if (snapTurnProvider)  snapTurnProvider.enabled  = !block;
        if (teleportProvider)  teleportProvider.enabled  = !(blockTeleport && block);

        if (extraBehavioursToDisable != null)
        {
            foreach (var b in extraBehavioursToDisable)
            {
                if (!b) continue;
                // ❗️防呆：永远不禁这些
                if (b is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor) continue;
                if (b is XRBaseController) continue;
                if (b is UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual) continue;
                if (b.GetType().IsAssignableFrom(typeof(LineRenderer))) continue;
                b.enabled = !block;
            }
        }
    }

    // 若你不是通过 SetActive(false) 关闭菜单，可在关菜单时手动复位一次
    public void ForceRelease()
    {
        _hoverOrDragCount = 0;
        Apply();
    }
}
