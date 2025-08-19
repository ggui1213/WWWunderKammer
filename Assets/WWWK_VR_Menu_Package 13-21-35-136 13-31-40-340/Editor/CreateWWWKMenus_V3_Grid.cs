#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using WWWK.Menu;

namespace WWWK.Menu.Editor
{
    /// <summary>
    /// V3 Grid：可调 Rows/Columns，自动生成主菜单/腕表菜单。
    /// 支持：圆形缩略图（Mask + Ring + Bottom Gradient）或矩形 16:9。
    /// 使用 GridLayoutGroup + ResponsiveGrid，避免重叠。
    /// </summary>
    public class CreateWWWKMenus_V3_Grid : EditorWindow
    {
        [MenuItem("Tools/WWWunderkammer/Create VR Menus (V3 Grid)")]
        public static void ShowWindow() => GetWindow<CreateWWWKMenus_V3_Grid>("WWWK Menus (V3 Grid)");

        // Sprites
        public Sprite panelSprite;
        public Sprite cardSprite;
        public bool   useHeaderBar = false;      // wrist 可选标题条
        public Sprite headerSprite;              // 可留空

        // Thumbnail options
        public bool useCircularThumbnails = true;
        public Sprite circleMask;               // 圆形遮罩（建议）
        public Sprite circleRing;               // 外环（可选）
        public Sprite bottomGradient;           // 渐隐（可选）
        public Sprite rectThumbSlot;            // 矩形槽（若不用圆形）

        // Text
        public string mainTitle  = "WWWunderkammer · 场景菜单";
        public string wristTitle = "腕表快捷菜单";

        // MAIN grid settings
        public int mainColumns = 3;
        public int mainRows    = 2;
        public Vector2 mainGridSize = new(1280, 560);
        public Vector2 mainSpacing  = new(18, 18);
        public Vector2 mainPadding  = new(16, 16);

        // WRIST grid settings
        public int wristColumns = 2;
        public int wristRows    = 2;
        public Vector2 wristGridSize = new(780, 720);
        public Vector2 wristSpacing  = new(16, 16);
        public Vector2 wristPadding  = new(14, 14);

        // Card inner layout
        public float circlePercent = 0.68f;
        public float circleTopInsetPercent = 0.08f;
        public float titleHeightPercent    = 0.24f;

        void OnGUI()
        {
            var so = new SerializedObject(this);
            GUILayout.Label("Sprites", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("panelSprite"));
            EditorGUILayout.PropertyField(so.FindProperty("cardSprite"));

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(so.FindProperty("useHeaderBar"));
            if (useHeaderBar) EditorGUILayout.PropertyField(so.FindProperty("headerSprite"));

            EditorGUILayout.Space(6);
            GUILayout.Label("Thumbnails", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("useCircularThumbnails"));
            if (useCircularThumbnails){
                EditorGUILayout.PropertyField(so.FindProperty("circleMask"));
                EditorGUILayout.PropertyField(so.FindProperty("circleRing"));
                EditorGUILayout.PropertyField(so.FindProperty("bottomGradient"));
            }else{
                EditorGUILayout.PropertyField(so.FindProperty("rectThumbSlot"));
            }

            EditorGUILayout.Space(6);
            GUILayout.Label("Titles", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("mainTitle"));
            EditorGUILayout.PropertyField(so.FindProperty("wristTitle"));

            EditorGUILayout.Space(6);
            GUILayout.Label("Main Grid", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("mainColumns"));
            EditorGUILayout.PropertyField(so.FindProperty("mainRows"));
            EditorGUILayout.PropertyField(so.FindProperty("mainGridSize"));
            EditorGUILayout.PropertyField(so.FindProperty("mainSpacing"));
            EditorGUILayout.PropertyField(so.FindProperty("mainPadding"));

            EditorGUILayout.Space(6);
            GUILayout.Label("Wrist Grid", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("wristColumns"));
            EditorGUILayout.PropertyField(so.FindProperty("wristRows"));
            EditorGUILayout.PropertyField(so.FindProperty("wristGridSize"));
            EditorGUILayout.PropertyField(so.FindProperty("wristSpacing"));
            EditorGUILayout.PropertyField(so.FindProperty("wristPadding"));

            EditorGUILayout.Space(6);
            GUILayout.Label("Card Inner Layout", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("circlePercent"));
            EditorGUILayout.PropertyField(so.FindProperty("circleTopInsetPercent"));
            EditorGUILayout.PropertyField(so.FindProperty("titleHeightPercent"));
            so.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create Menus In Scene", GUILayout.Height(32)))
            {
                if (!panelSprite || !cardSprite){
                    EditorUtility.DisplayDialog("Missing", "至少需要 Panel 与 Card 的 Sprite。", "OK"); return;
                }
                if (!useCircularThumbnails && !rectThumbSlot){
                    EditorUtility.DisplayDialog("Missing", "矩形方案需要 Rect Thumb Slot Sprite。", "OK"); return;
                }
                if (useCircularThumbnails && !circleMask){
                    EditorUtility.DisplayDialog("Missing", "圆形方案请至少提供 Circle Mask。", "OK"); return;
                }

                CreateMainMenu();
                CreateWristMenu();
            }

            EditorGUILayout.HelpBox("这版使用 GridLayoutGroup + ResponsiveGrid，根据行列自动算 cellSize；每张卡片内有 CardAutoLayout，圆形 Mask / 外环 / 渐隐 / 标题会随尺寸自适应。", MessageType.Info);
        }

        void CreateMainMenu()
        {
            var root = MakeCanvasGO("WWWK_FollowMenu_V3");
            var panel = CreateImage("Panel", panelSprite, root.transform, Image.Type.Sliced);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(1600, 900);

            // Title
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(panel.transform, false);
            var t = title.GetComponent<TextMeshProUGUI>();
            t.text = mainTitle; t.fontSize = 48; t.alignment = TextAlignmentOptions.Center; t.outlineWidth = 0.12f;
            var tr = title.GetComponent<RectTransform>(); tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 1f); tr.anchoredPosition = new Vector2(0, -40); tr.sizeDelta = new Vector2(1200, 100);

            // Grid
            var grid = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ResponsiveGrid));
            grid.transform.SetParent(panel.transform, false);
            var grt = grid.GetComponent<RectTransform>();
            grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.5f);
            grt.sizeDelta = mainGridSize; grt.anchoredPosition = new Vector2(0, -20);

            var gl = grid.GetComponent<GridLayoutGroup>();
            var rg = grid.GetComponent<ResponsiveGrid>();
            rg.columns = mainColumns; rg.rows = mainRows; rg.spacing = mainSpacing; rg.padding = mainPadding;

            int total = Mathf.Max(1, mainColumns * mainRows);
            for (int i = 0; i < total; i++)
            {
                var card = CreateCard($"Card_{i+1}");
                card.transform.SetParent(grid.transform, false);
            }
            rg.Apply();
            Selection.activeGameObject = root;
        }

        void CreateWristMenu()
        {
            var root = MakeCanvasGO("WWWK_WristMenu_V3");
            var panel = CreateImage("Panel", panelSprite, root.transform, Image.Type.Sliced);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(900, 1100);

            if (useHeaderBar){
                var head = CreateImage("Header", headerSprite ? headerSprite : cardSprite, panel.transform, Image.Type.Sliced);
                var hrt = head.GetComponent<RectTransform>(); hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f,1f); hrt.sizeDelta = new Vector2(780,180); hrt.anchoredPosition = new Vector2(0,-80);
                var htxt = new GameObject("HeaderText", typeof(RectTransform), typeof(TextMeshProUGUI)); htxt.transform.SetParent(head.transform,false);
                var ht = htxt.GetComponent<TextMeshProUGUI>(); ht.text = wristTitle; ht.fontSize = 46; ht.alignment = TextAlignmentOptions.Center; ht.outlineWidth = 0.08f;
                htxt.GetComponent<RectTransform>().sizeDelta = new Vector2(740,120);
            }
            else {
                var htxt = new GameObject("HeaderText", typeof(RectTransform), typeof(TextMeshProUGUI)); htxt.transform.SetParent(panel.transform,false);
                var ht = htxt.GetComponent<TextMeshProUGUI>(); ht.text = wristTitle; ht.fontSize = 44; ht.alignment = TextAlignmentOptions.Center; ht.outlineWidth = 0.08f;
                var r = htxt.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=new Vector2(0.5f,1f); r.sizeDelta=new Vector2(780,80); r.anchoredPosition=new Vector2(0,-60);
            }

            var grid = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ResponsiveGrid));
            grid.transform.SetParent(panel.transform, false);
            var grt = grid.GetComponent<RectTransform>();
            grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.5f);
            grt.sizeDelta = wristGridSize; grt.anchoredPosition = new Vector2(0, -360);

            var gl = grid.GetComponent<GridLayoutGroup>();
            var rg = grid.GetComponent<ResponsiveGrid>();
            rg.columns = wristColumns; rg.rows = wristRows; rg.spacing = wristSpacing; rg.padding = wristPadding;

            int total = Mathf.Max(1, wristColumns * wristRows);
            for (int i = 0; i < total; i++)
            {
                var card = CreateCard($"Card_{i+1}");
                card.transform.SetParent(grid.transform, false);
            }
            rg.Apply();
            Selection.activeGameObject = root;
        }

        GameObject CreateCard(string name)
        {
            var card = CreateImage(name, cardSprite, null, Image.Type.Sliced);
            var rt = card.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

            // 标题
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(card.transform, false);
            var t = title.GetComponent<TextMeshProUGUI>();
            t.text = "Room"; t.fontSize = 32; t.alignment = TextAlignmentOptions.Center; t.outlineWidth = 0.08f;

            if (useCircularThumbnails)
            {
                // 圆形 Mask
                var maskGO = CreateImage("ThumbMask", circleMask, card.transform, Image.Type.Simple);
                var m = maskGO.AddComponent<Mask>(); m.showMaskGraphic = false;

                // 缩略图（贴到 Mask 下面）
                var thumb = CreateImage("Thumb", null, maskGO.transform, Image.Type.Simple);

                // 外环（可选，放最上层）
                if (circleRing){
                    var ring = CreateImage("Ring", circleRing, card.transform, Image.Type.Simple);
                    ring.transform.SetAsLastSibling();
                }

                // 底部渐隐（可选，放在 Mask 里，这样会被裁成圆形）
                if (bottomGradient){
                    var grad = CreateImage("ThumbGradient", bottomGradient, maskGO.transform, Image.Type.Sliced);
                    var img = grad.GetComponent<Image>(); if (img){ var c = img.color; c.a = 0.95f; img.color = c; }
                }
            }
            else
            {
                // 矩形槽 + 缩略图（16:9）
                var slot = CreateImage("ThumbSlot", rectThumbSlot, card.transform, Image.Type.Sliced);
                var thumb = CreateImage("Thumb", null, slot.transform, Image.Type.Simple);
            }

            // 自适应组件（让圆直径、外环、渐隐、标题在运行/编辑时随尺寸调整）
            var auto = card.AddComponent<CardAutoLayout>();
            auto.circleDiameterPercent = circlePercent;
            auto.circleTopInsetPercent = circleTopInsetPercent;
            auto.titleHeightPercent    = titleHeightPercent;
            auto.maskRT     = card.transform.Find("ThumbMask") as RectTransform;
            auto.ringRT     = card.transform.Find("Ring") as RectTransform;
            auto.gradientRT = (card.transform.Find("ThumbGradient") as RectTransform) 
                              ?? (card.transform.Find("ThumbMask/ThumbGradient") as RectTransform);
            auto.titleRT    = card.transform.Find("Title") as RectTransform;

            return card;
        }

        GameObject MakeCanvasGO(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 2f;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1920, 1080);
            return go;
        }

        GameObject CreateImage(string name, Sprite sp, Transform parent, Image.Type type)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            if (parent) go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>(); img.sprite = sp; img.type = type;
            return go;
        }
    }
}
#endif
