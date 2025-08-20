using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

public class MenuCardUpdater_Auto : EditorWindow
{
    [Header("目标 Grid (不会清空子物体)")]
    public Transform gridParent;

    [Header("缩略图与场景的搜索文件夹")]
    public DefaultAsset thumbnailsFolder;
    public DefaultAsset scenesFolder;

    [Header("匹配策略")]
    public bool matchByCardName = true;     // true: 用卡片的 key/名字 匹配同名 sprite/scene
    public bool sequentialFill = true;      // true: 顺序填充（自然排序）
    public bool replaceUnderscoreWithSpace = true;

    [MenuItem("Tools/WWWK/Card Updater (Auto)")]
    public static void Open() => GetWindow<MenuCardUpdater_Auto>("Card Updater (Auto)");

    private void OnGUI()
    {
        var so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("gridParent"));
        EditorGUILayout.PropertyField(so.FindProperty("thumbnailsFolder"));
        EditorGUILayout.PropertyField(so.FindProperty("scenesFolder"));
        EditorGUILayout.Space(6);
        EditorGUILayout.PropertyField(so.FindProperty("matchByCardName"));
        EditorGUILayout.PropertyField(so.FindProperty("sequentialFill"));
        EditorGUILayout.PropertyField(so.FindProperty("replaceUnderscoreWithSpace"));
        so.ApplyModifiedProperties();

        GUI.enabled = gridParent && thumbnailsFolder;
        if (GUILayout.Button("覆盖现有卡片（自动匹配）")) UpdateAuto();
        GUI.enabled = true;
    }

    private void UpdateAuto()
    {
        var sprites = LoadSprites(thumbnailsFolder).OrderBy(s => s.name, new NaturalComparer()).ToList();
        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("失败", "缩略图文件夹中没有 Sprite。", "OK");
            return;
        }

        var sceneDict = BuildSceneDict(scenesFolder);

        int idxSprite = 0, updated = 0;
        for (int i = 0; i < gridParent.childCount; i++)
        {
            var view = gridParent.GetChild(i).GetComponent<MenuCardView>();
            if (!view) continue;

            string key = GetCardKeyCompat(view); // 兼容读取 key
            Sprite choose = null;
            string title = null;
            string scenePath = null;

            if (matchByCardName && !string.IsNullOrEmpty(key))
            {
                choose = sprites.FirstOrDefault(s => s.name == key);
                if (sceneDict.TryGetValue(key, out var pByKey))
                    scenePath = pByKey;
            }

            if (!choose && sequentialFill && idxSprite < sprites.Count)
            {
                choose = sprites[idxSprite++];
            }

            if (choose)
            {
                title = replaceUnderscoreWithSpace ? choose.name.Replace('_', ' ') : choose.name;
                if (string.IsNullOrEmpty(scenePath))
                {
                    if (sceneDict.TryGetValue(choose.name, out var pBySprite))
                        scenePath = pBySprite;
                }

                ApplyToCard(view, title, choose, scenePath);
                updated++;
            }
        }

        EditorUtility.DisplayDialog("完成", $"已自动覆盖 {updated} 张卡片。", "OK");
    }

    // --- helpers ---
    private static string GetCardKeyCompat(MenuCardView view)
    {
        // 1) 尝试字段 cardKey
        var f = view.GetType().GetField("cardKey", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(string))
        {
            var val = f.GetValue(view) as string;
            if (!string.IsNullOrEmpty(val)) return val;
        }
        // 2) 尝试属性 EffectiveKey
        var p = view.GetType().GetProperty("EffectiveKey", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(string))
        {
            var val = p.GetValue(view) as string;
            if (!string.IsNullOrEmpty(val)) return val;
        }
        // 3) 回退：物体名
        return view.gameObject.name;
    }

    private static IEnumerable<Sprite> LoadSprites(DefaultAsset folder)
    {
        string root = folder ? AssetDatabase.GetAssetPath(folder) : null;
        var guids = root != null ? AssetDatabase.FindAssets("t:Sprite", new[] { root })
                                 : AssetDatabase.FindAssets("t:Sprite");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s) yield return s;
        }
    }

    private static Dictionary<string, string> BuildSceneDict(DefaultAsset folder)
    {
        var dict = new Dictionary<string, string>();
        string root = folder ? AssetDatabase.GetAssetPath(folder) : null;
        var guids = root != null ? AssetDatabase.FindAssets("t:Scene", new[] { root })
                                 : AssetDatabase.FindAssets("t:Scene");
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            var n = System.IO.Path.GetFileNameWithoutExtension(p);
            if (!dict.ContainsKey(n)) dict[n] = p;
        }
        return dict;
    }

    private static void ApplyToCard(MenuCardView view, string title, Sprite sprite, string scenePath)
    {
        Undo.RecordObject(view, "Update Card");
        view.Setup(title, sprite, scenePath);

        if (view.button)
        {
            var onClick = view.button.onClick;
            for (int i = onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(onClick, i); // 注意：单数
            UnityEventTools.AddPersistentListener(onClick, view.OnClick);
            EditorUtility.SetDirty(view.button);
        }

        if (view.titleText) EditorUtility.SetDirty(view.titleText);
        if (view.thumbnail) EditorUtility.SetDirty(view.thumbnail);
        EditorUtility.SetDirty(view);
    }

    private class NaturalComparer : IComparer<string>
    {
        public int Compare(string x, string y) => EditorUtility.NaturalCompare(x, y);
    }
}
