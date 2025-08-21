using UnityEngine;
using UnityEngine.SceneManagement;

namespace ______Summer2025.Scripts
{
    public class SceneTrigger : MonoBehaviour
    {
        [Tooltip("Name of the scene to load when the player enters the trigger")]  
        public string sceneToLoad;

        private void OnTriggerEnter(Collider other)
        {
            // Check collision with the player
            if (other.CompareTag("Player"))
            {
                // Load the specified scene
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}