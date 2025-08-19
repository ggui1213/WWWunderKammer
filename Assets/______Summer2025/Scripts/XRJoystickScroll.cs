using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// 用控制器摇杆/手柄左摇杆垂直滚动 ScrollRect（不依赖拖拽）
/// - 建议把 ScrollRect 的 Inertia 关掉或减小，交给本脚本平滑
/// - 支持“需要指针悬停时才滚”的可选开关
public class XRJoystickScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ScrollRect scroll;
#if ENABLE_INPUT_SYSTEM
    [Header("Input System (Vector2)")]
    public InputActionReference axis;  // 绑定 <XRController>{RightHand}/primary2DAxis 等
#endif
    [Header("Tuning")]
    [Tooltip("每秒改变的归一化滚动量（0~1）；1 代表 1 秒滚完整个列表")]
    public float speed = 1.2f;
    [Tooltip("小于该阈值的输入视为 0，去抖动")]
    public float deadZone = 0.15f;
    [Tooltip("是否把摇杆上推视为向下滚动（更符合直觉）")]
    public bool invert = true;
    [Tooltip("平滑时间（秒），0=不平滑")]
    public float smoothTime = 0.08f;
    [Tooltip("仅当指针悬停在此 ScrollView/Viewport 上时才允许摇杆滚动")]
    public bool requireHover = false;

    float _vel;      // SmoothDamp 用的速度
    bool _hovering;  // 悬停态（可选）

    void Reset()
    {
        if (!scroll) scroll = GetComponent<ScrollRect>();
    }

    void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        axis?.action?.Enable();
#endif
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        axis?.action?.Disable();
#endif
    }

    void Update()
    {
        if (!scroll) return;

        float y = 0f;

#if ENABLE_INPUT_SYSTEM
        if (axis && axis.action != null)
        {
            Vector2 v = axis.action.ReadValue<Vector2>();
            y = v.y;
        }
#else
        y = Input.GetAxisRaw("Vertical"); // 旧输入系统兜底
#endif
        if (Mathf.Abs(y) < deadZone) y = 0f;
        if (invert) y = -y;
        if (requireHover && !_hovering) return;

        // 目标滚动位置（0=底部，1=顶部）
        float target = Mathf.Clamp01(scroll.verticalNormalizedPosition + y * speed * Time.unscaledDeltaTime);

        float newPos = smoothTime > 0f
            ? Mathf.SmoothDamp(scroll.verticalNormalizedPosition, target, ref _vel, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime)
            : target;

        scroll.verticalNormalizedPosition = newPos;
    }

    // 只有当本脚本挂在 Viewport/Content 或有可接收事件的 Graphic 上时，悬停才会触发
    public void OnPointerEnter(PointerEventData e) { _hovering = true; }
    public void OnPointerExit(PointerEventData e)  { _hovering = false; }
}
