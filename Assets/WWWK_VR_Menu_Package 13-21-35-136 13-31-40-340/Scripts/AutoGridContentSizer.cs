using UnityEngine;
using UnityEngine.UI;

/// 自动根据 Grid 的列数/卡片尺寸/间距计算 Content 高度（竖向滚动）
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AutoGridContentSizer : MonoBehaviour
{
    public GridLayoutGroup grid;
    public int columnsOverride = 0;      // 0=用 grid.constraintCount
    public float extraBottomPadding = 0f;
    public bool runEveryFrameInEditMode = true;

    RectTransform _rt;
    void Reset(){ _rt = GetComponent<RectTransform>(); if (!grid) grid = GetComponent<GridLayoutGroup>(); Apply(); }
    void OnEnable(){ _rt = GetComponent<RectTransform>(); if (!grid) grid = GetComponent<GridLayoutGroup>(); Apply(); }
    void OnTransformChildrenChanged(){ Apply(); }
    void Update(){ if (!Application.isPlaying && runEveryFrameInEditMode) Apply(); }
    void LateUpdate(){ if (Application.isPlaying) Apply(); }

    public void Apply()
    {
        if (!_rt || !grid) return;
        int cols = columnsOverride > 0 ? columnsOverride : Mathf.Max(1, grid.constraintCount);
        if (grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount) cols = Mathf.Max(1, cols);

        int active = 0;
        for (int i = 0; i < _rt.childCount; i++) if (_rt.GetChild(i).gameObject.activeSelf) active++;
        int rows = Mathf.Max(1, Mathf.CeilToInt(active / (float)cols));

        var pad = grid.padding; var sp = grid.spacing; var cell = grid.cellSize;
        float height = pad.top + pad.bottom + rows * cell.y + (rows - 1) * sp.y + extraBottomPadding;

        _rt.anchorMin = new Vector2(0f, 1f);
        _rt.anchorMax = new Vector2(1f, 1f);
        _rt.pivot     = new Vector2(0.5f, 1f);

        var size = _rt.sizeDelta;
        size.y = height;
        _rt.sizeDelta = size;
    }
}
