using System; using System.Collections.Generic;
using UnityEngine; using UnityEngine.UI; using TMPro; using UnityEngine.SceneManagement;
namespace WWWK.Menu {
[Serializable] public class SceneItem{ public string displayName; public string sceneName; public Sprite thumbnail; }
public class SceneMenuController:MonoBehaviour{
    public Transform gridParent; public GameObject sceneCardPrefab; public List<SceneItem> scenes=new List<SceneItem>();
    public bool autoPopulateFromBuildIfEmpty=true;
    public void Build(){
        if(!gridParent||!sceneCardPrefab) return;
        for(int i=gridParent.childCount-1;i>=0;i--) DestroyImmediate(gridParent.GetChild(i).gameObject);
        foreach(var s in scenes){
            var go=Instantiate(sceneCardPrefab,gridParent);
            var thumb=go.transform.Find("Thumb")?.GetComponent<Image>(); var title=go.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if(thumb) thumb.sprite=s.thumbnail; if(title) title.text=string.IsNullOrEmpty(s.displayName)?s.sceneName:s.displayName;
            var btn=go.GetComponentInChildren<Button>(); if(btn) btn.onClick.AddListener(()=> LoadScene(s.sceneName));
        }}
    public void LoadScene(string n){ if(string.IsNullOrEmpty(n)) return; SceneManager.LoadSceneAsync(n,LoadSceneMode.Single); }
    [ContextMenu("从 Build Settings 自动填充")] public void AutoPopulateFromBuild(){ scenes.Clear();
        for(int i=0;i<SceneManager.sceneCountInBuildSettings;i++){ string path=SceneUtility.GetScenePathByBuildIndex(i); string name=System.IO.Path.GetFileNameWithoutExtension(path); scenes.Add(new SceneItem{displayName=name,sceneName=name}); }
        Build(); }
    void Start(){ if(autoPopulateFromBuildIfEmpty && (scenes==null||scenes.Count==0)) AutoPopulateFromBuild(); else Build(); }
}}