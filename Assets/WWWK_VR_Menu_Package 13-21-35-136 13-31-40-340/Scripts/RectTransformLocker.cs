using UnityEngine;

[ExecuteAlways]
public class RectTransformLocker : MonoBehaviour
{
    public bool enabledLock = true;
    public bool lockPosition = true;
    public bool lockSize = true;
    public bool lockRotation = true;
    public bool lockScale = true;

    [SerializeField] Vector2 _pos;
    [SerializeField] Vector2 _size;
    [SerializeField] Quaternion _rot;
    [SerializeField] Vector3 _scale;

    RectTransform _rt;

    void OnEnable(){ _rt = transform as RectTransform; Restore(); }
    void OnValidate(){ if (!isActiveAndEnabled) return; Restore(); }

    [ContextMenu("Capture Now")]
    public void Capture()
    {
        if (!_rt) _rt = transform as RectTransform;
        if (!_rt) return;
        _pos = _rt.anchoredPosition;
        _size = _rt.sizeDelta;
        _rot = _rt.localRotation;
        _scale = _rt.localScale;
    }

    void Restore()
    {
        if (!enabledLock) return;
        if (!_rt) _rt = transform as RectTransform;
        if (!_rt) return;
        if (lockPosition) _rt.anchoredPosition = _pos;
        if (lockSize) _rt.sizeDelta = _size;
        if (lockRotation) _rt.localRotation = _rot;
        if (lockScale) _rt.localScale = _scale;
    }
}
