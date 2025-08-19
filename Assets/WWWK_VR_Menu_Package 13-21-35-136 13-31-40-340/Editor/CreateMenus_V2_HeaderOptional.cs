#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace WWWK.Menu.Editor
{
    public enum HeaderStyle { None, SolidBar, SpriteBar, Underline }

    /// <summary>
    /// WWWunderkammer: Create VR Menus (V2, header optional)
    /// - Sprite 选择器；圆/矩形缩略图切换；Header 可选：None / SolidBar(内置) / SpriteBar(自定义) / Underline。
    /// - 生成 Follow & Wrist 两个 Prefab，并装配 MenuRig。
    /// </summary>
    public class CreateWWWKMenuWindow_V2_Headless : EditorWindow
    {
        [MenuItem("Tools/WWWunderkammer/Create VR Menus (V2, Header Optional)")]
        public static void ShowWindow(){ GetWindow<CreateWWWKMenuWindow_V2_Headless>("Create VR Menus"); }

        [Header("Base Sprites")]
        public Sprite panelSprite;
        public Sprite cardSprite;

        [Header("Header")]
        public HeaderStyle headerStyle = HeaderStyle.SolidBar;
        public Sprite headerSprite; // 仅当 SpriteBar 时使用
        public Color headerColor = new Color(0.15f,0.12f,0.33f,0.95f);
        public string titleText = "WWWunderkammer · 场景菜单";

        [Header("Rectangle Thumbnail Slot (16:9)")]
        public Sprite rectThumbSlot;

        [Header("Circle Thumbnail Parts")]
        public Sprite circleMask;
        public Sprite circleRing;
        public Sprite bottomGradient;

        [Header("Options")]
        public bool useCircularThumbnails = true;

        void OnEnable()
        {
            TryLoadDefault(ref panelSprite,  "Assets/WWWK_VR_Menu_Package/Sprites/panel_bg_round_512.png",
                                             "Assets/WWWK_VR_Menu_Package/Sprites/panel_round_512.png");
            TryLoadDefault(ref cardSprite,   "Assets/WWWK_VR_Menu_Package/Sprites/card_bg_round_384x256.png",
                                             "Assets/WWWK_VR_Menu_Package/Sprites/button_round_384x256.png");

            TryLoadDefault(ref rectThumbSlot,"Assets/WWWK_VR_Menu_Package/Sprites/thumb_slot_round_320x180.png");
            TryLoadDefault(ref circleMask,   "Assets/WWWK_UI_CircleThumb_Addon/thumb_circle_mask_512.png");
            TryLoadDefault(ref circleRing,   "Assets/WWWK_UI_CircleThumb_Addon/thumb_circle_ring_purple_512.png");
            TryLoadDefault(ref bottomGradient,"Assets/WWWK_UI_CircleThumb_Addon/thumb_bottom_gradient_512.png");

            EnsureSprite(panelSprite,  new Vector4(56,56,56,56));
            EnsureSprite(cardSprite,   new Vector4(40,40,40,40));
            EnsureSprite(rectThumbSlot,new Vector4(20,20,20,20));
        }

        static void TryLoadDefault(ref Sprite sp, params string[] paths){
            if (sp) return;
            foreach (var p in paths){ var s = AssetDatabase.LoadAssetAtPath<Sprite>(p); if (s){ sp = s; return; } }
        }

        static void EnsureSprite(Sprite sp, Vector4 border){
            if (!sp) return;
            var path = AssetDatabase.GetAssetPath(sp);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;
            bool need = false;
            if (ti.textureType != TextureImporterType.Sprite){ ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single; need = true; }
            if (ti.spriteBorder != border){ ti.spriteBorder = border; need = true; }
            if (ti.mipmapEnabled){ ti.mipmapEnabled = false; need = true; }
            if (need) ti.SaveAndReimport();
        }

        void OnGUI(){
            GUILayout.Label("Create WWWunderkammer VR Menus (V2, Header Optional)", EditorStyles.boldLabel);
            var so = new SerializedObject(this);
            EditorGUILayout.PropertyField(so.FindProperty("panelSprite"));
            EditorGUILayout.PropertyField(so.FindProperty("cardSprite"));

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(so.FindProperty("headerStyle"));
            if ((HeaderStyle)so.FindProperty("headerStyle").enumValueIndex == HeaderStyle.SpriteBar){
                EditorGUILayout.PropertyField(so.FindProperty("headerSprite"));
            }else if ((HeaderStyle)so.FindProperty("headerStyle").enumValueIndex == HeaderStyle.SolidBar){
                EditorGUILayout.PropertyField(so.FindProperty("headerColor"));
            }
            EditorGUILayout.PropertyField(so.FindProperty("titleText"));

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(so.FindProperty("useCircularThumbnails"));
            if (useCircularThumbnails){
                EditorGUILayout.PropertyField(so.FindProperty("circleMask"));
                EditorGUILayout.PropertyField(so.FindProperty("circleRing"));
                EditorGUILayout.PropertyField(so.FindProperty("bottomGradient"));
            }else{
                EditorGUILayout.PropertyField(so.FindProperty("rectThumbSlot"));
            }
            so.ApplyModifiedProperties();

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Create Prefabs & Add Menu Rig To Scene", GUILayout.Height(28)))
            {
                if (!panelSprite || !cardSprite){
                    EditorUtility.DisplayDialog("Missing", "请先选择 Panel/Card 两个基础 Sprite。", "OK");
                    return;
                }
                if (useCircularThumbnails){
                    if (!circleMask || !circleRing || !bottomGradient){
                        EditorUtility.DisplayDialog("Missing", "圆形缩略图需要：Mask / Ring / Gradient。", "OK");
                        return;
                    }
                }else if (!rectThumbSlot){
                    EditorUtility.DisplayDialog("Missing", "矩形缩略图需要：16:9 Slot Sprite。", "OK");
                    return;
                }
                CreatePrefabsAndRig();
            }

            EditorGUILayout.HelpBox("无需 Header 精灵时，选择 HeaderStyle=SolidBar（使用 Unity 内置九宫格背景并可指定颜色）、Underline（仅金色下划线）或 None（只显示标题文字）。", MessageType.Info);
        }

        void CreatePrefabsAndRig(){
            string prefabDir = "Assets/WWWK_VR_Menu_Package/Prefabs";
            Directory.CreateDirectory(prefabDir);

            var follow = BuildFollowMenu();
            var fpath = $"{prefabDir}/WWWK_FollowMenu_V2Headless.prefab";
            var fpf = PrefabUtility.SaveAsPrefabAsset(follow, fpath); DestroyImmediate(follow);

            var wrist = BuildWristMenu();
            var wpath = $"{prefabDir}/WWWK_WristMenu_V2Headless.prefab";
            var wpf = PrefabUtility.SaveAsPrefabAsset(wrist, wpath); DestroyImmediate(wrist);

            var rig = new GameObject("WWWK_MenuRig");
            var router = rig.AddComponent<WWWK.Menu.MenuInputRouter>();
            var boot = rig.AddComponent<WWWK.Menu.WWWKMenuBootstrap>();

            var followGo = (GameObject)PrefabUtility.InstantiatePrefab(fpf);
            var wristGo  = (GameObject)PrefabUtility.InstantiatePrefab(wpf);
            followGo.transform.SetParent(rig.transform);
            wristGo.transform.SetParent(rig.transform);

            boot.input = router;
            boot.followMenu = followGo.GetComponent<WWWK.Menu.FollowFrontMenu>();
            boot.wristMenu  = wristGo.GetComponent<WWWK.Menu.WristMenuAnchor>();

            Selection.activeGameObject = rig;
            EditorGUIUtility.PingObject(rig);
        }

        GameObject BuildFollowMenu(){
            var root = new GameObject("WWWK_FollowMenu_V2Headless",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(WWWK.Menu.FollowFrontMenu));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(1600,900);
            root.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            var group = root.GetComponent<CanvasGroup>(); root.GetComponent<WWWK.Menu.FollowFrontMenu>().canvasGroup = group;

            var panel = CreateImage("Panel", panelSprite, root.transform, Image.Type.Sliced);
            var prt = panel.GetComponent<RectTransform>(); prt.anchorMin=Vector2.zero; prt.anchorMax=Vector2.one; prt.offsetMin=new Vector2(60,60); prt.offsetMax=new Vector2(-60,-60);

            CreateHeader(panel.transform, "Title", titleText, 48);

            var grid = new GameObject("Grid", typeof(RectTransform)); grid.transform.SetParent(panel.transform,false);
            var gRT = grid.GetComponent<RectTransform>(); gRT.anchorMin=gRT.anchorMax=new Vector2(0.5f,0.5f); gRT.sizeDelta=new Vector2(1280,560); gRT.anchoredPosition=new Vector2(0,-20);

            for (int i=0;i<6;i++){
                var card = CreateCard($"Card_{i+1}"); card.transform.SetParent(grid.transform,false);
                var crt = card.GetComponent<RectTransform>();
                int row=i/3, col=i%3; float cellW=1280f/3f, cellH=560f/2f;
                crt.sizeDelta = new Vector2(cellW-24, cellH-24);
                crt.anchoredPosition = new Vector2(-1280f/2f + cellW*(col+0.5f), 560f/2f - cellH*(row+0.5f));
            }

            var ctrl = root.AddComponent<WWWK.Menu.SceneMenuController>();
            ctrl.gridParent = grid.transform;
            ctrl.sceneCardPrefab = root.transform.Find("Panel/Grid/Card_1").gameObject;
            return root;
        }

        GameObject BuildWristMenu(){
            var root = new GameObject("WWWK_WristMenu_V2Headless",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(WWWK.Menu.WristMenuAnchor));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(900,1100);
            root.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2.5f;
            var group = root.GetComponent<CanvasGroup>(); root.GetComponent<WWWK.Menu.WristMenuAnchor>().group = group;

            var panel = CreateImage("Panel", panelSprite, root.transform, Image.Type.Sliced);
            var prt = panel.GetComponent<RectTransform>(); prt.anchorMin=Vector2.zero; prt.anchorMax=Vector2.one; prt.offsetMin=new Vector2(30,30); prt.offsetMax=new Vector2(-30,-30);

            CreateHeader(panel.transform, "Header", "腕表快捷菜单", 46, width:780, yOffset:-80);

            var grid = new GameObject("Grid", typeof(RectTransform)); grid.transform.SetParent(panel.transform,false);
            var gRT = grid.GetComponent<RectTransform>(); gRT.sizeDelta=new Vector2(780,720); gRT.anchoredPosition=new Vector2(0,-360);

            for (int i=0;i<4;i++){
                var card = CreateCard($"Card_{i+1}"); card.transform.SetParent(grid.transform,false);
                var crt = card.GetComponent<RectTransform>(); int row=i/2, col=i%2; float cellW=780f/2f, cellH=720f/2f;
                crt.sizeDelta=new Vector2(cellW-20, cellH-20);
                crt.anchoredPosition=new Vector2(-780f/2f + cellW*(col+0.5f), 720f/2f - cellH*(row+0.5f));
            }

            var ctrl = root.AddComponent<WWWK.Menu.SceneMenuController>();
            ctrl.gridParent = grid.transform;
            ctrl.sceneCardPrefab = root.transform.Find("Panel/Grid/Card_1").gameObject;
            return root;
        }

        void CreateHeader(Transform parent, string name, string text, float fontSize, float width=0f, float yOffset=-40f){
            var anchor = new GameObject(name, typeof(RectTransform));
            anchor.transform.SetParent(parent,false);
            var art = anchor.GetComponent<RectTransform>();
            art.anchorMin=art.anchorMax=new Vector2(0.5f,1f); art.pivot=new Vector2(0.5f,1f);
            art.anchoredPosition=new Vector2(0,yOffset);
            art.sizeDelta = new Vector2(width>0?width:0, 0);

            if (headerStyle == HeaderStyle.SpriteBar){
                var bar = CreateImage("HeaderBar", headerSprite, anchor.transform, Image.Type.Sliced);
                var brt = bar.GetComponent<RectTransform>(); brt.sizeDelta = new Vector2(width>0?width:1280, 160);
            }else if (headerStyle == HeaderStyle.SolidBar){
                var builtin = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                var bar = CreateImage("HeaderBar", builtin, anchor.transform, Image.Type.Sliced);
                var brt = bar.GetComponent<RectTransform>(); brt.sizeDelta = new Vector2(width>0?width:1280, 160);
                var img = bar.GetComponent<Image>(); img.color = headerColor;
            }else if (headerStyle == HeaderStyle.Underline){
                var underline = new GameObject("Underline", typeof(RectTransform), typeof(Image));
                underline.transform.SetParent(anchor.transform,false);
                var urt = underline.GetComponent<RectTransform>(); urt.sizeDelta = new Vector2(width>0?width:1000, 4); urt.anchoredPosition = new Vector2(0,-40);
                var ui = underline.GetComponent<Image>(); ui.color = new Color(1f,0.84f,0.25f,1f); // gold line
            }
            var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            tgo.transform.SetParent(anchor.transform,false);
            var t = tgo.GetComponent<TextMeshProUGUI>(); t.text=text; t.fontSize=fontSize; t.alignment=TextAlignmentOptions.Center; t.outlineWidth=0.08f;
            tgo.GetComponent<RectTransform>().sizeDelta = new Vector2(width>0?width:1200, fontSize*2f);
        }

        GameObject CreateCard(string name){
            var card = CreateImage(name, cardSprite, null, Image.Type.Sliced);
            var rt = card.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=new Vector2(0.5f,0.5f);
            card.AddComponent<Button>();

            if (useCircularThumbnails){
                var maskGO = CreateImage("ThumbMask", circleMask, card.transform, Image.Type.Simple);
                var mrt = maskGO.GetComponent<RectTransform>(); mrt.anchorMin=mrt.anchorMax=new Vector2(0.5f,1f); mrt.sizeDelta=new Vector2(420,420); mrt.anchoredPosition=new Vector2(0,-40);
                var mask = maskGO.AddComponent<Mask>(); mask.showMaskGraphic=false;

                var thumb = CreateImage("Thumb", null, maskGO.transform, Image.Type.Simple);
                var thrt = thumb.GetComponent<RectTransform>(); thrt.anchorMin=thrt.anchorMax=new Vector2(0.5f,0.5f); thrt.sizeDelta=new Vector2(380,380);

                var ring = CreateImage("Ring", circleRing, card.transform, Image.Type.Simple);
                var rrt = ring.GetComponent<RectTransform>(); rrt.anchorMin=rrt.anchorMax=new Vector2(0.5f,1f); rrt.sizeDelta=new Vector2(420,420); rrt.anchoredPosition=new Vector2(0,-40);

                var grad = CreateImage("ThumbGradient", bottomGradient, card.transform, Image.Type.Sliced);
                var grt = grad.GetComponent<RectTransform>(); grt.anchorMin=grt.anchorMax=new Vector2(0.5f,1f); grt.sizeDelta=new Vector2(400,260); grt.anchoredPosition=new Vector2(0,-60);
                var gradImg = grad.GetComponent<Image>();
                if (gradImg) gradImg.color = new Color(1f, 1f, 1f, 0.95f);
            }else{
                var slot = CreateImage("ThumbSlot", rectThumbSlot, card.transform, Image.Type.Sliced);
                var srt = slot.GetComponent<RectTransform>(); srt.anchorMin=srt.anchorMax=new Vector2(0.5f,1f); srt.sizeDelta=new Vector2(420,220); srt.anchoredPosition=new Vector2(0,-60);

                var thumb = CreateImage("Thumb", null, slot.transform, Image.Type.Simple);
                var thrt = thumb.GetComponent<RectTransform>(); thrt.anchorMin=thrt.anchorMax=new Vector2(0.5f,0.5f); thrt.sizeDelta=new Vector2(404,204);
            }

            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(card.transform,false);
            var t = title.GetComponent<TextMeshProUGUI>(); t.text="Room"; t.fontSize=36; t.alignment=TextAlignmentOptions.Center; t.outlineWidth=0.08f;
            var tr = title.GetComponent<RectTransform>(); tr.sizeDelta=new Vector2(420,80); tr.anchoredPosition=new Vector2(0,-150);
            return card;
        }

        GameObject CreateImage(string name, Sprite sp, Transform parent, Image.Type type){
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            if (parent) go.transform.SetParent(parent,false);
            var img = go.GetComponent<Image>(); img.sprite = sp; img.type = type;
            return go;
        }
    }
}
#endif
