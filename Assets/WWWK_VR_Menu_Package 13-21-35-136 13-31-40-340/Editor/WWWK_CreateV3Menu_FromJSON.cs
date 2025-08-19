#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class WWWK_CreateV3Menu_FromJSON : EditorWindow
{
    [MenuItem("Tools/WWWunderkammer/V3 Grid Menu → Build From JSON")]
    public static void Open() => GetWindow<WWWK_CreateV3Menu_FromJSON>("V3 Grid Menu From JSON");

    [Header("Sources")]
    public TextAsset jsonFile;
    public string externalJsonPath;

    [Header("Targets")]
    public Transform gridParent;
    public GameObject sceneCardPrefab;

    [Header("Grid Settings (optional)")]
    public bool ensureGridLayoutGroup = true;
    public int columns = 4;
    public int rows = 0; // 0=auto
    public Vector2 spacing = new Vector2(20, 20);
    public Vector2 padding = new Vector2(48, 48);
    public TextAnchor childAlignment = TextAnchor.MiddleCenter;

    [Header("Build Options")]
    public bool clearExistingChildren = true;
    public bool nameCardsAs = true; // "Card_{order:00}_{id}"
    public bool useOrder = true;    // sort items by 'order' (ascending)
    public bool tryBindButtonToScene = false;  // if card has a Button, bind to open scenePath in Editor (playmode: SceneManager)
    public bool selectCreated = true;

    [Serializable]
    class SceneCardJson
    {
        public string id;
        public string title;
        public string scenePath;
        public string thumbnail;
        public string description;
        public int order = 0;
        public string[] tags;
    }

    [Serializable]
    class SceneCardJsonList
    {
        public SceneCardJson[] cards;
    }

    void OnGUI()
    {
        var so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("jsonFile"));
        EditorGUILayout.PropertyField(so.FindProperty("externalJsonPath"));
        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(so.FindProperty("gridParent"));
        EditorGUILayout.PropertyField(so.FindProperty("sceneCardPrefab"));
        EditorGUILayout.Space(4);
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("ensureGridLayoutGroup"));
        EditorGUILayout.PropertyField(so.FindProperty("columns"));
        EditorGUILayout.PropertyField(so.FindProperty("rows"));
        EditorGUILayout.PropertyField(so.FindProperty("spacing"));
        EditorGUILayout.PropertyField(so.FindProperty("padding"));
        EditorGUILayout.PropertyField(so.FindProperty("childAlignment"));
        EditorGUILayout.Space(4);
        GUILayout.Label("Build Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("clearExistingChildren"));
        EditorGUILayout.PropertyField(so.FindProperty("nameCardsAs"));
        EditorGUILayout.PropertyField(so.FindProperty("useOrder"));
        EditorGUILayout.PropertyField(so.FindProperty("tryBindButtonToScene"));
        EditorGUILayout.PropertyField(so.FindProperty("selectCreated"));
        so.ApplyModifiedProperties();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Build / Update Grid From JSON", GUILayout.Height(36)))
        {
            Build();
        }

        EditorGUILayout.HelpBox("JSON: cards[{ id,title,scenePath,thumbnail,description,order,tags }]\nPrefab children: ThumbMask(Image/RawImage), Title(TMP/Text), Ring(opt), ThumbGradient(opt)", MessageType.Info);
    }

    void Build()
    {
        if (!gridParent){ EditorUtility.DisplayDialog("提示","请指定 gridParent。","OK"); return; }
        if (!sceneCardPrefab){ EditorUtility.DisplayDialog("提示","请指定 sceneCardPrefab。","OK"); return; }

        var cards = LoadCards();
        if (cards == null || cards.Length == 0)
        {
            EditorUtility.DisplayDialog("提示","JSON 中没有 cards。","OK"); return;
        }

        if (useOrder) cards = cards.OrderBy(c => c.order).ToArray();

        Undo.RegisterFullObjectHierarchyUndo(gridParent.gameObject, "WWWK V3 Build From JSON");

        // Ensure grid
        GridLayoutGroup grid = null;
        if (ensureGridLayoutGroup)
        {
            grid = gridParent.GetComponent<GridLayoutGroup>();
            if (!grid) grid = Undo.AddComponent<GridLayoutGroup>(gridParent.gameObject);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, columns);
            grid.spacing = spacing;
            grid.padding = new RectOffset((int)padding.x, (int)padding.x, (int)padding.y, (int)padding.y);
            grid.childAlignment = childAlignment;
            // Auto cell size if rows > 0
            var rt = gridParent as RectTransform;
            if (rt && columns > 0 && rows > 0)
            {
                var size = rt.rect.size;
                var availW = size.x - padding.x*2f - spacing.x*(columns-1);
                var availH = size.y - padding.y*2f - spacing.y*(rows-1);
                var cellW = Mathf.Floor(availW / columns);
                var cellH = Mathf.Floor(availH / rows);
                grid.cellSize = new Vector2(cellW, cellH);
            }
        }

        if (clearExistingChildren)
        {
            var toDelete = new List<GameObject>();
            foreach (Transform ch in gridParent) toDelete.Add(ch.gameObject);
            foreach (var go in toDelete) Undo.DestroyObjectImmediate(go);
        }

        var created = new List<GameObject>();
        int idx = 0;
        foreach (var c in cards)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(sceneCardPrefab, gridParent);
            if (!go) go = GameObject.Instantiate(sceneCardPrefab, gridParent);
            go.name = nameCardsAs ? $"Card_{c.order:00}_{c.id}" : sceneCardPrefab.name + "_" + idx;

            // Bind meta
            var meta = go.GetComponent<WWWK.SceneCardMeta>();
            if (!meta) meta = go.AddComponent<WWWK.SceneCardMeta>();
            meta.id = c.id;
            meta.title = string.IsNullOrEmpty(c.title) ? c.id : c.title;
            meta.description = c.description;
            meta.scenePath = c.scenePath;
            meta.tags = c.tags;

            // Title
            var titleT = go.transform.Find("Title");
            if (titleT)
            {
                var tmp = titleT.GetComponent<TMP_Text>();
                if (tmp) tmp.text = meta.title;
                else
                {
                    var t = titleT.GetComponent<UnityEngine.UI.Text>();
                    if (t) t.text = meta.title;
                }
            }

            // Thumb
            var thumbT = go.transform.Find("ThumbMask");
            if (thumbT)
            {
                var img = thumbT.GetComponent<Image>();
                var raw = thumbT.GetComponent<RawImage>();
                TryAssignThumbnail(c.thumbnail, img, raw);
            }

            // Button (optional open scene in editor)
            if (tryBindButtonToScene && !string.IsNullOrEmpty(c.scenePath))
            {
                var btn = go.GetComponentInChildren<Button>();
                if (btn)
                {
                    var sp = c.scenePath;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (Application.isPlaying)
                        {
                            Debug.Log("[WWWK] At runtime, handle SceneManager.LoadScene for: " + sp);
                        }
                        else
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                EditorSceneManager.OpenScene(sp);
                        }
                    });
                }
            }

            created.Add(go);
            idx++;
        }

        Selection.objects = selectCreated ? created.ToArray() : new UnityEngine.Object[]{ gridParent.gameObject };
        EditorGUIUtility.PingObject(gridParent);

        EditorUtility.DisplayDialog("完成",
            $"已根据 JSON 生成 {created.Count} 张卡片。\n列数：{columns} 间距：{spacing} 内边距：{padding}",
            "OK");
    }

    [Serializable]
    class SceneCardJsonArr { public SceneCardJson[] items; }

    SceneCardJson[] LoadCards()
    {
        string json = null;
        if (jsonFile) json = jsonFile.text;
        else if (!string.IsNullOrEmpty(externalJsonPath) && File.Exists(externalJsonPath)) json = File.ReadAllText(externalJsonPath);

        if (string.IsNullOrEmpty(json))
        {
            EditorUtility.DisplayDialog("JSON 未找到", "请指定 jsonFile 或 externalJsonPath。", "OK");
            return null;
        }

        try
        {
            var list = JsonUtility.FromJson<SceneCardJsonList>(json);
            if (list != null && list.cards != null) return list.cards;
        }
        catch {}

        try
        {
            var arr = JsonUtility.FromJson<SceneCardJsonArr>("{\"items\":" + json + "}");
            if (arr != null && arr.items != null) return arr.items;
        }
        catch {}

        Debug.LogError("[WWWK] 解析 JSON 失败或为空。");
        return null;
    }

    void TryAssignThumbnail(string assetPath, Image img, RawImage raw)
    {
        if (string.IsNullOrEmpty(assetPath)) return;
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sp && img){ img.sprite = sp; return; }
        if (sp && raw){ raw.texture = sp.texture; return; }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex && raw){ raw.texture = tex; return; }
        if (tex && img)
        {
            var s = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f), 100f);
            img.sprite = s;
        }
    }
}
#endif
