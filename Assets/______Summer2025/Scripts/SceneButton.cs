using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    [Tooltip("把要去的场景名填这里（需要先加到 Build Settings）")]
    public string sceneName;

    [Tooltip("是否异步加载（推荐）")]
    public bool loadAsync = true;

    public void Load()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneButton] sceneName 为空。");
            return;
        }

        if (loadAsync)
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}