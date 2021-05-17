using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTetromino : MonoBehaviour
{
    public GameObject[] types;
    private Color currentColor;
    private GameObject currentObject;
    private Color[] colorArray =
    {
        new Color(1f, 0.8f, 0.3f), // yellow 
        new Color(0.9f, 0.3f, 0.1f), // red
        new Color(0.3f, 0.5f, 0.8f), // light-blue 
        new Color(0.3f, 0.9f, 0.3f), // green 
        new Color(0.7f, 0.3f, 0.9f), // purple 
        new Color(0.9f, 0.3f, 0.6f), // pink 
        new Color(0.3f, 0.9f, 0.7f) // cyan 
    };

    // Start is called before the first frame update
    void Start()
    {
        SpawnNew();
    }

    public void SpawnNew()
    {
        currentObject = Instantiate(types[Random.Range(0, types.Length)], transform.position, Quaternion.identity);
        currentColor = colorArray[Random.Range(0, colorArray.Length)];
        foreach (Transform child in currentObject.transform)
        {
           child.GetComponent<Renderer>().material.SetColor("_Color", currentColor);
        }
    }
}
