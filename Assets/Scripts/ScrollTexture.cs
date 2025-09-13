using UnityEngine;

public class URPScrollTexture_MPB : MonoBehaviour
{
    public Vector2 speed = new Vector2(0.1f, 0.0f);
    public string textureProperty = "_BaseMap"; // Shader Graph里请用它的 Reference 名
    private int _stId;

    private Renderer _r;
    private MaterialPropertyBlock _mpb;
    private Vector2 _tiling = Vector2.one;
    private Vector2 _offset = Vector2.zero;

    void Awake()
    {
        _r = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        _stId = Shader.PropertyToID(textureProperty + "_ST");

        // 读取材质当前的Tiling/Offset作为起点
        var mat = _r.sharedMaterial;
        if (mat && mat.HasProperty(textureProperty))
        {
            _tiling = mat.GetTextureScale(textureProperty);
            _offset = mat.GetTextureOffset(textureProperty);
        }
    }

    void Update()
    {
        _offset += speed * Time.deltaTime;
        _offset.x = Mathf.Repeat(_offset.x, 1f);
        _offset.y = Mathf.Repeat(_offset.y, 1f);

        _r.GetPropertyBlock(_mpb);
        // (tiling.x, tiling.y, offset.x, offset.y)
        _mpb.SetVector(_stId, new Vector4(_tiling.x, _tiling.y, _offset.x, _offset.y));
        _r.SetPropertyBlock(_mpb);
    }
}