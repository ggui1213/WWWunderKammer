using UnityEngine;
using UnityEngine.InputSystem;
namespace WWWK.Menu {
public class FollowFrontMenu : MonoBehaviour {
    public InputActionReference menuAction;
    public CanvasGroup canvasGroup;
    public float distance = 2.2f;
    public bool smoothFollow = true;
    public float followSpeed = 10f;
    public float fadeSpeed = 12f;
    Transform camT; bool visible;
    void Awake(){ camT = Camera.main?Camera.main.transform:null; if(canvasGroup){canvasGroup.alpha=0; canvasGroup.interactable=canvasGroup.blocksRaycasts=false;}}
    void OnEnable(){ if(menuAction) menuAction.action.performed += _=> Toggle(); }
    void OnDisable(){ if(menuAction) menuAction.action.performed -= _=> Toggle(); }
    public void Toggle(){ SetVisible(!visible,false);} public void Show(){SetVisible(true,false);} public void Hide(){SetVisible(false,false);}
    void SetVisible(bool v,bool instant){ visible=v; if(!canvasGroup) return; if(instant) canvasGroup.alpha=v?1f:0f; canvasGroup.interactable=canvasGroup.blocksRaycasts=v; }
    void LateUpdate(){
        if(camT==null){ if(Camera.main) camT=Camera.main.transform; else return; }
        Vector3 tp = camT.position + camT.forward*distance; tp.y = Mathf.Lerp(transform.position.y, camT.position.y, 0.5f);
        Quaternion tr = Quaternion.LookRotation((tp-camT.position).normalized, Vector3.up);
        if(smoothFollow){ transform.position = Vector3.Lerp(transform.position,tp,Time.deltaTime*followSpeed); transform.rotation = Quaternion.Slerp(transform.rotation,tr,Time.deltaTime*followSpeed); }
        else transform.SetPositionAndRotation(tp,tr);
        if(canvasGroup){ float tgt=visible?1f:0f; canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha,tgt,Time.deltaTime*fadeSpeed); }
    }
}}