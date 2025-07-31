using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(BaseTeleportationInteractable))]
public class PressTeleportVisual : MonoBehaviour
{
    [Tooltip("要在按下时显示的网格对象")]
    public GameObject meshObject;

    BaseTeleportationInteractable teleportAnchor;

    void Awake()
    {
        teleportAnchor = GetComponent<BaseTeleportationInteractable>();
        // 设置为按下松开时触发瞬移
        teleportAnchor.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnDeactivated;
        // 初始隐藏
        if (meshObject) meshObject.SetActive(false);
        // 订阅按下（Activate）与松开（Deactivate）事件
        teleportAnchor.activated.AddListener(OnActivated);
        teleportAnchor.deactivated.AddListener(OnDeactivated);
    }

    void OnDestroy()
    {
        teleportAnchor.activated.RemoveListener(OnActivated);
        teleportAnchor.deactivated.RemoveListener(OnDeactivated);
    }

    // 用户按下瞬移按钮
    void OnActivated(ActivateEventArgs args)
    {
        if (meshObject) meshObject.SetActive(true);
    }

    // 用户松开瞬移按钮（同时真正执行瞬移）
    void OnDeactivated(DeactivateEventArgs args)
    {
        if (meshObject) meshObject.SetActive(false);
    }
}