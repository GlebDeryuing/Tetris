using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTetromino : MonoBehaviour
{
    public GameObject[] types;  // array, which contains all tetromino types prefabs
    [SerializeField]
    GameObject nextSpawn;   // gameobject - position of spawning "next figure" preview
    private Color currentColor; // color of the current tetromino
    private static GameObject type = null;  // type of the next tetromino
    private GameObject currentObject;   // link to the current tetromino
    private static GameObject nextObject;   // link to the next tetromino in the "next figure" preview
    private Color[] colorArray =    // array, which contains all color tetromino can spawn with
    {
        new Color(1f, 0.8f, 0.3f),      // yellow 
        new Color(0.9f, 0.3f, 0.1f),    // red
        new Color(0.3f, 0.5f, 0.8f),    // light-blue 
        new Color(0.3f, 0.9f, 0.3f),    // green 
        new Color(0.7f, 0.3f, 0.9f),    // purple 
        new Color(0.9f, 0.3f, 0.6f),    // pink 
        new Color(0.3f, 0.9f, 0.7f)     // cyan 
    };
    private int lastColor;  // integer value of last used color from colorArray

    public string Type
    {
        get
        {
            return currentObject.transform.name;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SpawnNew(); // if game is started - spaw new tetromino
    }

    /// <summary>
    /// Spawn new Tetromino on the game screen and show which one will be next at the right of the game screen.
    /// Tetros can't use one color twice, but can have same form.
    /// </summary>
    public void SpawnNew()
    {
        if (type==null)     // if game starts and type is not set
        {
            type = types[Random.Range(0, types.Length)];    // generete new type immediately
        }
        currentObject = Instantiate(type, transform.position, Quaternion.identity);     // generating new object with type from "type"
        // generating new color what is not the same to the last used color
        int tempColor;
        do
        {
            tempColor = Random.Range(0, colorArray.Length);
        }
        while (tempColor == lastColor);
        lastColor = tempColor;
        currentColor = colorArray[lastColor];   
        foreach (Transform child in currentObject.transform)    // fill all elements of current tetro to the new generated color
        {
            child.GetComponent<SpriteRenderer>().color = currentColor;
        }

        type = types[Random.Range(0, types.Length)];    // generate new type for the next tetro
        Destroy(nextObject);    // destroy old next tetro
        nextObject = Instantiate(type, nextSpawn.transform.position, Quaternion.identity);  // and spawn new one with generated type
        nextObject.GetComponent<BlockBehavior>().enabled = false;

    }
}
