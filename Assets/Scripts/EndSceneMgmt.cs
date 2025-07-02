using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneMgmt : MonoBehaviour
{

    public float timer = 0.0f;
    //public string levelName;
    private Scene currentScene;
    string sceneName;
    public static bool galaxyVisited = false;
    public static bool matrixVisited = false;

    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
        Debug.Log("Active Scene is '" + currentScene.name + "'.");

    }

    void Update()
    {
        timer -= Time.deltaTime;
        endScene();
        sceneCheck();
    }

    void sceneCheck()
        //keeping check of variable to conjure scene flow
    {
        if (sceneName == "04_Galaxy")
        {
            Debug.Log("we in this galazy bitch owo galaxyvisited is " + galaxyVisited + ".");
            galaxyVisited = true;
            Debug.Log("matrixVisited is " + matrixVisited);

        }
        else if (sceneName == "05_Mat4rix")
        {
            Debug.Log("we in thE FUCKING MATRIX, matrixVisited is " + matrixVisited + ".");
            matrixVisited = true;
            Debug.Log("matrixVisited is " + matrixVisited);

        }
    }

    void sceneChange()
        //decides when things change based on variables
    {
        if (matrixVisited == true)
        {
            Debug.Log("matrixVisited is " + matrixVisited);
            if (timer <= 0)
            {
                //Debug.Log("change scene now");
                SceneManager.LoadScene("04_Galaxy");
            }
        }

        if ((timer <= 0) && (galaxyVisited == true))
        {
            if (timer <= 0)
            {
               /// Debug.Log("change scene now");
                SceneManager.LoadScene("05_Mat4rix");
            }
        }

       


    }

    void endScene()
        //only triggers after the others are done 
    {
        if ((galaxyVisited == true) && (matrixVisited == true))
        {
            Debug.Log("Everything is true! matrix is " + matrixVisited + "and galaxy is " + galaxyVisited);

            if (timer <= 0)
            {
                Debug.Log("change scene now");
                SceneManager.LoadScene("07_GrRealityEnd");
            }
        }
        else
        {
            sceneChange();
        }
    }


}