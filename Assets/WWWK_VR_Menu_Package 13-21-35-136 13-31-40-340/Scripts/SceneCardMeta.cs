using UnityEngine;

namespace WWWK
{
    public class SceneCardMeta : MonoBehaviour
    {
        public string id;
        public string title;
        [TextArea(3,8)] public string description;
        public string scenePath;
        public string[] tags;
    }
}
