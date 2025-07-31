using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.BaseTeleportationInteractable))]
public class TeleportPointVisual : MonoBehaviour
{
    [Tooltip("瞬移点的可视对象（圆环、箭头等）")]
    public GameObject meshObject;
    [Tooltip("指向 XRI Default Input Actions 里 Grip 按键的 ActionReference")]
    public InputActionReference gripAction;

    UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.BaseTeleportationInteractable anchor;
    bool isGripping;

    void Awake()
    {
        anchor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.BaseTeleportationInteractable>();

        // 初始隐藏
        if (meshObject) meshObject.SetActive(false);

        // 订阅 Grip 按键事件
        gripAction.action.started += ctx => isGripping = true;
        gripAction.action.canceled += ctx => isGripping = false;

        // 订阅 Hover/Select 事件
        anchor.hoverEntered.AddListener(OnHoverEntered);
        anchor.hoverExited.AddListener(OnHoverExited);
        anchor.selectEntered.AddListener(OnSelectEntered);
        anchor.selectExited.AddListener(OnSelectExited);
    }

    void OnDestroy()
    {
        gripAction.action.started   -= ctx => isGripping = true;
        gripAction.action.canceled  -= ctx => isGripping = false;
        anchor.hoverEntered.RemoveListener(OnHoverEntered);
        anchor.hoverExited .RemoveListener(OnHoverExited);
        anchor.selectEntered.RemoveListener(OnSelectEntered);
        anchor.selectExited .RemoveListener(OnSelectExited);
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (meshObject) meshObject.SetActive(true);
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        // 只有在“未按住 Grip 且未选中”时才隐藏
        if (!isGripping && !anchor.isSelected && meshObject)
            meshObject.SetActive(false);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (meshObject) meshObject.SetActive(true);
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        // 只有在“未按住 Grip”时才隐藏
        if (!isGripping && meshObject)
            meshObject.SetActive(false);
    }
}
