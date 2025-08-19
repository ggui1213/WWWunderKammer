using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// 短按：切换“前方菜单”
/// 长按：按住时显示“腕表菜单”，松开隐藏
/// - 兼容新输入系统（推荐）与旧输入系统 KeyCode
/// - 打开前方菜单时自动把 ScrollRect 滚到顶部（可关）
/// - 可选：长按腕表时临时隐藏前方菜单（不改它的开关状态）
public class WWWK_MenuShortLongToggle : MonoBehaviour
{
    [Header("Menu Roots")]
    public GameObject frontMenuRoot;   // 前方菜单（含 Scroll View）
    public GameObject wristMenuRoot;   // 腕表菜单（只在按住时显示）

    [Header("Input")]
    public float longPressThreshold = 0.35f;  // 判定长按的时间（秒）
    public KeyCode legacyKey = KeyCode.Tab;   // 没用新输入系统时的兜底按键
#if ENABLE_INPUT_SYSTEM
    public InputActionReference openClose;    // 绑定到你的“菜单/返回”按钮 Action（如 Menu/X/A）
#endif

    [Header("Behavior")]
    public bool hideFrontWhileWristActive = false; // 长按腕表时是否临时隐藏前方菜单
    public bool resetScrollTopOnFrontOpen = true;  // 打开前方菜单时把 ScrollRect 顶到最上
    public bool applyGridSizerOnOpen = true;       // 若 Content 上有 AutoGridContentSizer，则调用一次 Apply()
    public bool setTimeScaleOnFrontOpen = false;   // 打开前方菜单时是否暂停
    public float pausedTimeScale = 0f;

    bool isFrontOpen;
    bool pressed;
    bool wristShownThisHold;
    float pressT;

    void Awake()
    {
        if (frontMenuRoot) frontMenuRoot.SetActive(false);
        if (wristMenuRoot) wristMenuRoot.SetActive(false);
#if ENABLE_INPUT_SYSTEM
        if (openClose)
        {
            openClose.action.started  += OnStarted;
            openClose.action.canceled += OnCanceled;
            openClose.action.Enable();
        }
#endif
    }

#if ENABLE_INPUT_SYSTEM
    void OnStarted(InputAction.CallbackContext ctx) { BeginPress(); }
    void OnCanceled(InputAction.CallbackContext ctx) { EndPress(); }
#endif

    void Update()
    {
#if !ENABLE_INPUT_SYSTEM
        // 旧输入系统兜底
        if (Input.GetKeyDown(legacyKey)) BeginPress();
        if (Input.GetKeyUp(legacyKey))   EndPress();
#else
        // 没绑定 Action 时允许用旧按键兜底
        if (!openClose)
        {
            if (Input.GetKeyDown(legacyKey)) BeginPress();
            if (Input.GetKeyUp(legacyKey))   EndPress();
        }
#endif
        // 到达阈值 → 进入长按腕表
        if (pressed && !wristShownThisHold && (Time.unscaledTime - pressT) >= longPressThreshold)
        {
            ShowWrist(true);
            wristShownThisHold = true;
            if (hideFrontWhileWristActive) SetFrontOpen(false);
        }
    }

    void BeginPress()
    {
        pressed = true;
        wristShownThisHold = false;
        pressT = Time.unscaledTime;
    }

    void EndPress()
    {
        if (!pressed) return;
        pressed = false;

        if (wristShownThisHold)
        {
            // 长按结束：收起腕表，不触发短按切换
            ShowWrist(false);
            return;
        }

        // 短按：切换前方菜单
        SetFrontOpen(!isFrontOpen);
    }

    void ShowWrist(bool on)
    {
        if (wristMenuRoot) wristMenuRoot.SetActive(on);
    }

    void SetFrontOpen(bool open)
    {
        if (isFrontOpen == open) return;
        isFrontOpen = open;

        if (frontMenuRoot) frontMenuRoot.SetActive(isFrontOpen);
        if (setTimeScaleOnFrontOpen) Time.timeScale = isFrontOpen ? pausedTimeScale : 1f;

        if (isFrontOpen)
        {
            if (resetScrollTopOnFrontOpen) ResetFrontScrollToTop();
            if (applyGridSizerOnOpen) TryApplyGridSizer();
        }
    }

    void ResetFrontScrollToTop()
    {
        if (!frontMenuRoot) return;
        var sr = frontMenuRoot.GetComponentInChildren<ScrollRect>(true);
        if (!sr) return;
        Canvas.ForceUpdateCanvases();
        if (sr.content) LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);
        sr.verticalNormalizedPosition = 1f; // 顶部
    }

    void TryApplyGridSizer()
    {
        if (!frontMenuRoot) return;
        var sr = frontMenuRoot.GetComponentInChildren<ScrollRect>(true);
        if (!sr || !sr.content) return;
        var sizer = sr.content.GetComponent(typeof(AutoGridContentSizer));
        if (sizer != null)
        {
            var m = sizer.GetType().GetMethod("Apply", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m != null) m.Invoke(sizer, null);
        }
    }

    void OnDestroy()
    {
#if ENABLE_INPUT_SYSTEM
        if (openClose)
        {
            openClose.action.started  -= OnStarted;
            openClose.action.canceled -= OnCanceled;
        }
#endif
    }
}
