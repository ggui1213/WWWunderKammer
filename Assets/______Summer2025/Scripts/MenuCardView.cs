using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuCardView : MonoBehaviour
{
    [Header("指向 UI 子结点")]
    public Image thumbnail;     // 例如 ThumbMask/Thumb 上的 Image
    public TMP_Text titleText;  // 例如 Title (TMP)
    public Button button;       // 卡片上的 Button

    [Header("可选：用于映射的唯一键（不填则用 GameObject 名）")]
    public string cardKey;

    [SerializeField] private string scenePath; // Assets/.../YourScene.unity

    public string EffectiveKey => string.IsNullOrEmpty(cardKey) ? gameObject.name : cardKey;

    public void Setup(string title, Sprite sprite, string sceneAssetPath)
    {
        if (titleText) titleText.text = title ?? titleText.text;
        if (thumbnail) thumbnail.sprite = sprite ?? thumbnail.sprite;
        scenePath = sceneAssetPath ?? scenePath;
    }

    public void OnClick()
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogWarning($"[MenuCardView] 未指定场景路径：{name}");
            return;
        }

        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            return;
        }
#endif
        SceneManager.LoadScene(sceneName);
    }
}