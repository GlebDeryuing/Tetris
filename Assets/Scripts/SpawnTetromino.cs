using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTetromino : MonoBehaviour
{
    public GameObject[] types;
    [SerializeField]
    GameObject nextSpawn;
    private Color currentColor;
    private static GameObject type = null;
    private GameObject currentObject;
    private static GameObject nextObject;
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
    private int lastColor;

    // Start is called before the first frame update
    void Start()
    {
        SpawnNew();
    }

    public void SpawnNew()
    {
        if (type==null)
        {
            type = types[Random.Range(0, types.Length)];
        }
        currentObject = Instantiate(type, transform.position, Quaternion.identity);
        int tempColor;
        do
        {
            tempColor = Random.Range(0, colorArray.Length);
        }
        while (tempColor == lastColor);
        lastColor = tempColor;
        currentColor = colorArray[lastColor];
        foreach (Transform child in currentObject.transform)
        {
           child.GetComponent<Renderer>().material.SetColor("_Color", currentColor);
        }

        type = types[Random.Range(0, types.Length)];
        Debug.Log(type);
        Destroy(nextObject);
        nextObject = Instantiate(type, nextSpawn.transform.position, Quaternion.identity);
        nextObject.GetComponent<BlockBehavior>().enabled = false;

    }
}
