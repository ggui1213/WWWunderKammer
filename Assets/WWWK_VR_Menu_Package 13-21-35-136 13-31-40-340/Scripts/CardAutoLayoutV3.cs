using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WWWK.Menu
{
    [ExecuteAlways]
    public class CardAutoLayoutV2 : MonoBehaviour
    {
        public enum LayoutMode { Live, OnDemand, Disabled }
        public LayoutMode mode = LayoutMode.OnDemand;

        [Header("Child Refs")]
        public RectTransform maskRT;       // "ThumbMask"
        public RectTransform ringRT;       // "Ring" (optional)
        public RectTransform gradientRT;   // "ThumbGradient" (optional)
        public RectTransform titleRT;      // "Title" (TMP)

        [Header("Anchor/Pivot")]
        public bool centerAnchorsAndPivot = true;

        [Header("Use Percentages (建议)")]
        public bool usePercentages = true;

        [Range(0.40f, 0.95f)] public float circleDiameterPercent = 0.68f;
        [Range(0.00f, 0.20f)] public float circleTopInsetPercent = 0.06f;
        [Range(0.15f, 0.40f)] public float titleHeightPercent = 0.22f;
        [Range(0.60f, 1.10f)] public float gradientWidthToCircle = 0.95f;
        [Range(0.30f, 0.90f)] public float gradientHeightToCircle = 0.58f;
        [Range(-0.50f, 0.50f)] public float gradientYOffsetToCircle = -0.20f;

        [Header("Absolute Pixels (当 usePercentages=false)")]
        public float circleDiameterPx = 220f;
        public float circleTopInsetPx = 18f;
        public float titleHeightPx = 72f;
        public float gradientWidthPx = 200f;
        public float gradientHeightPx = 120f;
        public float gradientYOffsetPx = -40f;

        [Header("Title Style Helper (可选)")]
        public bool applyTitleBestFit = true;
        public int titleMin = 18;
        public int titleMax = 46;

        RectTransform _rt;

        void Reset(){ Cache(); FindRefs(); Apply(); }
        void OnEnable(){ Cache(); if (mode == LayoutMode.Live) Apply(); }
        void OnValidate(){ if (!isActiveAndEnabled) return; Cache(); if (mode == LayoutMode.Live) Apply(); }
        void OnRectTransformDimensionsChange(){ if (mode == LayoutMode.Live) Apply(); }

        void Cache(){ if(!_rt) _rt = transform as RectTransform; }
        void FindRefs()
        {
            if (!maskRT     && transform.Find("ThumbMask"))     maskRT     = (RectTransform)transform.Find("ThumbMask");
            if (!ringRT     && transform.Find("Ring"))          ringRT     = (RectTransform)transform.Find("Ring");
            if (!gradientRT && transform.Find("ThumbGradient")) gradientRT = (RectTransform)transform.Find("ThumbGradient");
            if (!titleRT    && transform.Find("Title"))         titleRT    = (RectTransform)transform.Find("Title");
        }

        [ContextMenu("Apply Now")]
        public void Apply()
        {
            if (mode == LayoutMode.Disabled) return;
            if (!_rt) _rt = transform as RectTransform;
            FindRefs();

            if (centerAnchorsAndPivot)
            {
                if (_rt){ _rt.pivot = new Vector2(0.5f, 0.5f); _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f); }
                if (maskRT){ maskRT.pivot = new Vector2(0.5f,0.5f); maskRT.anchorMin = maskRT.anchorMax = new Vector2(0.5f,0.5f); }
                if (ringRT){ ringRT.pivot = new Vector2(0.5f,0.5f); ringRT.anchorMin = ringRT.anchorMax = new Vector2(0.5f,0.5f); }
                if (gradientRT){ gradientRT.pivot = new Vector2(0.5f,0.5f); gradientRT.anchorMin = gradientRT.anchorMax = new Vector2(0.5f,0.5f); }
                if (titleRT){ titleRT.pivot = new Vector2(0.5f,0.5f); titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f,0.5f); }
            }

            var size = _rt ? _rt.rect.size : new Vector2(300,300);

            float circleD, topInset, tHeight, gW, gH, gYOffset;
            if (usePercentages)
            {
                circleD  = Mathf.Min(size.x, size.y) * circleDiameterPercent;
                topInset = size.y * circleTopInsetPercent;
                tHeight  = size.y * titleHeightPercent;
                float ratioBase = circleD;
                gW       = ratioBase * gradientWidthToCircle;
                gH       = ratioBase * gradientHeightToCircle;
                gYOffset = ratioBase * gradientYOffsetToCircle;
            }
            else
            {
                circleD  = circleDiameterPx;
                topInset = circleTopInsetPx;
                tHeight  = titleHeightPx;
                gW       = gradientWidthPx;
                gH       = gradientHeightPx;
                gYOffset = gradientYOffsetPx;
            }

            Vector2 ringPos = new Vector2(0, -topInset);
            if (maskRT){ maskRT.sizeDelta = new Vector2(circleD, circleD); maskRT.anchoredPosition = ringPos; }
            if (ringRT){ ringRT.sizeDelta = new Vector2(circleD, circleD); ringRT.anchoredPosition = ringPos; ringRT.SetAsLastSibling(); }
            if (gradientRT)
            {
                gradientRT.sizeDelta = new Vector2(gW, gH);
                gradientRT.anchoredPosition = new Vector2(0, -topInset + gYOffset);
                var img = gradientRT.GetComponent<Image>(); if (img){ var c = img.color; if (c.a < 0.95f) c.a = 0.95f; img.color = c; }
            }
            if (titleRT)
            {
                titleRT.sizeDelta = new Vector2(size.x * 0.9f, tHeight);
                titleRT.anchoredPosition = new Vector2(0, -size.y * 0.5f + tHeight * 0.55f);
                var tmp = titleRT.GetComponent<TMP_Text>();
                if (tmp && applyTitleBestFit) { tmp.enableAutoSizing = true; tmp.fontSizeMin = titleMin; tmp.fontSizeMax = titleMax; tmp.alignment = TextAlignmentOptions.Center; }
            }
        }

        [ContextMenu("Capture From Current (Learn Manual Layout)")]
        public void CaptureFromCurrent()
        {
            if (!_rt) _rt = transform as RectTransform;
            FindRefs();
            var size = _rt ? _rt.rect.size : new Vector2(300,300);

            if (maskRT)
            {
                float d = Mathf.Min(size.x, size.y);
                float cd = maskRT.rect.width;
                float top = -maskRT.anchoredPosition.y;
                circleDiameterPercent = Mathf.Clamp01(cd / d);
                circleTopInsetPercent = Mathf.Clamp01(size.y > 1e-3f ? top / size.y : 0f);
                circleDiameterPx = cd;
                circleTopInsetPx = top;
            }

            if (titleRT)
            {
                float h = titleRT.rect.height;
                titleHeightPercent = Mathf.Clamp01(size.y > 1e-3f ? h / size.y : 0f);
                titleHeightPx = h;
            }

            if (gradientRT && maskRT)
            {
                float cd = maskRT.rect.width;
                float gW = gradientRT.rect.width;
                float gH = gradientRT.rect.height;
                float gOff = gradientRT.anchoredPosition.y - maskRT.anchoredPosition.y; // 相对圆心的偏移
                gradientWidthToCircle   = Mathf.Clamp(gW / Mathf.Max(1e-3f, cd), 0.1f, 2f);
                gradientHeightToCircle  = Mathf.Clamp(gH / Mathf.Max(1e-3f, cd), 0.1f, 2f);
                gradientYOffsetToCircle = Mathf.Clamp(gOff / Mathf.Max(1e-3f, cd), -1.0f, 1.0f);

                gradientWidthPx = gW;
                gradientHeightPx = gH;
                gradientYOffsetPx = gOff;
            }

            // 学完后：用百分比 + 按需刷新，避免再次覆盖手动调整
            usePercentages = true;
            mode = LayoutMode.OnDemand;
        }
    }
}
