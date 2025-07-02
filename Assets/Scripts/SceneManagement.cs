using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneManagement : MonoBehaviour
{

    public float timer = 0.0f;
    public string levelName;

    void Start()
    {
        
    }

    void Update()
    {
        timer -= Time.deltaTime;
        //Debug.Log("test " + timer);
        sceneChange();
        
    }

    void sceneChange()
    {
        //warning time for main www gallery scene
        if (timer <= 300)
        {
            //play audio file once for main gallery or silence/nothing
        }

        if (timer <= 0) {
        SceneManager.LoadScene(levelName);
    }
    
    }
  
}
