using UnityEngine; using UnityEngine.InputSystem; using System;
namespace WWWK.Menu {
public class MenuInputRouter:MonoBehaviour{
    public InputActionReference menuAction; public float longPressTime=0.6f, doubleTapGap=0.28f;
    public event Action OnShortPress, OnLongPress, OnDoubleTap; float down, lastUp; bool sentLong;
    void OnEnable(){ if(menuAction==null) return; var a=menuAction.action; a.started += _=>{down=Time.time; sentLong=false;};
        a.performed += _=>{ if(!sentLong && Time.time-down>=longPressTime){ sentLong=true; OnLongPress?.Invoke(); } };
        a.canceled += _=>{ float now=Time.time; if(!sentLong){ if(now-lastUp<=doubleTapGap) OnDoubleTap?.Invoke(); else OnShortPress?.Invoke(); } lastUp=now; }; }
}}