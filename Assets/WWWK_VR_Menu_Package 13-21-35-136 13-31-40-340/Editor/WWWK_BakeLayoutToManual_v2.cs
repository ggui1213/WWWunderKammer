#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class WWWK_BakeLayoutToManual_v2 : EditorWindow
{
    [MenuItem("Tools/WWWunderkammer/Bake Grid → Manual (Safe)")]
    public static void Open() => GetWindow<WWWK_BakeLayoutToManual_v2>("Bake Grid → Manual (Safe)");

    public Transform parent;
    public bool duplicateBackup = true;
    public bool setParentCenter = true;
    public bool centerChildren = true;
    public bool keepSizes = true;

    public bool removeGridLayoutGroup = true;
    public bool removeContentSizeFitter = true;
    public bool removeAutoGridPadding = true;
    public bool removeHorizontalVerticalLayout = true;
    public bool disableRowSnap = true;
    public bool tryRemoveDependents = true;
    public string[] extraDependentTypes = new string[]{ "ResponsiveGrid" };

    public bool fallbackIgnoreLayout = true;
    public bool addIgnoreLayoutToChildren = true;

    [Serializable]
    struct Saved
    {
        public RectTransform rt;
        public Vector2 pos;
        public Vector2 size;
        public Quaternion rot;
        public Vector3 scale;
    }

    void OnGUI()
    {
        var so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("parent"));
        EditorGUILayout.PropertyField(so.FindProperty("duplicateBackup"));
        EditorGUILayout.PropertyField(so.FindProperty("setParentCenter"));
        EditorGUILayout.PropertyField(so.FindProperty("centerChildren"));
        EditorGUILayout.PropertyField(so.FindProperty("keepSizes"));
        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(so.FindProperty("removeGridLayoutGroup"));
        EditorGUILayout.PropertyField(so.FindProperty("removeContentSizeFitter"));
        EditorGUILayout.PropertyField(so.FindProperty("removeAutoGridPadding"));
        EditorGUILayout.PropertyField(so.FindProperty("removeHorizontalVerticalLayout"));
        EditorGUILayout.PropertyField(so.FindProperty("disableRowSnap"));
        EditorGUILayout.PropertyField(so.FindProperty("tryRemoveDependents"));
        EditorGUILayout.PropertyField(so.FindProperty("extraDependentTypes"), true);
        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(so.FindProperty("fallbackIgnoreLayout"));
        EditorGUILayout.PropertyField(so.FindProperty("addIgnoreLayoutToChildren"));
        so.ApplyModifiedProperties();

        if (GUILayout.Button("Bake → 手动（安全执行）", GUILayout.Height(34)))
        {
            if (!parent){ EditorUtility.DisplayDialog("提示","请指定 Parent。","OK"); return; }
            BakeSafe();
        }
    }

    void BakeSafe()
    {
        Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "WWWK Bake Grid → Manual (Safe)");

        var prt = parent as RectTransform;
        Canvas.ForceUpdateCanvases();
        if (prt) LayoutRebuilder.ForceRebuildLayoutImmediate(prt);

        if (duplicateBackup)
        {
            var copy = GameObject.Instantiate(parent.gameObject, parent.parent);
            copy.name = parent.name + "_Backup(BeforeBake)";
            copy.SetActive(false);
        }

        var saved = new List<Saved>();
        foreach (Transform ch in parent)
        {
            var rt = ch as RectTransform; if (!rt) continue;
            saved.Add(new Saved{ rt=rt, pos=rt.anchoredPosition, size=rt.sizeDelta, rot=rt.localRotation, scale=rt.localScale });
        }

        if (setParentCenter && prt){ prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f,0.5f); }
        if (centerChildren){ foreach (var s in saved){ s.rt.anchorMin = s.rt.anchorMax = s.rt.pivot = new Vector2(0.5f,0.5f); } }

        GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
        if (tryRemoveDependents && grid) RemoveDependents(parent.gameObject, grid);

        if (removeContentSizeFitter){ var f = parent.GetComponent<ContentSizeFitter>(); if (f) DestroyImmediate(f); }
        if (removeAutoGridPadding){ var auto = parent.GetComponent("AutoGridPadding") as Component; if (auto) DestroyImmediate(auto); }
        if (removeHorizontalVerticalLayout)
        {
            var h = parent.GetComponent<HorizontalLayoutGroup>(); if (h) DestroyImmediate(h);
            var v = parent.GetComponent<VerticalLayoutGroup>();   if (v) DestroyImmediate(v);
        }
        if (disableRowSnap)
        {
            var rs = parent.GetComponent("RowSnap") as Behaviour;
            if (rs) rs.enabled = false;
        }

        bool gridRemoved = false;
        grid = parent.GetComponent<GridLayoutGroup>();
        if (grid && removeGridLayoutGroup)
        {
            try { DestroyImmediate(grid); gridRemoved = true; }
            catch (Exception ex) { Debug.LogWarning("[WWWK] 无法删除 GridLayoutGroup: " + ex.Message); gridRemoved = false; }
        }

        if (!gridRemoved)
        {
            grid = parent.GetComponent<GridLayoutGroup>();
            if (grid && fallbackIgnoreLayout) grid.enabled = false;
            if (addIgnoreLayoutToChildren)
            {
                foreach (var s in saved)
                {
                    var le = s.rt.GetComponent<LayoutElement>();
                    if (!le) le = s.rt.gameObject.AddComponent<LayoutElement>();
                    le.ignoreLayout = true;
                }
            }
        }

        foreach (var s in saved)
        {
            s.rt.anchoredPosition = s.pos;
            if (keepSizes) s.rt.sizeDelta = s.size;
            s.rt.localRotation = s.rot;
            s.rt.localScale = s.scale;
        }

        Canvas.ForceUpdateCanvases();
        if (prt) LayoutRebuilder.ForceRebuildLayoutImmediate(prt);

        EditorUtility.DisplayDialog("完成","当前布局已烘焙为手动，且处理了对 Grid 的依赖。","OK");
    }

    void RemoveDependents(GameObject go, GridLayoutGroup grid)
    {
        var comps = go.GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (!c) continue;
            var t = c.GetType();

            // 优先删除明确列出的类型
            foreach (var name in extraDependentTypes)
            {
                if (!string.IsNullOrEmpty(name) && t.Name == name)
                {
                    DestroyImmediate(c);
                    break;
                }
            }

            // RequireComponent(GridLayoutGroup) 的也移除
            var attrs = (RequireComponent[])t.GetCustomAttributes(typeof(RequireComponent), true);
            foreach (var a in attrs)
            {
                if (a.m_Type0 == typeof(GridLayoutGroup) || a.m_Type1 == typeof(GridLayoutGroup) || a.m_Type2 == typeof(GridLayoutGroup))
                {
                    DestroyImmediate(c);
                    break;
                }
            }
        }
    }
}
#endif
