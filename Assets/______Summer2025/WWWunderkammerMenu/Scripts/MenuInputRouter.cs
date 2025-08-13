using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace WWWunderkammer.Menu {
    public class MenuInputRouter : MonoBehaviour {
        public InputActionReference menuAction;
        public float longPressTime = 0.6f;
        public float doubleTapGap = 0.28f;

        public event Action OnShortPress, OnLongPress, OnDoubleTap;
        float pressDownTime, lastUpTime; bool isLong;

        void OnEnable(){
            if (menuAction==null) return;
            var a = menuAction.action;
            a.started += _ => { pressDownTime = Time.time; isLong = FalseSafeReset(); };
            a.performed += _ => { if (!isLong && Time.time-pressDownTime>=longPressTime){ isLong=true; OnLongPress?.Invoke(); } };
            a.canceled += _ => {
                float now = Time.time;
                if (!isLong){
                    if (now - lastUpTime <= doubleTapGap) OnDoubleTap?.Invoke();
                    else OnShortPress?.Invoke();
                }
                lastUpTime = now;
            };
        }
        bool FalseSafeReset(){ return false; }
        void OnDisable(){
            if (menuAction==null) return;
            var a = menuAction.action;
            a.started -= _ => {};
            a.performed -= _ => {};
            a.canceled -= _ => {};
        }
    }
}
