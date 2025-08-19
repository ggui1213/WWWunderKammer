#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class WWWK_CopyCardRectTransforms : EditorWindow
{
    [MenuItem("Tools/WWWunderkammer/Copy Card RectTransforms")]
    public static void Open() => GetWindow<WWWK_CopyCardRectTransforms>("Copy Card Rects");

    [Header("References")]
    public RectTransform referenceCard;   // 参考卡片（含子物体）
    public Transform applyParent;         // 父容器（在其子层级中寻找目标卡片）

    [Header("Which objects to affect")]
    public bool includeDescendants = true;
    public bool includeInactive = true;
    public bool onlyDirectChildren = false;  // 与 includeDescendants 二选一（UI 方便）

    [Header("Child names to copy (leave blank to skip that child)")]
    public string thumbMaskName = "ThumbMask";
    public string ringName = "Ring";
    public string gradientName = "ThumbGradient";
    public string titleName = "Title";

    [Header("Copy options")]
    public bool copyCardRootRect = true;       // 复制卡片根 Rect（size/anchors/pivot/pos/rot/scale）
    public bool enforceCardSizeToReference = true; // 强制目标卡片 sizeDelta 与参考一致（如果 Grid/Responsive 在改大小，可先禁用那些脚本）
    public bool copyAnchorsAndPivot = true;    // 同步 anchors/pivot，确保定位一致
    public bool copyChildren = true;           // 复制四个子物体（按名字匹配）
    public bool copyPosition = true;
    public bool copySize = true;
    public bool copyRotation = true;
    public bool copyScale = true;

    [Header("Auto-layout scripts handling")]
    public bool disableLegacyCardAutoLayout = true; // 停用旧版 CardAutoLayout
    public bool setV2ToOnDemandAndCapture = true;   // 对 CardAutoLayoutV2：切成 OnDemand 并调用 CaptureFromCurrent（采用你当前手调的样子）

    [Header("Persist (optional)")]
    public bool addWWWKLockerAndCapture = false;    // 给卡片与子物体添加 WWWKRectTransformLocker 并 Capture，防止保存/编译被改回
    public string lockerTypeName = "WWWK.Tools.WWWKRectTransformLocker, Assembly-CSharp";

    void OnGUI()
    {
        var so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("referenceCard"));
        EditorGUILayout.PropertyField(so.FindProperty("applyParent"));

        GUILayout.Label("Scope", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("includeDescendants"));
        EditorGUILayout.PropertyField(so.FindProperty("includeInactive"));
        EditorGUILayout.PropertyField(so.FindProperty("onlyDirectChildren"));

        GUILayout.Label("Child names", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("thumbMaskName"));
        EditorGUILayout.PropertyField(so.FindProperty("ringName"));
        EditorGUILayout.PropertyField(so.FindProperty("gradientName"));
        EditorGUILayout.PropertyField(so.FindProperty("titleName"));

        GUILayout.Label("Copy options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("copyCardRootRect"));
        EditorGUILayout.PropertyField(so.FindProperty("enforceCardSizeToReference"));
        EditorGUILayout.PropertyField(so.FindProperty("copyAnchorsAndPivot"));
        EditorGUILayout.PropertyField(so.FindProperty("copyChildren"));
        EditorGUILayout.PropertyField(so.FindProperty("copyPosition"));
        EditorGUILayout.PropertyField(so.FindProperty("copySize"));
        EditorGUILayout.PropertyField(so.FindProperty("copyRotation"));
        EditorGUILayout.PropertyField(so.FindProperty("copyScale"));

        GUILayout.Label("Auto-layout scripts", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("disableLegacyCardAutoLayout"));
        EditorGUILayout.PropertyField(so.FindProperty("setV2ToOnDemandAndCapture"));

        GUILayout.Label("Persist (optional)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("addWWWKLockerAndCapture"));
        if (addWWWKLockerAndCapture)
            EditorGUILayout.PropertyField(so.FindProperty("lockerTypeName"));

        so.ApplyModifiedProperties();

        GUILayout.Space(8);
        if (GUILayout.Button("复制到 选中对象", GUILayout.Height(28)))
        {
            CopyToTargets(Selection.gameObjects);
        }
        if (GUILayout.Button("复制到 父容器下所有卡片", GUILayout.Height(32)))
        {
            if (!applyParent){ EditorUtility.DisplayDialog("提示","请指定 applyParent。","OK"); return; }
            var list = CollectTargets(applyParent);
            CopyToTargets(list.ToArray());
        }

        EditorGUILayout.HelpBox(
            "说明：本工具会直接把参考卡片及其命名子物体(ThumbMask/Ring/ThumbGradient/Title)的 RectTransform 值复制到目标。\n" +
            "若目标尺寸与参考不同，建议勾选“强制卡片尺寸与参考一致”，或者先禁用 ResponsiveGrid/GridLayoutGroup 等会改尺寸的组件。\n" +
            "为避免保存/编译被其它脚本改回，建议：禁用旧版 CardAutoLayout；对 V2 切 OnDemand 并执行 Capture；或添加 WWWKRectTransformLocker。",
            MessageType.Info);
    }

    List<GameObject> CollectTargets(Transform root)
    {
        var list = new List<GameObject>();
        if (!root) return list;

        if (onlyDirectChildren)
        {
            foreach (Transform ch in root)
            {
                if (!includeInactive && !ch.gameObject.activeInHierarchy) continue;
                if (ch is RectTransform) list.Add(ch.gameObject);
            }
        }
        else if (includeDescendants)
        {
            var stack = new Stack<Transform>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                foreach (Transform ch in cur)
                {
                    if (!includeInactive && !ch.gameObject.activeInHierarchy) continue;
                    if (ch is RectTransform) list.Add(ch.gameObject);
                    stack.Push(ch);
                }
            }
        }
        else
        {
            foreach (Transform ch in root)
            {
                if (!includeInactive && !ch.gameObject.activeInHierarchy) continue;
                if (ch is RectTransform) list.Add(ch.gameObject);
            }
        }
        return list;
    }

    void CopyToTargets(GameObject[] targets)
    {
        if (!referenceCard){ EditorUtility.DisplayDialog("提示","请先指定 referenceCard（含子物体的卡片根）。","OK"); return; }

        int total=0, copiedCards=0, copiedChildren=0, disabledLegacy=0, v2Captured=0, locked=0;
        foreach (var go in targets)
        {
            if (!go) continue;
            if (!includeInactive && !go.activeInHierarchy) continue;
            var targetRT = go.GetComponent<RectTransform>();
            if (!targetRT) continue;
            total++;

            Undo.RegisterFullObjectHierarchyUndo(go, "WWWK Copy Card Rects");

            // 复制卡片根 Rect
            if (copyCardRootRect)
            {
                CopyRect(referenceCard, targetRT, copyAnchorsAndPivot, copyPosition, copySize && enforceCardSizeToReference, copyRotation, copyScale);
                copiedCards++;
            }

            // 复制子物体（按名字匹配）
            if (copyChildren)
            {
                copiedChildren += CopyChildByName(referenceCard, targetRT, thumbMaskName);
                copiedChildren += CopyChildByName(referenceCard, targetRT, ringName);
                copiedChildren += CopyChildByName(referenceCard, targetRT, gradientName);
                copiedChildren += CopyChildByName(referenceCard, targetRT, titleName);
            }

            // 处理自动布局脚本
            if (disableLegacyCardAutoLayout)
            {
                var legacy = go.GetComponent("CardAutoLayout") as Behaviour;
                if (legacy && legacy.enabled) { legacy.enabled = false; disabledLegacy++; }
            }
            if (setV2ToOnDemandAndCapture)
            {
                var v2 = go.GetComponent("WWWK.Menu.CardAutoLayoutV2") as Component;
                if (v2)
                {
                    var t = v2.GetType();
                    var modeProp = t.GetField("mode");
                    if (modeProp != null)
                    {
                        var enumType = modeProp.FieldType;
                        var onDemand = System.Enum.Parse(enumType, "OnDemand");
                        modeProp.SetValue(v2, onDemand);
                    }
                    var capture = t.GetMethod("CaptureFromCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (capture != null) { capture.Invoke(v2, null); v2Captured++; }
                }
            }

            // 持久化：加 Locker 并 Capture
            if (addWWWKLockerAndCapture)
            {
                var lockerType = System.Type.GetType(lockerTypeName);
                if (lockerType != null)
                {
                    // 给卡片根加
                    var lk = go.GetComponent(lockerType) as Component;
                    if (!lk) lk = Undo.AddComponent(go, lockerType) as Component;
                    var mCap = lockerType.GetMethod("CaptureNow", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    mCap?.Invoke(lk, null);
                    locked++;

                    // 给子物体加
                    TryAddAndCaptureLocker(targetRT, thumbMaskName, lockerType);
                    TryAddAndCaptureLocker(targetRT, ringName, lockerType);
                    TryAddAndCaptureLocker(targetRT, gradientName, lockerType);
                    TryAddAndCaptureLocker(targetRT, titleName, lockerType);
                }
                else
                {
                    Debug.LogWarning("[WWWK] Locker 类型未找到：" + lockerTypeName + "。请确认已导入 WWWKRectTransformLocker。");
                }
            }

            EditorUtility.SetDirty(go);
        }

        Debug.Log($"[WWWK] Copy Card Rects → Targets:{total}, RootCopied:{copiedCards}, ChildrenCopiedOps:{copiedChildren}, LegacyDisabled:{disabledLegacy}, V2Captured:{v2Captured}, LockersAdded:{locked}");
        EditorUtility.DisplayDialog("完成",
            $"处理完成：\n- 目标：{total}\n- 已复制根 Rect：{copiedCards}\n- 已复制子 Rect 操作数：{copiedChildren}\n- 停用旧版：{disabledLegacy}\n- V2 Capture：{v2Captured}\n- 加锁：{locked}\n\n如仍不生效，请先禁用会改尺寸/位置的布局脚本（ResponsiveGrid/GridLayoutGroup 等），或使用我提供的 Bake → Manual(Safe)。",
            "OK");
    }

    void TryAddAndCaptureLocker(RectTransform targetRT, string childName, System.Type lockerType)
    {
        if (string.IsNullOrEmpty(childName)) return;
        var child = FindChild(targetRT, childName);
        if (!child) return;
        var lk = child.GetComponent(lockerType) as Component;
        if (!lk) lk = Undo.AddComponent(child.gameObject, lockerType) as Component;
        var mCap = lockerType.GetMethod("CaptureNow", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        mCap?.Invoke(lk, null);
    }

    int CopyChildByName(RectTransform refRoot, RectTransform dstRoot, string childName)
    {
        if (string.IsNullOrEmpty(childName)) return 0;
        var src = FindChild(refRoot, childName);
        var dst = FindChild(dstRoot, childName);
        if (!src || !dst) return 0;
        CopyRect(src, dst, copyAnchorsAndPivot, copyPosition, copySize, copyRotation, copyScale);
        return 1;
    }

    RectTransform FindChild(Transform root, string n)
    {
        if (!root || string.IsNullOrEmpty(n)) return null;
        var t = root.Find(n);
        return t as RectTransform;
    }

    void CopyRect(RectTransform src, RectTransform dst, bool anchors, bool pos, bool size, bool rot, bool scale)
    {
        if (!src || !dst) return;
        // 先拷贝 anchors/pivot，避免 pos/size 被锚系影响
        if (anchors)
        {
            dst.anchorMin = src.anchorMin;
            dst.anchorMax = src.anchorMax;
            dst.pivot     = src.pivot;
        }
        if (pos)   dst.anchoredPosition = src.anchoredPosition;
        if (size)  dst.sizeDelta        = src.sizeDelta;
        if (rot)   dst.localRotation    = src.localRotation;
        if (scale) dst.localScale       = src.localScale;
    }
}
#endif
