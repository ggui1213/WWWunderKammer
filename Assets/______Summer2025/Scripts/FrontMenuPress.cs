using UnityEngine;
using UnityEngine.InputSystem;

public class FollowFrontMenu : MonoBehaviour
{
    [Header("绑定到 <XRController>{LeftHand}/menuButton")]
    public InputActionReference menuAction;

    [Header("世界空间 Canvas 根节点")]
    public GameObject menuUI;

    [Header("菜单距离玩家眼睛的距离 (米)")]
    public float distance = 2f;

    [Header("是否开启平滑移动")]
    public bool smoothFollow = true;

    [Header("平滑移动速度 (单位：米/秒)")]
    public float followSpeed = 10f;

    private Transform camT;

    private void Awake()
    {
        camT = Camera.main.transform;
        if (menuUI) menuUI.SetActive(false);
        
        int uiLayer = LayerMask.NameToLayer("UI");
        // 忽略 UI 层 和 0-31 号层之间的所有物理碰撞
        for (int i = 0; i < 32; i++)
            if (i != uiLayer)
                Physics.IgnoreLayerCollision(uiLayer, i, true);
    }

    private void OnEnable()
    {
        if (menuAction?.action != null)
        {
            menuAction.action.Enable();
            menuAction.action.performed += OnMenuPressed;
        }
    }

    private void OnDisable()
    {
        if (menuAction?.action != null)
            menuAction.action.performed -= OnMenuPressed;
    }

    private void Update()
    {
        // 如果菜单打开，则每帧更新位置和朝向
        if (menuUI != null && menuUI.activeSelf)
        {
            UpdateMenuTransform();
        }
    }

    private void OnMenuPressed(InputAction.CallbackContext ctx)
    {
        // 切换菜单显示
        bool willOpen = !menuUI.activeSelf;
        if (willOpen)
        {
            // 初次打开时直接对齐一次
            UpdateMenuTransform(immediate: true);
        }
        menuUI.SetActive(willOpen);
    }

    /// <summary>
    /// 更新菜单的位置和朝向
    /// </summary>
    /// <param name="immediate">是否立即跳到目标位置，跳过平滑</param>
    private void UpdateMenuTransform(bool immediate = false)
    {
        // 计算目标位置：相机前方 distance 米，同高
        Vector3 targetPos = camT.position + camT.forward * distance;
        targetPos.y = camT.position.y;

        // 计算目标朝向：水平朝向玩家
        Vector3 targetDirection = camT.forward;
        targetDirection.y = 0; // 保持水平
        targetDirection.Normalize();
        Quaternion targetRot = Quaternion.LookRotation(targetDirection);
        
        //Vector3 lookAt = new Vector3(camT.position.x, targetPos.y, camT.position.z);
        //Quaternion targetRot = Quaternion.LookRotation(lookAt - targetPos);

        if (smoothFollow && !immediate)
        {
            // 平滑插值位置和旋转
            menuUI.transform.position = Vector3.Lerp(
                menuUI.transform.position,
                targetPos,
                Mathf.Clamp01(followSpeed * Time.unscaledDeltaTime)
            );
            menuUI.transform.rotation = Quaternion.Slerp(
                menuUI.transform.rotation,
                targetRot,
                Mathf.Clamp01(followSpeed * Time.unscaledDeltaTime)
            );
        }
        else
        {
            // 立即更新
            menuUI.transform.position = targetPos;
            menuUI.transform.rotation = targetRot;
        }
    }
}
