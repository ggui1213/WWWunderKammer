using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WWWK.Menu
{
    /// <summary>
    /// 让 GridLayoutGroup 根据行列数自动算 cellSize，避免不同分辨率/面板尺寸下卡片重叠。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
    public class ResponsiveGrid : MonoBehaviour
    {
        public int columns = 3;
        public int rows = 2;
        public Vector2 spacing = new Vector2(18, 18);
        public Vector2 padding = new Vector2(16, 16);

        RectTransform _rt;
        GridLayoutGroup _grid;

        void Reset(){ Cache(); Apply(); }
        void OnEnable(){ Cache(); Apply(); }
        void OnValidate(){ columns=Mathf.Max(1,columns); rows=Mathf.Max(1,rows); Cache(); Apply(); }
        void OnRectTransformDimensionsChange(){ Apply(); }

        void Cache(){ if(!_rt) _rt = GetComponent<RectTransform>(); if(!_grid) _grid = GetComponent<GridLayoutGroup>(); }

        public void Apply()
        {
            if (!_rt || !_grid) return;
            var size = _rt.rect.size;
            var availW = size.x - padding.x*2f - spacing.x*(columns-1);
            var availH = size.y - padding.y*2f - spacing.y*(rows-1);
            float cellW = Mathf.Floor(availW / columns);
            float cellH = Mathf.Floor(availH / rows);

            _grid.cellSize = new Vector2(cellW, cellH);
            _grid.spacing = spacing;
            _grid.padding = new RectOffset((int)padding.x,(int)padding.x,(int)padding.y,(int)padding.y);
            _grid.childAlignment = TextAnchor.MiddleCenter;
            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = columns;
        }
    }

    /// <summary>
    /// 让卡片内部的圆形缩略图/外环/渐隐/标题随卡片尺寸自适应。
    /// </summary>
    [ExecuteAlways]
    public class CardAutoLayout : MonoBehaviour
    {
        public RectTransform maskRT;       // "ThumbMask"
        public RectTransform ringRT;       // "Ring" (optional)
        public RectTransform gradientRT;   // "ThumbGradient" (optional)
        public RectTransform titleRT;      // "Title"

        [Range(0.4f, 0.95f)] public float circleDiameterPercent = 0.68f;
        [Range(0f, 0.2f)]   public float circleTopInsetPercent = 0.08f;
        [Range(0.15f,0.4f)] public float titleHeightPercent = 0.24f;

        void Reset(){ FindRefs(); Apply(); }
        void OnEnable(){ Apply(); }
        void OnValidate(){ Apply(); }
        void OnRectTransformDimensionsChange(){ Apply(); }

        void FindRefs()
        {
            if (!maskRT     && transform.Find("ThumbMask"))     maskRT     = (RectTransform)transform.Find("ThumbMask");
            if (!ringRT     && transform.Find("Ring"))          ringRT     = (RectTransform)transform.Find("Ring");
            if (!gradientRT && transform.Find("ThumbGradient")) gradientRT = (RectTransform)transform.Find("ThumbGradient");
            if (!titleRT    && transform.Find("Title"))         titleRT    = (RectTransform)transform.Find("Title");
        }

        public void Apply()
        {
            var rt = transform as RectTransform; if (!rt) return;
            FindRefs();

            var size = rt.rect.size;
            float d = Mathf.Min(size.x, size.y) * circleDiameterPercent;
            float topInset = size.y * circleTopInsetPercent;

            if (maskRT){ maskRT.sizeDelta = new Vector2(d, d); maskRT.anchoredPosition = new Vector2(0, -topInset); }
            if (ringRT){ ringRT.sizeDelta = new Vector2(d, d); ringRT.anchoredPosition = new Vector2(0, -topInset); ringRT.SetAsLastSibling(); }

            if (gradientRT){
                gradientRT.sizeDelta = new Vector2(d*0.95f, d*0.62f);
                gradientRT.anchoredPosition = new Vector2(0, -topInset - d*0.20f);
                var img = gradientRT.GetComponent<Image>();
                if (img){ var c = img.color; c.a = 0.95f; img.color = c; }
            }

            if (titleRT){
                titleRT.sizeDelta = new Vector2(size.x*0.9f, size.y*titleHeightPercent);
                titleRT.anchoredPosition = new Vector2(0, -size.y*0.45f);
            }
        }
    }
}
