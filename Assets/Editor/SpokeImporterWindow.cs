// Assets/Editor/SpokeImporterWindow.cs

using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SpokeImporterWindow : EditorWindow
{
    TextAsset spokeAsset;
    Transform parent;

    [MenuItem("Spoke/Import .spoke")]
    static void Open() => GetWindow<SpokeImporterWindow>("Spoke Importer");

    void OnGUI()
    {
        spokeAsset = (TextAsset)EditorGUILayout.ObjectField("Spoke 文件", spokeAsset, typeof(TextAsset), false);
        parent     = (Transform)EditorGUILayout.ObjectField("父节点 (可选)", parent, typeof(Transform), true);

        if (GUILayout.Button("导入"))
        {
            if (spokeAsset == null)
            {
                Debug.LogError("请先指定一个 .spoke 文本文件");
                return;
            }

            // 1. 反序列化
            var json  = spokeAsset.text;
            var scene = JsonConvert.DeserializeObject<SpokeScene>(json);

            // 2. 遍历 scene.Entities （注意大小写）
            int count = 0;
            foreach (KeyValuePair<string, SpokeEntity> kv in scene.Entities)
            {
                string uuid = kv.Key;
                SpokeEntity ent = kv.Value;
                CreateEntity(uuid, ent, parent);
                count++;
            }

            Debug.Log($"导入完成，共 {count} 个实体");
        }
    }

    void CreateEntity(string uuid, SpokeEntity ent, Transform parent)
    {
        // 1) 新建空物体
        GameObject go = new GameObject(uuid);
        if (parent != null) go.transform.SetParent(parent, false);

        // 2) 遍历所有组件
        foreach (var comp in ent.Components)
        {
            switch (comp.Name)
            {
                case "transform":
                {
                    var tp = comp.Props.ToObject<TransformProps>();
                    go.transform.localPosition    = new Vector3(tp.Position.X, tp.Position.Y, tp.Position.Z);
                    go.transform.localEulerAngles = new Vector3(tp.Rotation.X, tp.Rotation.Y, tp.Rotation.Z);
                    go.transform.localScale       = new Vector3(tp.Scale.X,    tp.Scale.Y,    tp.Scale.Z);
                    break;
                }
                case "gltf-model":
                {
                    var gm = comp.Props.ToObject<GltfModelProps>();
                    // 异步加载 glTF
                    //_ = LoadGltfAsync(gm.Src, go.transform);
                    break;
                }
                case "geometry":
                {
                    // 原生几何体
                    string prim = comp.Props["primitive"]?.ToString();
                    if (!string.IsNullOrEmpty(prim)
                        && Enum.TryParse(prim, true, out PrimitiveType pt))
                    {
                        var primGO = GameObject.CreatePrimitive(pt);
                        primGO.transform.SetParent(go.transform, false);
                    }
                    break;
                }
                case "material":
                {
                    // 着色：必须在 geometry 或 gltf-model 之后执行
                    var mp = comp.Props;
                    if (mp["color"] != null)
                    {
                        string hex = mp["color"].ToString();
                        if (ColorUtility.TryParseHtmlString(hex, out var col))
                        {
                            var rend = go.GetComponentInChildren<Renderer>();
                            if (rend != null) rend.sharedMaterial.color = col;
                        }
                    }
                    break;
                }
                case "visible":
                {
                    var vp = comp.Props.ToObject<VisibleProps>();
                    go.SetActive(vp.Visible);
                    break;
                }
                case "shadow":
                {
                    var sp = comp.Props.ToObject<ShadowProps>();
                    var rend = go.GetComponentInChildren<Renderer>();
                    if (rend != null)
                    {
                        rend.shadowCastingMode = sp.Cast ? 
                            UnityEngine.Rendering.ShadowCastingMode.On : 
                            UnityEngine.Rendering.ShadowCastingMode.Off;
                        rend.receiveShadows = sp.Receive;
                    }
                    break;
                }
                case "ambient-light":
                {
                    var ap = comp.Props.ToObject<AmbientLightProps>();
                    RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = ColorUtility.TryParseHtmlString(ap.Color, out var ac) 
                                                  ? ac : RenderSettings.ambientLight;
                    break;
                }
                case "directional-light":
                {
                    var lp = comp.Props.ToObject<DirectionalLightProps>();
                    var lightGO = new GameObject(uuid + "_DirLight");
                    lightGO.transform.SetParent(go.transform, false);
                    var light = lightGO.AddComponent<Light>();
                    light.type          = LightType.Directional;
                    light.color         = ColorUtility.TryParseHtmlString(lp.Color, out var dc) ? dc : Color.white;
                    light.intensity     = lp.Intensity;
                    light.shadows       = lp.CastShadow ? LightShadows.Soft : LightShadows.None;
                    light.shadowBias    = lp.ShadowBias;
                    light.shadowNormalBias = lp.ShadowRadius;
                    break;
                }
                case "point-light":
                {
                    var pp = comp.Props.ToObject<PointLightProps>();
                    var lightGO = new GameObject(uuid + "_PointLight");
                    lightGO.transform.SetParent(go.transform, false);
                    var light = lightGO.AddComponent<Light>();
                    light.type      = LightType.Point;
                    light.color     = ColorUtility.TryParseHtmlString(pp.Color, out var pc) ? pc : Color.white;
                    light.intensity = pp.Intensity;
                    light.range     = pp.Range;
                    light.shadows   = pp.CastShadow ? LightShadows.Soft : LightShadows.None;
                    light.shadowBias    = pp.ShadowBias;
                    light.shadowNormalBias = pp.ShadowRadius;
                    break;
                }
                case "spot-light":
                {
                    var sp = comp.Props.ToObject<SpotLightProps>();
                    var lightGO = new GameObject(uuid + "_SpotLight");
                    lightGO.transform.SetParent(go.transform, false);
                    var light = lightGO.AddComponent<Light>();
                    light.type      = LightType.Spot;
                    light.color     = ColorUtility.TryParseHtmlString(sp.Color, out var sc) ? sc : Color.white;
                    light.intensity = sp.Intensity;
                    light.range     = sp.Range;
                    light.spotAngle = sp.OuterConeAngle * Mathf.Rad2Deg;
                    light.shadows   = sp.CastShadow ? LightShadows.Soft : LightShadows.None;
                    light.shadowBias      = sp.ShadowBias;
                    light.shadowNormalBias = sp.ShadowRadius;
                    break;
                }
                case "fog":
                {
                    var fp = comp.Props.ToObject<FogProps>();
                    RenderSettings.fog         = true;
                    RenderSettings.fogColor    = ColorUtility.TryParseHtmlString(fp.Color, out var fc) ? fc : RenderSettings.fogColor;
                    if (fp.Type == "linear")
                    {
                        RenderSettings.fogMode = FogMode.Linear;
                        RenderSettings.fogStartDistance = fp.Near;
                        RenderSettings.fogEndDistance   = fp.Far;
                    }
                    else
                    {
                        RenderSettings.fogMode    = FogMode.Exponential;
                        RenderSettings.fogDensity = fp.Density;
                    }
                    break;
                }
                case "background":
                {
                    var bp = comp.Props.ToObject<BackgroundProps>();
                    if (Camera.main != null)
                        Camera.main.backgroundColor = ColorUtility.TryParseHtmlString(bp.Color, out var bc) 
                                                     ? bc : Camera.main.backgroundColor;
                    break;
                }
                case "video":
                {
                    var vp = comp.Props.ToObject<VideoProps>();
                    var vpComp = go.AddComponent<UnityEngine.Video.VideoPlayer>();
                    vpComp.url        = vp.Src;
                    vpComp.isLooping  = vp.Loop;
                    vpComp.playOnAwake= vp.AutoPlay;
                    vpComp.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
                    break;
                }
                case "image":
                {
                    var ip = comp.Props.ToObject<ImageProps>();
                    // 这里简单用 Quad 展示图片，需要你把贴图放 Resources 下
                    var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.transform.SetParent(go.transform, false);
                    var mat = quad.GetComponent<Renderer>().material;
                    mat.mainTexture = Resources.Load<Texture2D>(ip.Src);
                    break;
                }
                case "link":
                {
                    var lp = comp.Props.ToObject<LinkProps>();
                    // 可在运行时添加交互脚本，例如点击打开 URL
                    break;
                }
                case "audio-params":
                case "audio-settings":
                {
                    var ap = comp.Props.ToObject<AudioParamsProps>();
                    var src = go.AddComponent<AudioSource>();
                    src.spatialBlend   = 1f;
                    src.rolloffMode    = AudioRolloffMode.Logarithmic;
                    src.minDistance    = ap.RefDistance;
                    src.maxDistance    = ap.MaxDistance;
                    src.dopplerLevel   = ap.OverrideAudioSettings ? ap.Gain : 1f;
                    break;
                }
                case "particle-emitter":
                {
                    var pp = comp.Props.ToObject<ParticleEmitterProps>();
                    var psGO = new GameObject(uuid + "_Particles");
                    psGO.transform.SetParent(go.transform, false);
                    var ps = psGO.AddComponent<ParticleSystem>();
                    var main = ps.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(
                        ColorUtility.TryParseHtmlString(pp.StartColor, out var sc1) ? sc1 : Color.white,
                        ColorUtility.TryParseHtmlString(pp.EndColor,   out var sc2) ? sc2 : Color.white
                    );
                    main.startSize = pp.StartSize;
                    main.startLifetime = pp.Lifetime;
                    ps.Emit(pp.ParticleCount);
                    break;
                }
                case "floor-plan":
                {
                    var fp = comp.Props.ToObject<FloorPlanProps>();
                    // 如果你使用 NavMesh，可以在这里根据 fp 生成 NavMesh
                    break;
                }
                case "simple-water":
                {
                    var wp = comp.Props.ToObject<SimpleWaterProps>();
                    // 你可以在项目里准备一个 Water prefab，动态设置参数
                    break;
                }
                case "skybox":
                {
                    var spb = comp.Props.ToObject<SkyboxProps>();
                    // 假设你的场景使用 Procedural Skybox
                    var mat = RenderSettings.skybox;
                    if (mat.HasProperty("_SunSize")) mat.SetFloat("_SunSize", spb.Inclination);
                    // …根据需要映射其他属性
                    break;
                }
                case "editor-settings":
                {
                    // 编辑器专用，运行时忽略
                    break;
                }
                default:
                {
                    Debug.LogWarning($"未处理的组件：{comp.Name}");
                    break;
                }
            }
        }
}

}
