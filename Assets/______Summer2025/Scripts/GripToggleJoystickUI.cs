using UnityEngine;
using UnityEngine.InputSystem;

public class GripToggleJoystickUI_Action : MonoBehaviour
{
    public GameObject joystickUI;
    public InputActionReference gripAction;  // 绑定到 XR Controller 的 "Grip" 动作
    private bool prevState = false;

    void OnEnable()
    {
        gripAction.action.performed += OnGripChanged;
        gripAction.action.canceled += OnGripChanged;
    }
    void OnDisable()
    {
        gripAction.action.performed -= OnGripChanged;
        gripAction.action.canceled -= OnGripChanged;
    }

    private void OnGripChanged(InputAction.CallbackContext ctx)
    {
        bool isPressed = ctx.ReadValue<float>() > 0.5f;
        if (isPressed != prevState)
        {
            joystickUI.SetActive(isPressed);
            prevState = isPressed;
        }
    }

    void Start()
    {
        if (joystickUI != null) joystickUI.SetActive(false);
    }
}