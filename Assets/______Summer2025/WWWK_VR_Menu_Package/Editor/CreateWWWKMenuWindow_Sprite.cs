#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace WWWK.Menu.Editor
{
    /// <summary>
    /// Drop-in replacement for CreateWWWKMenuWindow.cs that lets you pick **Sprite** assets
    /// directly in the inspector (instead of Texture2D). It still auto-applies 9-slice borders
    /// via TextureImporter on the sprite's source texture.
    /// </summary>
    public class CreateWWWKMenuWindow_Sprite : EditorWindow
    {
        [MenuItem("Tools/WWWunderkammer/Create VR Menus (Sprite Picker)")]
        public static void ShowWindow()
        {
            GetWindow<CreateWWWKMenuWindow_Sprite>("Create VR Menus");
        }

        [Header("Pick Sprites (Sliced recommended)")]
        public Sprite panelSprite;
        public Sprite buttonSprite;
        public Sprite thumbSprite;

        void OnEnable()
        {
            // Try auto-load sprites placed by the package (if present)
            TryLoadDefault(ref panelSprite, "Assets/WWWK_VR_Menu_Package/Sprites/panel_round_512.png");
            TryLoadDefault(ref buttonSprite, "Assets/WWWK_VR_Menu_Package/Sprites/button_round_384x256.png");
            TryLoadDefault(ref thumbSprite,  "Assets/WWWK_VR_Menu_Package/Sprites/thumb_round_320x180.png");

            // Ensure they import as sprites and set 9-slice borders
            EnsureSpriteSettings(panelSprite, new Vector4(56, 56, 56, 56));
            EnsureSpriteSettings(buttonSprite, new Vector4(40, 40, 40, 40));
            EnsureSpriteSettings(thumbSprite,  new Vector4(20, 20, 20, 20));
        }

        static void TryLoadDefault(ref Sprite sp, string path)
        {
            if (sp) return;
            sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void EnsureSpriteSettings(Sprite sp, Vector4 border)
        {
            if (!sp) return;
            string path = AssetDatabase.GetAssetPath(sp);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;

            bool needReimport = false;
            if (ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                needReimport = true;
            }
            // Apply border if different
            if (ti.spriteBorder != border)
            {
                ti.spriteBorder = border;
                needReimport = true;
            }
            if (ti.mipmapEnabled)
            {
                ti.mipmapEnabled = false;
                needReimport = true;
            }
            if (needReimport) ti.SaveAndReimport();
        }

        void OnGUI()
        {
            GUILayout.Label("Create WWWunderkammer VR Menus (Sprite Picker)", EditorStyles.boldLabel);

            panelSprite  = (Sprite)EditorGUILayout.ObjectField("Panel Sprite",  panelSprite,  typeof(Sprite), false);
            buttonSprite = (Sprite)EditorGUILayout.ObjectField("Button Sprite", buttonSprite, typeof(Sprite), false);
            thumbSprite  = (Sprite)EditorGUILayout.ObjectField("Thumb Sprite",  thumbSprite,  typeof(Sprite), false);

            EditorGUILayout.Space();
            if (GUILayout.Button("Create Prefabs & Add Menu Rig To Scene"))
            {
                if (!panelSprite || !buttonSprite || !thumbSprite)
                    EditorUtility.DisplayDialog("Sprites Missing", "请先选择 Panel/Button/Thumb 三个 Sprite。", "OK");
                else
                    CreatePrefabsAndRig();
            }

            EditorGUILayout.HelpBox("此版本支持直接选择 Sprite。会创建两个 Prefab（Follow/Wrist）并在场景中添加 WWWK_MenuRig。", MessageType.Info);
        }

        void CreatePrefabsAndRig()
        {
            string prefabDir = "Assets/WWWK_VR_Menu_Package/Prefabs";
            Directory.CreateDirectory(prefabDir);

            var follow = BuildFollowMenu();
            string followPath = $"{prefabDir}/WWWK_FollowMenu.prefab";
            var followPrefab = PrefabUtility.SaveAsPrefabAsset(follow, followPath);
            DestroyImmediate(follow);

            var wrist = BuildWristMenu();
            string wristPath = $"{prefabDir}/WWWK_WristMenu.prefab";
            var wristPrefab = PrefabUtility.SaveAsPrefabAsset(wrist, wristPath);
            DestroyImmediate(wrist);

            var rig = new GameObject("WWWK_MenuRig");
            var router = rig.AddComponent<WWWK.Menu.MenuInputRouter>();
            var boot = rig.AddComponent<WWWK.Menu.WWWKMenuBootstrap>();

            var followGo = (GameObject)PrefabUtility.InstantiatePrefab(followPrefab);
            var wristGo  = (GameObject)PrefabUtility.InstantiatePrefab(wristPrefab);
            followGo.transform.SetParent(rig.transform);
            wristGo.transform.SetParent(rig.transform);

            boot.input = router;
            boot.followMenu = followGo.GetComponent<WWWK.Menu.FollowFrontMenu>();
            boot.wristMenu  = wristGo.GetComponent<WWWK.Menu.WristMenuAnchor>();

            Selection.activeGameObject = rig;
            EditorGUIUtility.PingObject(rig);
        }

        GameObject BuildFollowMenu()
        {
            var root = new GameObject("WWWK_FollowMenu",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(WWWK.Menu.FollowFrontMenu));

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(1600, 900);
            root.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            var group = root.GetComponent<CanvasGroup>();
            root.GetComponent<WWWK.Menu.FollowFrontMenu>().canvasGroup = group;

            // Panel
            var panel = CreateImage("Panel", panelSprite, root.transform);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(60, 60); prt.offsetMax = new Vector2(-60, -60);

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panel.transform, false);
            var titleRT = titleGo.GetComponent<RectTransform>();
            titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f); titleRT.anchoredPosition = new Vector2(0, -40);
            var title = titleGo.GetComponent<TextMeshProUGUI>();
            title.text = "WWWunderkammer · 场景菜单"; title.fontSize = 48;
            title.alignment = TextAlignmentOptions.Center; title.outlineWidth = 0.12f;

            // Grid container (3x2)
            var grid = new GameObject("Grid", typeof(RectTransform));
            grid.transform.SetParent(panel.transform, false);
            var gridRT = grid.GetComponent<RectTransform>();
            gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridRT.sizeDelta = new Vector2(1280, 560);
            gridRT.anchoredPosition = new Vector2(0, -20);

            // Create 6 cards
            for (int i = 0; i < 6; i++)
            {
                var card = CreateCard($"Card_{i+1}", buttonSprite, thumbSprite);
                card.transform.SetParent(grid.transform, false);
                var crt = card.GetComponent<RectTransform>();
                int row = i / 3, col = i % 3;
                float cellW = 1280f / 3f, cellH = 560f / 2f;
                crt.sizeDelta = new Vector2(cellW - 24, cellH - 24);
                crt.anchoredPosition = new Vector2(-1280f / 2f + cellW * (col + 0.5f), 560f / 2f - cellH * (row + 0.5f));
            }

            var ctrl = root.AddComponent<WWWK.Menu.SceneMenuController>();
            ctrl.gridParent = grid.transform;
            ctrl.sceneCardPrefab = root.transform.Find("Panel/Grid/Card_1").gameObject;
            return root;
        }

        GameObject BuildWristMenu()
        {
            var root = new GameObject("WWWK_WristMenu",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(WWWK.Menu.WristMenuAnchor));

            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 1100);
            root.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2.5f;
            var group = root.GetComponent<CanvasGroup>();
            root.GetComponent<WWWK.Menu.WristMenuAnchor>().group = group;

            var panel = CreateImage("Panel", panelSprite, root.transform);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(30, 30); prt.offsetMax = new Vector2(-30, -30);

            var head = CreateImage("Header", buttonSprite, panel.transform);
            var hrt = head.GetComponent<RectTransform>();
            hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 1f);
            hrt.sizeDelta = new Vector2(780, 180); hrt.anchoredPosition = new Vector2(0, -80);
            var htxt = new GameObject("HeaderText", typeof(RectTransform), typeof(TextMeshProUGUI));
            htxt.transform.SetParent(head.transform, false);
            var ht = htxt.GetComponent<TextMeshProUGUI>();
            ht.text = "腕表快捷菜单"; ht.fontSize = 46; ht.alignment = TextAlignmentOptions.Center; ht.outlineWidth = 0.08f;
            htxt.GetComponent<RectTransform>().sizeDelta = new Vector2(740, 120);

            var grid = new GameObject("Grid", typeof(RectTransform));
            grid.transform.SetParent(panel.transform, false);
            var gridRT = grid.GetComponent<RectTransform>();
            gridRT.sizeDelta = new Vector2(780, 720); gridRT.anchoredPosition = new Vector2(0, -360);

            for (int i = 0; i < 4; i++)
            {
                var card = CreateCard($"Card_{i+1}", buttonSprite, thumbSprite);
                card.transform.SetParent(grid.transform, false);
                var crt = card.GetComponent<RectTransform>();
                int row = i / 2, col = i % 2;
                float cellW = 780f / 2f, cellH = 720f / 2f;
                crt.sizeDelta = new Vector2(cellW - 20, cellH - 20);
                crt.anchoredPosition = new Vector2(-780f / 2f + cellW * (col + 0.5f), 720f / 2f - cellH * (row + 0.5f));
            }

            var ctrl = root.AddComponent<WWWK.Menu.SceneMenuController>();
            ctrl.gridParent = grid.transform;
            ctrl.sceneCardPrefab = root.transform.Find("Panel/Grid/Card_1").gameObject;
            return root;
        }

        GameObject CreateImage(string name, Sprite sprite, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            if (sprite)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
            }
            return go;
        }

        GameObject CreateCard(string name, Sprite buttonSprite, Sprite thumbSprite)
        {
            var card = CreateImage(name, buttonSprite, null);
            var rt = card.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            card.AddComponent<Button>();

            var thumb = CreateImage("Thumb", thumbSprite, card.transform);
            var trt = thumb.GetComponent<RectTransform>();
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 1f);
            trt.sizeDelta = new Vector2(420, 220);
            trt.anchoredPosition = new Vector2(0, -60);

            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(card.transform, false);
            var t = title.GetComponent<TextMeshProUGUI>();
            t.text = "Room"; t.fontSize = 36; t.alignment = TextAlignmentOptions.Center; t.outlineWidth = 0.08f;
            var tr = title.GetComponent<RectTransform>();
            tr.sizeDelta = new Vector2(420, 80); tr.anchoredPosition = new Vector2(0, -150);
            return card;
        }
    }
}
#endif
