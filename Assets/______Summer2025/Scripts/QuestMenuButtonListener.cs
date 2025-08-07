using UnityEngine;
using UnityEngine.InputSystem;

public class QuestMenuButtonListener : MonoBehaviour
{
    [Header("绑定 OpenMenu 动作（InputActionReference）")]
    public InputActionReference menuAction;

    private void OnEnable()
    {
        if (menuAction != null && menuAction.action != null)
        {
            menuAction.action.Enable();
            menuAction.action.performed += OnMenuPressed;
        }
    }

    private void OnDisable()
    {
        if (menuAction != null && menuAction.action != null)
            menuAction.action.performed -= OnMenuPressed;
    }

    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        // Menu 键 被按下
        Debug.Log("🎮 Menu 按键触发！");

        // TODO: 在这里调用你的弹出/关闭菜单 UI 逻辑
        // e.g. MenuManager.Instance.ToggleMenu();
    }
}