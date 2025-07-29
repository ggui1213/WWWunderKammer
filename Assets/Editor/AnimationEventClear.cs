using UnityEditor;
using UnityEngine;

public static class AnimationEventCleaner
{
    [MenuItem("Tools/Animation/Clear Empty Events (Selected Clips)")]
    static void ClearEmptyEvents()
    {
        foreach (var clip in Selection.GetFiltered<AnimationClip>(SelectionMode.DeepAssets))
        {
            bool changed = false;
            var events = AnimationUtility.GetAnimationEvents(clip);
            for (int i = events.Length - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(events[i].functionName))
                {
                    ArrayUtility.RemoveAt(ref events, i);
                    changed = true;
                }
            }
            if (changed)
            {
                AnimationUtility.SetAnimationEvents(clip, events);
                Debug.Log($"Removed empty events from {clip.name}", clip);
            }
        }
        AssetDatabase.SaveAssets();
    }
}