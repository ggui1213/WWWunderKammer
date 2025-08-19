#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class RectTransformLockerTools : EditorWindow
{
    [MenuItem("Tools/WWWunderkammer/RectTransform Locker Tools")]
    public static void Open() => GetWindow<RectTransformLockerTools>("RT Locker");

    public bool captureAfterAdd = true;

    void OnGUI()
    {
        GUILayout.Label("对选中对象批量：添加/捕获/移除 RectTransformLocker", EditorStyles.boldLabel);

        if (GUILayout.Button("添加 Locker 到选中对象", GUILayout.Height(28)))
        {
            foreach (var go in Selection.gameObjects)
            {
                var rt = go.GetComponent<RectTransform>(); if (!rt) continue;
                var lk = go.GetComponent<RectTransformLocker>();
                if (!lk) lk = Undo.AddComponent<RectTransformLocker>(go);
                if (captureAfterAdd) lk.Capture();
                EditorUtility.SetDirty(lk);
            }
        }

        if (GUILayout.Button("对选中对象执行 Capture Now", GUILayout.Height(28)))
        {
            foreach (var go in Selection.gameObjects)
            {
                var lk = go.GetComponent<RectTransformLocker>(); if (!lk) continue;
                lk.Capture();
                EditorUtility.SetDirty(lk);
            }
        }

        if (GUILayout.Button("移除 Locker（不改现有位置）", GUILayout.Height(28)))
        {
            foreach (var go in Selection.gameObjects)
            {
                var lk = go.GetComponent<RectTransformLocker>(); if (!lk) continue;
                Undo.DestroyObjectImmediate(lk);
            }
        }
    }
}
#endif
