using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    public int moveX = 2;
    public int moveY = 2;
    public float framesPerSecond = 10f;

    //the current frame to display
    private int index = 0;

    void Start()
    {
        var renderer = GetComponent<Renderer>();

        StartCoroutine(updateTiling());

        //set the tile size of the texture (in UV units), based on the rows and columns
        //Vector2 size = new Vector2(1f / widthX, 1f / heightY);
        //renderer.sharedMaterial.SetTextureScale("_MainTex", size);
    }

    private IEnumerator updateTiling()
    {
        var renderer = GetComponent<Renderer>();

        while (true)
        {
            //move to the next index
            index++;
            //if (index >= rows * columns)
              //  index = 0;

            //split into x and y indexes    */
            Vector2 offset = new Vector2((float)index / moveX, //x index
                                          index / moveY);          //y index
                                          
            renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);

            yield return new WaitForSeconds(1f / framesPerSecond);
        }

    }
}
