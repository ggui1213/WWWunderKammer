using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using GLTFast;  // 如果你用 GLTFast 加载 glTF

public class SpokeImporter : MonoBehaviour
{
    [Header("放在 StreamingAssets 下的 .spoke 文件名")]
    public string spokeFile = "Ahmed Ansari.spoke";

    [Header("实例化对象的父节点 (可留空)")]
    public Transform parent;

    void Start()
    {
        // 1. 拼出文件路径
        var path = Path.Combine(Application.streamingAssetsPath, spokeFile);
        if (!File.Exists(path))
        {
            Debug.LogError($"找不到文件：{path}");
            return;
        }

        // 2. 读取文本
        var json = File.ReadAllText(path);

        // 3. 反序列化到 SpokeScene
        var scene = JsonConvert.DeserializeObject<SpokeScene>(json);

        // 4. 遍历字典：注意用 Entities 而不是 entities
        foreach (var kv in scene.Entities)
        {
            string uuid = kv.Key;
            SpokeEntity ent = kv.Value;
            CreateEntity(uuid, ent);
        }
    }

    // 新增重载：带 uuid
    void CreateEntity(string uuid, SpokeEntity ent)
    {
        // 1) 新建空物体，名字用 uuid
        var go = new GameObject(uuid);
        go.transform.SetParent(parent, false);

        // 2) 找到 Transform 组件数据：注意用大写 Components
        var transformComp = ent.Components
            .Find(c => c.Name == "transform");
        if (transformComp != null)
        {
            var tp = transformComp.Props.ToObject<TransformProps>();
            go.transform.localPosition    = new Vector3(tp.Position.X, tp.Position.Y, tp.Position.Z);
            go.transform.localEulerAngles = new Vector3(tp.Rotation.X, tp.Rotation.Y, tp.Rotation.Z);
            go.transform.localScale       = new Vector3(tp.Scale.X,    tp.Scale.Y,    tp.Scale.Z);
        }

        // 3) glTF 模型
        var gltfComp = ent.Components
            .Find(c => c.Name == "gltf-model");
        if (gltfComp != null)
        {
            var gm = gltfComp.Props.ToObject<GltfModelProps>();
            LoadGltf(gm.Src, go.transform);
            return;
        }

        // 4) 原生几何体
        var geoComp = ent.Components
            .Find(c => c.Name == "geometry");
        if (geoComp != null)
        {
            geoComp = ent.Components.Find(c => c.Name == "geometry");
            if (geoComp != null)
            {
                // 直接从 Props 里取 primitive 字段
                string primitive = geoComp.Props["primitive"]?.ToString();
                if (!string.IsNullOrEmpty(primitive) 
                    && Enum.TryParse(primitive, true, out PrimitiveType pt))
                {
                    var prim = GameObject.CreatePrimitive(pt);
                    prim.transform.SetParent(go.transform, false);

                    // 4.1) 应用颜色材质（如果有）
                    var matComp = ent.Components.Find(c => c.Name == "material");
                    if (matComp != null && matComp.Props["color"] != null)
                    {
                        string colorStr = matComp.Props["color"].ToString();
                        if (ColorUtility.TryParseHtmlString(colorStr, out var col))
                            prim.GetComponent<Renderer>().material.color = col;
                    }
                }
            }
        }

        // 5) 你可以继续匹配其它组件（light, audio-settings…）
    }

    async void LoadGltf(string url, Transform parent)
    {
        var gltf = new GltfImport();
        if (await gltf.Load(new System.Uri(url)))
        {
            gltf.InstantiateMainScene(parent);
        }
        else
        {
            Debug.LogError($"GLTF 加载失败：{url}");
        }
    }
}
