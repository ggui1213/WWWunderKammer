using UnityEngine;
using UnityEngine.SceneManagement;

namespace WWWunderkammer.Menu {
    [RequireComponent(typeof(Collider))]
    public class ScenePortal : MonoBehaviour {
        public string sceneName;
        public ParticleSystem glowFx;
        void Reset(){ var c = GetComponent<Collider>(); c.isTrigger = true; }
        void OnTriggerEnter(Collider other){ if (other.CompareTag("Player")) Highlight(true); }
        void OnTriggerExit(Collider other){ if (other.CompareTag("Player")) Highlight(false); }
        public void Teleport(){ if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadSceneAsync(sceneName); }
        public void Highlight(bool v){ if (glowFx){ var e = glowFx.emission; e.enabled = v; } }
    }
}
