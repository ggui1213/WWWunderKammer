using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace WWWunderkammer.Menu {
    public class SceneMenuController : MonoBehaviour {
        [System.Serializable] public class SceneItem { public string displayName; public string sceneName; public Sprite thumbnail; }

        [Header("UI 引用")]
        public Transform gridParent;
        public GameObject sceneCardPrefab;
        [Header("场景清单（Inspector 或 JSON）")]
        public List<SceneItem> scenes = new List<SceneItem>();
        public TextAsset sceneListJson;

        [Header("选项")] public bool clearOnBuild = true;

        [System.Serializable] class SceneList { public List<SceneItemDTO> items; }
        [System.Serializable] class SceneItemDTO { public string displayName; public string sceneName; }

        void Start() {
            if (sceneListJson != null && scenes.Count == 0) {
                try {
                    var list = JsonUtility.FromJson<SceneList>(sceneListJson.text);
                    if (list != null && list.items != null) {
                        foreach (var it in list.items) scenes.Add(new SceneItem { displayName = it.displayName, sceneName = it.sceneName });
                    }
                } catch {}
            }
            Build();
        }

        public void Build() {
            if (!gridParent || !sceneCardPrefab) return;
            if (clearOnBuild) for(int i=gridParent.childCount-1;i>=0;--i) Destroy(gridParent.GetChild(i).gameObject);
            foreach (var s in scenes) {
                var go = GameObject.Instantiate(sceneCardPrefab, gridParent);
                var thumb = go.transform.Find("Thumb")?.GetComponent<Image>();
                var title = go.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                var btn = go.GetComponentInChildren<Button>();
                if (thumb) thumb.sprite = s.thumbnail;
                if (title) title.text = s.displayName;
                if (btn) btn.onClick.AddListener(()=> LoadScene(s.sceneName));
            }
        }
        public void LoadScene(string sceneName){
            if (string.IsNullOrEmpty(sceneName)) return;
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}
