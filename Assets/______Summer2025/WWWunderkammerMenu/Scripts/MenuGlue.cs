using UnityEngine;

namespace WWWunderkammer.Menu {
    public class MenuGlue : MonoBehaviour {
        public MenuInputRouter router;
        public GameObject followMenuRoot;
        public WristMenuAnchor wrist;

        void Start(){
            if (router==null) router = GetComponent<MenuInputRouter>();
            if (router==null) return;
            router.OnShortPress += ()=>{ if (followMenuRoot) followMenuRoot.SetActive(!followMenuRoot.activeSelf); if (wrist) wrist.Show(false); };
            router.OnLongPress  += ()=>{ if (wrist) wrist.Show(true); if (followMenuRoot) followMenuRoot.SetActive(false); };
        }
    }
}
