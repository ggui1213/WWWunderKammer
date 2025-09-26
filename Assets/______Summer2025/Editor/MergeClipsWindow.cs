// Assets/Editor/MergeClipsWindow.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MergeClipsWindow : EditorWindow
{
    enum MergeMode { Concatenate, Overlap_Additive, Overlap_Override }

    AnimationClip clipA;
    AnimationClip clipB;
    MergeMode mode = MergeMode.Concatenate;
    float gapSeconds = 0f;      // 拼接时 A 和 B 之间的空隙
    float overlapOffset = 0f;   // 叠加时 B 相对 A 的起始偏移（可为负）
    int sampleFPS = 30;         // 叠加采样帧率
    bool loopTime = false;

    [MenuItem("Tools/Animation/Merge Clips...")]
    static void Open() => GetWindow<MergeClipsWindow>("Merge Clips");

    void OnGUI()
    {
        clipA = (AnimationClip)EditorGUILayout.ObjectField("Clip A", clipA, typeof(AnimationClip), false);
        clipB = (AnimationClip)EditorGUILayout.ObjectField("Clip B", clipB, typeof(AnimationClip), false);

        mode = (MergeMode)EditorGUILayout.EnumPopup("Mode", mode);

        if (mode == MergeMode.Concatenate)
            gapSeconds = EditorGUILayout.FloatField("Gap (seconds)", gapSeconds);
        else
        {
            overlapOffset = EditorGUILayout.FloatField("B Offset (seconds)", overlapOffset);
            sampleFPS = EditorGUILayout.IntSlider("Sample FPS", sampleFPS, 15, 120);
        }

        loopTime = EditorGUILayout.Toggle("Loop Time", loopTime);

        GUI.enabled = clipA && clipB;
        if (GUILayout.Button("Merge & Save…"))
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Merged Clip", $"{clipA.name}_{clipB.name}_Merged.anim", "anim", "Choose save path");
            if (!string.IsNullOrEmpty(path))
            {
                var merged = MergeClips(clipA, clipB, mode, gapSeconds, overlapOffset, sampleFPS, loopTime);
                AssetDatabase.CreateAsset(merged, path);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Done", "Merged clip created.", "OK");
                Selection.activeObject = merged;
            }
        }
        GUI.enabled = true;

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Concatenate：按顺序拼接。\n" +
            "Overlap_Additive：时间上重叠并对同名曲线做数值相加（适合小幅度叠加，如 Idle + Breath）。\n" +
            "Overlap_Override：时间上重叠，B 覆盖 A（同属性时以 B 为准）。\n\n" +
            "注意：对旋转的加法是按欧拉通道相加，幅度大时可能产生万向节问题；如是 Humanoid，建议在 Animator 用 Additive Layer 更稳。",
            MessageType.Info);
    }

    static AnimationClip MergeClips(AnimationClip a, AnimationClip b, MergeMode mode, float gap, float bOffset, int fps, bool loop)
    {
        var outClip = new AnimationClip();
        AnimationClipSettings(outClip, loop);

        switch (mode)
        {
            case MergeMode.Concatenate:
                Concatenate(a, b, outClip, gap);
                break;
            case MergeMode.Overlap_Additive:
                OverlapAdditive(a, b, outClip, bOffset, fps);
                break;
            case MergeMode.Overlap_Override:
                OverlapOverride(a, b, outClip, bOffset);
                break;
        }

        // 合并 Animation Events
        var evts = new List<AnimationEvent>();
        evts.AddRange(AnimationUtility.GetAnimationEvents(a));
        if (mode == MergeMode.Concatenate)
        {
            float shift = a.length + Mathf.Max(0, gap);
            foreach (var e in AnimationUtility.GetAnimationEvents(b))
            {
                var ne = new AnimationEvent();
                ne.functionName = e.functionName;
                ne.time = e.time + shift;
                ne.stringParameter = e.stringParameter;
                ne.floatParameter = e.floatParameter;
                ne.intParameter = e.intParameter;
                ne.objectReferenceParameter = e.objectReferenceParameter;
                evts.Add(ne);
            }
        }
        else
        {
            foreach (var e in AnimationUtility.GetAnimationEvents(b))
            {
                var ne = new AnimationEvent();
                ne.functionName = e.functionName;
                ne.time = e.time + bOffset;
                ne.stringParameter = e.stringParameter;
                ne.floatParameter = e.floatParameter;
                ne.intParameter = e.intParameter;
                ne.objectReferenceParameter = e.objectReferenceParameter;
                evts.Add(ne);
            }
        }
        AnimationUtility.SetAnimationEvents(outClip, evts.ToArray());

        return outClip;
    }

    static void AnimationClipSettings(AnimationClip clip, bool loop)
    {
        var so = new SerializedObject(clip);
        var settings = so.FindProperty("m_AnimationClipSettings");
        settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
        so.ApplyModifiedProperties();
    }

    static void Concatenate(AnimationClip a, AnimationClip b, AnimationClip dst, float gap)
    {
        float shift = a.length + Mathf.Max(0, gap);

        // 浮点曲线
        foreach (var bind in AnimationUtility.GetCurveBindings(a))
        {
            var curve = AnimationUtility.GetEditorCurve(a, bind);
            AnimationUtility.SetEditorCurve(dst, bind, new AnimationCurve(curve.keys));
        }
        foreach (var bind in AnimationUtility.GetCurveBindings(b))
        {
            var curve = AnimationUtility.GetEditorCurve(b, bind);
            var shifted = ShiftCurve(curve, shift);
            MergeOrAppendCurve(dst, bind, shifted);
        }

        // 资源引用曲线（Sprite/Material等）
        foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(a))
            AnimationUtility.SetObjectReferenceCurve(dst, bind, AnimationUtility.GetObjectReferenceCurve(a, bind));
        foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(b))
        {
            var keys = AnimationUtility.GetObjectReferenceCurve(b, bind);
            var shifted = ShiftObjRefKeys(keys, shift);
            MergeOrAppendObjRefCurve(dst, bind, shifted);
        }
    }

    static void OverlapOverride(AnimationClip a, AnimationClip b, AnimationClip dst, float offset)
    {
        // 先放 A，再把 B（时间偏移后）写进去（同属性直接覆盖）
        foreach (var bind in AnimationUtility.GetCurveBindings(a))
            AnimationUtility.SetEditorCurve(dst, bind, new AnimationCurve(AnimationUtility.GetEditorCurve(a, bind).keys));

        foreach (var bind in AnimationUtility.GetCurveBindings(b))
        {
            var curve = AnimationUtility.GetEditorCurve(b, bind);
            AnimationUtility.SetEditorCurve(dst, bind, ShiftCurve(curve, offset));
        }

        foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(a))
            AnimationUtility.SetObjectReferenceCurve(dst, bind, AnimationUtility.GetObjectReferenceCurve(a, bind));

        foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(b))
        {
            var keys = ShiftObjRefKeys(AnimationUtility.GetObjectReferenceCurve(b, bind), offset);
            AnimationUtility.SetObjectReferenceCurve(dst, bind, keys);
        }
    }

    static void OverlapAdditive(AnimationClip a, AnimationClip b, AnimationClip dst, float offset, int fps)
    {
        float len = Mathf.Max(a.length, b.length + offset);
        float dt = 1f / Mathf.Max(1, fps);

        // 收集所有浮点绑定
        var binds = new HashSet<EditorCurveBinding>();
        foreach (var bind in AnimationUtility.GetCurveBindings(a)) binds.Add(bind);
        foreach (var bind in AnimationUtility.GetCurveBindings(b)) binds.Add(bind);

        foreach (var bind in binds)
        {
            var ca = AnimationUtility.GetEditorCurve(a, bind);
            var cb = AnimationUtility.GetEditorCurve(b, bind);

            var curve = new AnimationCurve();
            for (float t = 0f; t <= len + 1e-6f; t += dt)
            {
                float va = ca != null ? ca.Evaluate(t) : 0f;
                float vb = cb != null ? cb.Evaluate(t - offset) : 0f; // 采样 B 时减去偏移
                curve.AddKey(new Keyframe(t, va + vb));
            }
            // 简单处理切线
            for (int i = 0; i < curve.keys.Length; i++)
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            for (int i = 0; i < curve.keys.Length; i++)
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);

            AnimationUtility.SetEditorCurve(dst, bind, curve);
        }

        // 对象引用曲线没法“加法”，用覆盖逻辑：B（偏移后）优先
        foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(a))
            AnimationUtility.SetObjectReferenceCurve(dst, bind, AnimationUtility.GetObjectReferenceCurve(a, bind));
        foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(b))
        {
            var keys = ShiftObjRefKeys(AnimationUtility.GetObjectReferenceCurve(b, bind), offset);
            AnimationUtility.SetObjectReferenceCurve(dst, bind, keys);
        }
    }

    static AnimationCurve ShiftCurve(AnimationCurve src, float shift)
    {
        var dst = new AnimationCurve();
        foreach (var k in src.keys)
            dst.AddKey(new Keyframe(k.time + shift, k.value, k.inTangent, k.outTangent));
        return dst;
    }

    static void MergeOrAppendCurve(AnimationClip clip, EditorCurveBinding bind, AnimationCurve toAdd)
    {
        var exist = AnimationUtility.GetEditorCurve(clip, bind);
        if (exist == null) { AnimationUtility.SetEditorCurve(clip, bind, toAdd); return; }
        foreach (var k in toAdd.keys) exist.AddKey(k);
        AnimationUtility.SetEditorCurve(clip, bind, exist);
    }

    static ObjectReferenceKeyframe[] ShiftObjRefKeys(ObjectReferenceKeyframe[] keys, float shift)
    {
        var arr = new ObjectReferenceKeyframe[keys.Length];
        for (int i = 0; i < keys.Length; i++)
            arr[i] = new ObjectReferenceKeyframe { time = keys[i].time + shift, value = keys[i].value };
        return arr;
    }

    static void MergeOrAppendObjRefCurve(AnimationClip clip, EditorCurveBinding bind, ObjectReferenceKeyframe[] toAdd)
    {
        var exist = AnimationUtility.GetObjectReferenceCurve(clip, bind);
        if (exist == null || exist.Length == 0) { AnimationUtility.SetObjectReferenceCurve(clip, bind, toAdd); return; }

        var list = new List<ObjectReferenceKeyframe>(exist);
        list.AddRange(toAdd);
        AnimationUtility.SetObjectReferenceCurve(clip, bind, list.ToArray());
    }
}
#endif
