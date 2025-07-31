using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportMeshGlobalManager : MonoBehaviour
{
    [Tooltip("指向 XRI Default Input Actions 里 Grip 按键对应的 Action")]
    public InputActionReference grabAction;

    TeleportPointVisual[] visuals;

    void Awake()
    {
        visuals = FindObjectsOfType<TeleportPointVisual>();
        // 先全部隐藏
        foreach (var v in visuals)
            if (v.meshObject) v.meshObject.SetActive(false);
    }

    void OnEnable()
    {
        grabAction.action.started += _ => ShowAll();
        grabAction.action.canceled += _ => HideAll();
    }

    void OnDisable()
    {
        grabAction.action.started -= _ => ShowAll();
        grabAction.action.canceled -= _ => HideAll();
    }

    void ShowAll()
    {
        foreach (var v in visuals)
            if (v.meshObject) v.meshObject.SetActive(true);
    }

    void HideAll()
    {
        foreach (var v in visuals)
            if (v.meshObject) v.meshObject.SetActive(false);
    }
}
