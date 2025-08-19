#if UNITY_EDITOR
using UnityEngine; using UnityEditor; using UnityEngine.UI; using TMPro; using System.IO;
namespace WWWK.Menu.Editor {
public class CreateWWWKMenuWindow:EditorWindow{
    [MenuItem("Tools/WWWunderkammer/Create VR Menus")] public static void ShowWindow(){ GetWindow<CreateWWWKMenuWindow>("Create VR Menus"); }
    Texture2D panelSprite, buttonSprite, thumbSprite;
    void OnEnable(){
        panelSprite = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WWWK_VR_Menu_Package/Sprites/panel_round_512.png");
        buttonSprite= AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WWWK_VR_Menu_Package/Sprites/button_round_384x256.png");
        thumbSprite = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WWWK_VR_Menu_Package/Sprites/thumb_round_320x180.png");
        EnsureSprite(panelSprite,new Vector4(56,56,56,56)); EnsureSprite(buttonSprite,new Vector4(40,40,40,40)); EnsureSprite(thumbSprite,new Vector4(20,20,20,20));
    }
    static void EnsureSprite(Texture2D tex, Vector4 border){
        if(!tex) return; string path=AssetDatabase.GetAssetPath(tex);
        var ti=(TextureImporter)AssetImporter.GetAtPath(path);
        if(ti.textureType != TextureImporterType.Sprite || ti.spriteBorder!=border){
            ti.textureType=TextureImporterType.Sprite; ti.spriteImportMode=SpriteImportMode.Single; ti.spriteBorder=border;
            ti.mipmapEnabled=false; ti.alphaIsTransparency=true; ti.SaveAndReimport();
        }}
    void OnGUI(){
        GUILayout.Label("Create WWWunderkammer VR Menus", EditorStyles.boldLabel);
        panelSprite=(Texture2D)EditorGUILayout.ObjectField("Panel Sprite", panelSprite, typeof(Texture2D), false);
        buttonSprite=(Texture2D)EditorGUILayout.ObjectField("Button Sprite", buttonSprite, typeof(Texture2D), false);
        thumbSprite=(Texture2D)EditorGUILayout.ObjectField("Thumb Sprite", thumbSprite, typeof(Texture2D), false);
        if(GUILayout.Button("Create Prefabs & Add Menu Rig To Scene")) CreatePrefabsAndRig();
        EditorGUILayout.HelpBox("创建 Follow/Wrist Prefab + WWWK_MenuRig。", MessageType.Info);
    }
    void CreatePrefabsAndRig(){
        string prefabDir="Assets/WWWK_VR_Menu_Package/Prefabs"; Directory.CreateDirectory(prefabDir);
        var follow=BuildFollowMenu(); var followPath=$"{prefabDir}/WWWK_FollowMenu.prefab"; var followPrefab=PrefabUtility.SaveAsPrefabAsset(follow, followPath); GameObject.DestroyImmediate(follow);
        var wrist=BuildWristMenu(); var wristPath=$"{prefabDir}/WWWK_WristMenu.prefab"; var wristPrefab = PrefabUtility.SaveAsPrefabAsset(wrist, wristPath); GameObject.DestroyImmediate(wrist);
        var rig=new GameObject("WWWK_MenuRig"); var router=rig.AddComponent<WWWK.Menu.MenuInputRouter>(); var boot=rig.AddComponent<WWWK.Menu.WWWKMenuBootstrap>();
        var followGo=(GameObject)PrefabUtility.InstantiatePrefab(followPrefab); var wristGo=(GameObject)PrefabUtility.InstantiatePrefab(wristPrefab);
        followGo.transform.SetParent(rig.transform); wristGo.transform.SetParent(rig.transform);
        boot.input=router; boot.followMenu=followGo.GetComponent<WWWK.Menu.FollowFrontMenu>(); boot.wristMenu=wristGo.GetComponent<WWWK.Menu.WristMenuAnchor>();
        Selection.activeGameObject=rig; EditorGUIUtility.PingObject(rig);
    }
    GameObject BuildFollowMenu(){
        var root=new GameObject("WWWK_FollowMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(WWWK.Menu.FollowFrontMenu));
        var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace; root.GetComponent<RectTransform>().sizeDelta=new Vector2(1600,900);
        root.GetComponent<CanvasScaler>().dynamicPixelsPerUnit=2f; var group=root.GetComponent<CanvasGroup>(); root.GetComponent<WWWK.Menu.FollowFrontMenu>().canvasGroup=group;
        var panel=CreateImage("Panel", panelSprite, root.transform); var prt=panel.GetComponent<RectTransform>(); prt.anchorMin=Vector2.zero; prt.anchorMax=Vector2.one; prt.offsetMin=new Vector2(60,60); prt.offsetMax=new Vector2(-60,-60);
        var titleGo=new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)); titleGo.transform.SetParent(panel.transform,false);
        var title=titleGo.GetComponent<TextMeshProUGUI>(); title.text="WWWunderkammer · 场景菜单"; title.fontSize=48; title.alignment=TextAlignmentOptions.Center; title.outlineWidth=0.12f;
        var tRT=titleGo.GetComponent<RectTransform>(); tRT.anchorMin=tRT.anchorMax=new Vector2(0.5f,1f); tRT.pivot=new Vector2(0.5f,1f); tRT.anchoredPosition=new Vector2(0,-40);
        var grid=new GameObject("Grid", typeof(RectTransform)); grid.transform.SetParent(panel.transform,false); var gRT=grid.GetComponent<RectTransform>();
        gRT.anchorMin=gRT.anchorMax=new Vector2(0.5f,0.5f); gRT.sizeDelta=new Vector2(1280,560); gRT.anchoredPosition=new Vector2(0,-20);
        for(int i=0;i<6;i++){ var card=CreateCard($"Card_{i+1}", buttonSprite, thumbSprite); card.transform.SetParent(grid.transform,false);
            var crt=card.GetComponent<RectTransform>(); int row=i/3, col=i%3; float cellW=1280f/3f, cellH=560f/2f; crt.sizeDelta=new Vector2(cellW-24, cellH-24);
            crt.anchoredPosition=new Vector2(-1280f/2f + cellW*(col+0.5f), 560f/2f - cellH*(row+0.5f)); }
        var ctrl=root.AddComponent<WWWK.Menu.SceneMenuController>(); ctrl.gridParent=grid.transform; ctrl.sceneCardPrefab=root.transform.Find("Panel/Grid/Card_1").gameObject; return root;
    }
    GameObject BuildWristMenu(){
        var root=new GameObject("WWWK_WristMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(WWWK.Menu.WristMenuAnchor));
        var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace; root.GetComponent<RectTransform>().sizeDelta=new Vector2(900,1100);
        root.GetComponent<CanvasScaler>().dynamicPixelsPerUnit=2.5f; var group=root.GetComponent<CanvasGroup>(); root.GetComponent<WWWK.Menu.WristMenuAnchor>().group=group;
        var panel=CreateImage("Panel", panelSprite, root.transform); var prt=panel.GetComponent<RectTransform>(); prt.anchorMin=Vector2.zero; prt.anchorMax=Vector2.one; prt.offsetMin=new Vector2(30,30); prt.offsetMax=new Vector2(-30,-30);
        var head=CreateImage("Header", buttonSprite, panel.transform); var hrt=head.GetComponent<RectTransform>(); hrt.anchorMin=hrt.anchorMax=new Vector2(0.5f,1f); hrt.sizeDelta=new Vector2(780,180); hrt.anchoredPosition=new Vector2(0,-80);
        var htxt=new GameObject("HeaderText", typeof(RectTransform), typeof(TextMeshProUGUI)); htxt.transform.SetParent(head.transform,false);
        var t=htxt.GetComponent<TextMeshProUGUI>(); t.text="腕表快捷菜单"; t.fontSize=46; t.alignment=TextAlignmentOptions.Center; t.outlineWidth=0.08f; htxt.GetComponent<RectTransform>().sizeDelta=new Vector2(740,120);
        var grid=new GameObject("Grid", typeof(RectTransform)); grid.transform.SetParent(panel.transform,false); var gRT=grid.GetComponent<RectTransform>(); gRT.sizeDelta=new Vector2(780,720); gRT.anchoredPosition=new Vector2(0,-360);
        for(int i=0;i<4;i++){ var card=CreateCard($"Card_{i+1}", buttonSprite, thumbSprite); card.transform.SetParent(grid.transform,false);
            var crt=card.GetComponent<RectTransform>(); int row=i/2, col=i%2; float cellW=780f/2f, cellH=720f/2f; crt.sizeDelta=new Vector2(cellW-20, cellH-20);
            crt.anchoredPosition=new Vector2(-780f/2f + cellW*(col+0.5f), 720f/2f - cellH*(row+0.5f)); }
        var ctrl=root.AddComponent<WWWK.Menu.SceneMenuController>(); ctrl.gridParent=grid.transform; ctrl.sceneCardPrefab=root.transform.Find("Panel/Grid/Card_1").gameObject; return root;
    }
    GameObject CreateImage(string name, Texture2D tex, Transform parent){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false);
        var img=go.GetComponent<Image>(); if(tex){ var path=AssetDatabase.GetAssetPath(tex); var sp=AssetDatabase.LoadAssetAtPath<Sprite>(path); if(sp){ img.sprite=sp; img.type=Image.Type.Sliced; } } return go; }
    GameObject CreateCard(string name, Texture2D button, Texture2D thumb){ var card=CreateImage(name, button, null); var btn=card.AddComponent<Button>();
        var th=CreateImage("Thumb", thumb, card.transform); var thrt=th.GetComponent<RectTransform>(); thrt.anchorMin=thrt.anchorMax=new Vector2(0.5f,1f); thrt.sizeDelta=new Vector2(420,220); thrt.anchoredPosition=new Vector2(0,-60);
        var title=new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)); title.transform.SetParent(card.transform,false);
        var t=title.GetComponent<TextMeshProUGUI>(); t.text="Room"; t.fontSize=36; t.alignment=TextAlignmentOptions.Center; t.outlineWidth=0.08f;
        title.GetComponent<RectTransform>().sizeDelta=new Vector2(420,80); title.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-150); return card; }
}}
#endif
