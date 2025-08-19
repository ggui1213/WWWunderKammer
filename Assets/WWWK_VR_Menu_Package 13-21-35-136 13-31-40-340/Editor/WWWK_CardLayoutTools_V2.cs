#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using WWWK.Menu;

public class WWWK_CardLayoutTools_V2 : EditorWindow
{
    [MenuItem("Tools/WWWunderkammer/Card Layout Tools (V2)")]
    public static void Open() => GetWindow<WWWK_CardLayoutTools_V2>("Card Layout Tools (V2)");

    public CardAutoLayoutV2 reference;
    public Transform applyParent;
    public bool includeInactive = true;
    public bool includeDescendants = true;
    public bool onlyHasCardAutoLayout = false;
    public bool autoAddIfMissing = true;
    public bool autoBindChildRefsByName = true;
    public bool disableLegacyScripts = true; // 关闭/移除旧版 CardAutoLayout（避免刷新覆盖）
    public bool removeLegacyInsteadOfDisable = false;
    public bool applyAndExecute = true;

    public string thumbMaskName = "ThumbMask";
    public string ringName = "Ring";
    public string gradientName = "ThumbGradient";
    public string titleName = "Title";

    void OnGUI()
    {
        var so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("reference"));
        EditorGUILayout.PropertyField(so.FindProperty("applyParent"));
        EditorGUILayout.PropertyField(so.FindProperty("includeInactive"));
        EditorGUILayout.PropertyField(so.FindProperty("includeDescendants"));
        EditorGUILayout.PropertyField(so.FindProperty("onlyHasCardAutoLayout"));
        EditorGUILayout.PropertyField(so.FindProperty("autoAddIfMissing"));
        EditorGUILayout.PropertyField(so.FindProperty("autoBindChildRefsByName"));
        EditorGUILayout.PropertyField(so.FindProperty("disableLegacyScripts"));
        if (disableLegacyScripts)
            EditorGUILayout.PropertyField(so.FindProperty("removeLegacyInsteadOfDisable"));
        EditorGUILayout.PropertyField(so.FindProperty("applyAndExecute"));

        GUILayout.Space(6);
        GUILayout.Label("Child names for auto-binding:", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("thumbMaskName"));
        EditorGUILayout.PropertyField(so.FindProperty("ringName"));
        EditorGUILayout.PropertyField(so.FindProperty("gradientName"));
        EditorGUILayout.PropertyField(so.FindProperty("titleName"));

        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        if (GUILayout.Button("把参考卡片布局复制给 选中对象", GUILayout.Height(30))) CopyToTargets(Selection.gameObjects);
        if (GUILayout.Button("把参考卡片布局复制给 父节点下所有卡片", GUILayout.Height(34)))
        {
            if (!applyParent){ EditorUtility.DisplayDialog("提示","请指定 applyParent（包含卡片的容器）。","OK"); return; }
            var list = CollectUnder(applyParent);
            CopyToTargets(list.ToArray());
        }

        EditorGUILayout.HelpBox(
            "若看起来“没反应”，通常是：\n 1) 目标上没有 CardAutoLayoutV2（勾选 autoAddIfMissing 自动添加）\n 2) 旧版 CardAutoLayout 还在运行（勾选 disableLegacyScripts）\n 3) 选的父节点不是卡片的真正父级或卡片在更深层（勾选 includeDescendants）\n 4) 复制后没有 Apply（勾 applyAndExecute）",
            MessageType.Info);
    }

    List<GameObject> CollectUnder(Transform root)
    {
        var list = new List<GameObject>();
        if (!root) return list;

        void AddIfValid(Transform t)
        {
            if (!includeInactive && !t.gameObject.activeInHierarchy) return;
            bool ok = true;
            if (onlyHasCardAutoLayout)
                ok = t.GetComponent<CardAutoLayoutV2>() != null;
            if (ok) list.Add(t.gameObject);
        }

        if (!includeDescendants)
        {
            foreach (Transform t in root) AddIfValid(t);
        }
        else
        {
            var stack = new Stack<Transform>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                foreach (Transform ch in cur)
                {
                    AddIfValid(ch);
                    stack.Push(ch);
                }
            }
        }
        return list;
    }

    void CopyToTargets(GameObject[] targets)
    {
        if (!reference)
        {
            // 如果没指定，尝试从当前选中对象里找一个
            foreach (var go in Selection.gameObjects)
            {
                var r = go.GetComponent<CardAutoLayoutV2>();
                if (r){ reference = r; break; }
            }
        }
        if (!reference){ EditorUtility.DisplayDialog("提示","请先指定一个含 CardAutoLayoutV2 的参考卡片。","OK"); return; }

        int total = 0, added = 0, copied = 0, applied = 0, legacy = 0;
        foreach (var go in targets)
        {
            if (!go) continue;
            if (!includeInactive && !go.activeInHierarchy) continue;
            total++;

            var cal = go.GetComponent<CardAutoLayoutV2>();
            if (!cal && autoAddIfMissing)
            {
                cal = Undo.AddComponent<CardAutoLayoutV2>(go);
                added++;
            }
            if (!cal) continue;

            // 自动绑定子引用
            if (autoBindChildRefsByName)
            {
                if (!cal.maskRT && !string.IsNullOrEmpty(thumbMaskName) && go.transform.Find(thumbMaskName))
                    cal.maskRT = go.transform.Find(thumbMaskName) as RectTransform;
                if (!cal.ringRT && !string.IsNullOrEmpty(ringName) && go.transform.Find(ringName))
                    cal.ringRT = go.transform.Find(ringName) as RectTransform;
                if (!cal.gradientRT && !string.IsNullOrEmpty(gradientName) && go.transform.Find(gradientName))
                    cal.gradientRT = go.transform.Find(gradientName) as RectTransform;
                if (!cal.titleRT && !string.IsNullOrEmpty(titleName) && go.transform.Find(titleName))
                    cal.titleRT = go.transform.Find(titleName) as RectTransform;
            }

            // 复制参数
            Undo.RecordObject(cal, "Copy CardAutoLayoutV2");
            cal.mode = reference.mode;
            cal.centerAnchorsAndPivot = reference.centerAnchorsAndPivot;
            cal.usePercentages = reference.usePercentages;

            cal.circleDiameterPercent   = reference.circleDiameterPercent;
            cal.circleTopInsetPercent   = reference.circleTopInsetPercent;
            cal.titleHeightPercent      = reference.titleHeightPercent;
            cal.gradientWidthToCircle   = reference.gradientWidthToCircle;
            cal.gradientHeightToCircle  = reference.gradientHeightToCircle;
            cal.gradientYOffsetToCircle = reference.gradientYOffsetToCircle;

            cal.circleDiameterPx  = reference.circleDiameterPx;
            cal.circleTopInsetPx  = reference.circleTopInsetPx;
            cal.titleHeightPx     = reference.titleHeightPx;
            cal.gradientWidthPx   = reference.gradientWidthPx;
            cal.gradientHeightPx  = reference.gradientHeightPx;
            cal.gradientYOffsetPx = reference.gradientYOffsetPx;

            cal.applyTitleBestFit = reference.applyTitleBestFit;
            cal.titleMin = reference.titleMin;
            cal.titleMax = reference.titleMax;

            EditorUtility.SetDirty(cal);
            copied++;

            // 处理旧版 CardAutoLayout（避免它继续覆盖）
            if (disableLegacyScripts)
            {
                var monos = go.GetComponents<MonoBehaviour>();
                foreach (var m in monos)
                {
                    if (!m) continue;
                    var t = m.GetType();
                    if (t == typeof(CardAutoLayoutV2)) continue;
                    if (t.Name == "CardAutoLayout")
                    {
                        legacy++;
                        if (removeLegacyInsteadOfDisable) Undo.DestroyObjectImmediate(m);
                        else
                        {
                            var beh = m as Behaviour;
                            if (beh) beh.enabled = false;
                        }
                    }
                }
            }

            // 立即应用
            if (applyAndExecute)
            {
                cal.Apply();
                applied++;
            }
        }

        Debug.Log($"[WWWK] Card Layout Tools (V2) → Targets:{total}, Added:{added}, Copied:{copied}, Applied:{applied}, LegacyDisabled/Removed:{legacy}");
        EditorUtility.DisplayDialog("完成",
            $"处理完成：\n- 遍历对象：{total}\n- 新增组件：{added}\n- 复制参数：{copied}\n- 立即应用：{applied}\n- 停用/移除旧版：{legacy}\n\n如果仍看起来“没变化”，请确认：\n1) 参考卡片确实长得对；\n2) 目标的子物体命名与面板填写一致；\n3) 父级 ResponsiveGrid/布局脚本未干预内部子布局。",
            "OK");
    }
}
#endif
