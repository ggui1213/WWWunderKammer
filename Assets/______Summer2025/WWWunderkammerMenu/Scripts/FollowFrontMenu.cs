using UnityEngine;
using UnityEngine.InputSystem;

namespace WWWunderkammer.Menu {
    [DefaultExecutionOrder(100)]
    public class FollowFrontMenu : MonoBehaviour {
        [Header("绑定到 <XRController>{LeftHand}/menuButton")]
        public InputActionReference menuAction;
        [Header("菜单根 CanvasGroup")]
        public CanvasGroup canvasGroup;
        [Header("距离玩家眼睛 (米)")]
        public float distance = 2.2f;
        [Header("跟随与动画")]
        public bool smoothFollow = true;
        public float followSpeed = 10f;
        public float fadeSpeed = 12f;

        Transform camT;
        bool visible;

        void Awake() {
            camT = Camera.main ? Camera.main.transform : null;
            if (canvasGroup != null) {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
            }
        }

        void OnEnable() { if (menuAction != null) menuAction.action.performed += OnMenuPerformed; }
        void OnDisable(){ if (menuAction != null) menuAction.action.performed -= OnMenuPerformed; }

        void OnMenuPerformed(InputAction.CallbackContext _) => SetVisible(!visible, false);

        public void SetVisible(bool v, bool instant) {
            visible = v;
            if (!canvasGroup) return;
            if (instant) canvasGroup.alpha = v ? 1f : 0f;
            canvasGroup.interactable = canvasGroup.blocksRaycasts = v;
        }

        void LateUpdate() {
            if (camT == null) { if (Camera.main) camT = Camera.main.transform; else return; }
            Vector3 targetPos = camT.position + camT.forward * distance;
            targetPos.y = Mathf.Lerp(transform.position.y, camT.position.y, 0.5f);
            Quaternion targetRot = Quaternion.LookRotation((targetPos - camT.position).sqrMagnitude < 0.0001f
                ? camT.forward : (targetPos - camT.position).normalized, Vector3.up);
            if (smoothFollow) {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
            } else {
                transform.SetPositionAndRotation(targetPos, targetRot);
            }
            if (canvasGroup) {
                float tgt = visible ? 1f : 0f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, tgt, Time.deltaTime * fadeSpeed);
            }
        }
    }
}
