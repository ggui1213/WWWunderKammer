using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace ______Summer2025.Scripts
{
    /// <summary>
    /// 控制瞬移锚点的可视化，在瞄准/选择时显示，松开时隐藏。
    /// </summary>
    [RequireComponent(typeof(BaseTeleportationInteractable))]
    public class TeleportPointVisual : MonoBehaviour
    {
        [Tooltip("要显示的网格（如圆环或箭头）")]
        public GameObject meshObject;

        BaseTeleportationInteractable anchor;

        void Awake()
        {
            anchor = GetComponent<BaseTeleportationInteractable>();
            // 初始隐藏网格
            if (meshObject != null)
                meshObject.SetActive(false);

            // 订阅悬停和选择事件
            anchor.hoverEntered.AddListener(OnHoverEntered);
            anchor.hoverExited.AddListener(OnHoverExited);
            anchor.selectEntered.AddListener(OnSelectEntered);
            anchor.selectExited.AddListener(OnSelectExited);
        }

        void OnDestroy()
        {
            // 移除事件监听
            anchor.hoverEntered.RemoveListener(OnHoverEntered);
            anchor.hoverExited.RemoveListener(OnHoverExited);
            anchor.selectEntered.RemoveListener(OnSelectEntered);
            anchor.selectExited.RemoveListener(OnSelectExited);
        }

        // 射线开始悬停：显示可视化
        void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (meshObject != null)
                meshObject.SetActive(true);
        }

        // 射线离开悬停：隐藏可视化（仅在未被选择时）
        void OnHoverExited(HoverExitEventArgs args)
        {
            // 如果已经选择（玩家仍按着传送按钮），不隐藏
            if (!anchor.isSelected && meshObject != null)
                meshObject.SetActive(false);
        }

        // 玩家按下传送按钮开始选择：确保可视化显示
        void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (meshObject != null)
                meshObject.SetActive(true);
        }

        // 玩家松开传送按钮，选择结束：隐藏可视化
        void OnSelectExited(SelectExitEventArgs args)
        {
            if (meshObject != null)
                meshObject.SetActive(false);
        }
    }
}