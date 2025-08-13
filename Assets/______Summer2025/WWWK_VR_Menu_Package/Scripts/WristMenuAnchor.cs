using UnityEngine;
namespace WWWK.Menu {
public class WristMenuAnchor:MonoBehaviour{
    public Transform rightHand; public Vector3 localOffset=new Vector3(0.06f,0f,0.08f); public Vector3 localEuler=new Vector3(20f,0f,0f);
    public CanvasGroup group; public float followLerp=18f, fade=16f; bool showing;
    public void Show(bool v){ showing=v; if(group){ group.interactable=group.blocksRaycasts=v; } }
    public void Toggle(){ Show(!showing); }
    void LateUpdate(){ if(!rightHand||!group) return; var tp=rightHand.TransformPoint(localOffset); var tr= rightHand.rotation*Quaternion.Euler(localEuler);
        transform.position=Vector3.Lerp(transform.position,tp,Time.deltaTime*followLerp);
        transform.rotation=Quaternion.Slerp(transform.rotation,tr,Time.deltaTime*followLerp);
        group.alpha = Mathf.MoveTowards(group.alpha, showing?1f:0f, Time.deltaTime*fade); }
}}