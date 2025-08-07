using UnityEngine;
using UnityEngine.InputSystem;

public class OneTimeHandUI : MonoBehaviour
{
    public GameObject handHintsUI;
    public InputActionReference menuAction;  // 在 Inspector 里指向 “Menu” 这个 InputAction

    private bool hasHidden = false;

    void OnEnable()
    {
        menuAction.action.performed += OnMenuPressed;
    }
    void OnDisable()
    {
        menuAction.action.performed -= OnMenuPressed;
    }

    private void OnMenuPressed(InputAction.CallbackContext ctx)
    {
        if (hasHidden) return;
        handHintsUI.SetActive(false);
        hasHidden = true;
    }
}