using UnityEngine;
namespace WWWK.Menu {
public class WWWKMenuBootstrap:MonoBehaviour{
    public MenuInputRouter input; public FollowFrontMenu followMenu; public WristMenuAnchor wristMenu;
    void Start(){ if(input!=null){ input.OnShortPress += ()=>{ if(followMenu) followMenu.Toggle(); if(wristMenu) wristMenu.Show(false); };
        input.OnLongPress += ()=>{ if(wristMenu) wristMenu.Show(true); if(followMenu) followMenu.Hide(); };
        input.OnDoubleTap += ()=>{ if(followMenu) followMenu.Show(); }; } }
}}