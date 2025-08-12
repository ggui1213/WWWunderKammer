using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HorizonStyleMenu : MonoBehaviour
{
    [Header("输入绑定")]
    public InputActionReference menuAction;            // 绑定到 <XRController>{LeftHand}/menuButton
    [Header("菜单 UI 对象")]
    public GameObject menuUI;                         // 世界空间 Canvas 根节点
    public float distance = 2.0f;                     // 菜单距离玩家眼睛的固定距离
    public float fadeDuration = 0.25f;                // 淡入/淡出时长（秒）

    private CanvasGroup _canvasGroup;
    private bool _isOpen = false;
    private Transform _cameraT;

    void Awake()
    {
        // 获取主相机变换
        _cameraT = Camera.main.transform;
        if (menuUI != null)
        {
            // 确保 CanvasGroup 存在，用于淡入淡出
            _canvasGroup = menuUI.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = menuUI.AddComponent<CanvasGroup>();

            menuUI.SetActive(false);
            _canvasGroup.alpha = 0f;
        }
    }

    void OnEnable()
    {
        if (menuAction != null && menuAction.action != null)
        {
            menuAction.action.Enable();
            menuAction.action.performed += OnMenuPressed;
        }
    }

    void OnDisable()
    {
        if (menuAction != null && menuAction.action != null)
            menuAction.action.performed -= OnMenuPressed;
    }

    private void OnMenuPressed(InputAction.CallbackContext ctx)
    {
        ToggleMenu();
    }

    /// <summary>
    /// 打开或关闭菜单
    /// </summary>
    void ToggleMenu()
    {
        if (_isOpen)
        {
            StartCoroutine(FadeAndClose());
        }
        else
        {
            // 首次打开时，先布置位置和朝向
            PositionMenuInFront();
            menuUI.SetActive(true);
            StartCoroutine(FadeIn());
        }
        _isOpen = !_isOpen;
    }

    /// <summary>
    /// 将 menuUI 放置在玩家眼前固定距离，并水平朝向玩家
    /// </summary>
    void PositionMenuInFront()
    {
        Vector3 forward = _cameraT.forward;
        Vector3 targetPos = _cameraT.position + forward * distance;
        // 保持 메뉴 与相机同高，避免俯仰角影响
        targetPos.y = _cameraT.position.y;
        menuUI.transform.position = targetPos;

        // 水平朝向玩家
        Vector3 lookAtPos = new Vector3(_cameraT.position.x, targetPos.y, _cameraT.position.z);
        menuUI.transform.LookAt(lookAtPos);
        menuUI.transform.Rotate(0, 180f, 0); // Canvas 默认朝向背面，所以要翻转
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    IEnumerator FadeAndClose()
    {
        float elapsed = 0f;
        float startAlpha = _canvasGroup.alpha;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        menuUI.SetActive(false);
    }
}
