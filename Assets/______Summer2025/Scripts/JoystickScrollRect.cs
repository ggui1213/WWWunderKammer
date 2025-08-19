using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class JoystickScrollRect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("拖入 ScrollRect")]
    public ScrollRect scrollRect;

    [Header("摇杆输入（Vector2），如：XRI RightHand/Primary2DAxis 或 Move")]
    public InputActionReference scrollAction;

    [Header("滚动速度：越大越快")]
    public float verticalSpeed = 1.5f;
    public float horizontalSpeed = 1.5f;

    [Header("死区（防抖）")]
    [Range(0f, 1f)] public float deadzone = 0.15f;

    [Header("只在指针悬停时响应该滚动")]
    public bool onlyWhenHovered = true;

    [Header("反转 Y 方向（需要时勾上）")]
    public bool invertY = true;

    bool _hover;

    void OnEnable()
    {
        if (scrollAction != null) scrollAction.action.Enable();
    }

    void OnDisable()
    {
        if (scrollAction != null) scrollAction.action.Disable();
    }

    public void OnPointerEnter(PointerEventData eventData) => _hover = true;
    public void OnPointerExit(PointerEventData eventData)  => _hover = false;

    void Update()
    {
        if (scrollRect == null || scrollAction == null) return;
        if (onlyWhenHovered && !_hover) return;

        Vector2 stick = scrollAction.action.ReadValue<Vector2>();
        if (stick.magnitude < deadzone) return;

        // 纵向
        if (scrollRect.vertical)
        {
            float y = invertY ? -stick.y : stick.y;
            float v = scrollRect.verticalNormalizedPosition + y * verticalSpeed * Time.unscaledDeltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(v);
        }

        // 横向（如果是水平 ScrollView 就开）
        if (scrollRect.horizontal)
        {
            float h = stick.x;
            float hv = scrollRect.horizontalNormalizedPosition - h * horizontalSpeed * Time.unscaledDeltaTime;
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(hv);
        }
    }
}