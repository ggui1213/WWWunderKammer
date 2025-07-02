using UnityEngine;
using UnityEditor;

public static class ReplaceWithPrefab
{
    // 验证函数：至少选中两个对象才能启用菜单
    [MenuItem("Tools/批量替换/Replace With Selected Prefab", true)]
    private static bool ValidateReplace()
    {
        return Selection.objects != null && Selection.objects.Length >= 2;
    }

    // 执行替换
    [MenuItem("Tools/批量替换/Replace With Selected Prefab")]
    private static void ExecuteReplace()
    {
        Object[] sel = Selection.objects;
        // 第一个选中的是 Prefab Asset
        GameObject prefab = sel[0] as GameObject;
        if (prefab == null || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
        {
            EditorUtility.DisplayDialog("错误",
                "请先在 Project 视图中选中一个 Prefab 作为模板，再选中要替换的场景对象。", "OK");
            return;
        }

        int count = 0;
        // 遍历剩余选中，替换为 Prefab
        for (int i = 1; i < sel.Length; i++)
        {
            GameObject orig = sel[i] as GameObject;
            if (orig == null) continue;

            // 在原有父物体下实例化 Prefab
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, orig.transform.parent);
            inst.transform.localPosition = orig.transform.localPosition;
            inst.transform.localRotation = orig.transform.localRotation;
            inst.transform.localScale    = orig.transform.localScale;
            inst.name = prefab.name;

            Undo.RegisterCreatedObjectUndo(inst, "Replace With Prefab");
            Undo.DestroyObjectImmediate(orig);
            count++;
        }

        EditorUtility.DisplayDialog("完成", $"已将 {count} 个对象替换为 Prefab：{prefab.name}", "OK");
    }
}
