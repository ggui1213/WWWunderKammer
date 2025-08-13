using UnityEngine;

namespace WWWunderkammer.Menu {
    [ExecuteAlways]
    public class ArcLayoutGroup : MonoBehaviour {
        public float radius = 0.9f;
        public float arcDegrees = 28f;
        public Vector2 itemSize = new Vector2(420,260);

        void OnEnable(){ Layout(); }
        void OnValidate(){ Layout(); }
        void OnTransformChildrenChanged(){ Layout(); }

        public void Layout(){
            int n = transform.childCount; if (n==0) return;
            float step = (n==1)?0f:arcDegrees/(n-1);
            float start = -arcDegrees*0.5f;
            for(int i=0;i<n;i++){
                var rt = transform.GetChild(i) as RectTransform; if (!rt) continue;
                float ang = (start + step*i) * Mathf.Deg2Rad;
                float x = Mathf.Sin(ang) * radius * 1000f;
                float z = (1f - Mathf.Cos(ang)) * radius * 1000f;
                rt.anchoredPosition = new Vector2(x, -z*0.08f);
                rt.sizeDelta = itemSize;
                rt.localRotation = Quaternion.Euler(0, -Mathf.Rad2Deg*ang, 0);
            }
        }
    }
}
