using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerScene : MonoBehaviour
{

    //public string levelName;

    public void btn_change_scene(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }

   /* private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + "was triggered by " + other.gameObject.name);
        SceneManager.LoadScene(levelName);

    }*/
}
