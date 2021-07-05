using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class SpawnTetromino : MonoBehaviourPunCallbacks 
{
    public GameObject[] types;  // array, which contains all tetromino types prefabs
    [SerializeField]
    GameObject nextSpawn;   // gameobject - position of spawning "next figure" preview
    private Color currentColor; // color of the current tetromino
    private Color nextColor = Color.yellow;
    private Color noColor = new Color();
    private static GameObject type1 = null, type2 = null, type3 = null;  // type of the next tetromino
    private GameObject currentObject;   // link to the current tetromino
    private static GameObject[] ObjectPool = new GameObject[3];
    private static int selected = 0;
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

    public string Type
    {
        get
        {
            return currentObject.transform.name;
        }
        set
        {
            if (value == null)
            {
                type1 = null;
            }
        }
    }
    public Color CurrentColor
    {
        get
        {
            return currentColor;
        }        
    }

    
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnNew(); // if game is started - spaw new tetromino
        }
    }

    void Update()
    {
        if (nextObject != ObjectPool[selected])
        {
            Colorize(Color.white, nextObject.transform);
            nextObject = ObjectPool[selected];
            Colorize(nextColor, nextObject.transform);
        }

        if (!PhotonNetwork.IsMasterClient &&
            Input.GetKeyDown(KeyCode.A))
        {
            if (selected != 0)
            {
                selected = selected - 1;
                PhotonNetwork.RaiseEvent(42, selected, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            }
        }
        if (!PhotonNetwork.IsMasterClient && 
            Input.GetKeyDown(KeyCode.D))
        {
            if (selected != 2)
            {
                selected = selected + 1;
                PhotonNetwork.RaiseEvent(42, selected, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            }
        }

    }

    /// <summary>
    /// Spawn new Tetromino on the game screen and show which one will be next at the right of the game screen.
    /// Tetros can't use one color twice, but can have same form.
    /// </summary>
    public void SpawnNew()
    {
        GameObject selectedType = null;
        if (type1 == null)     // if game starts and type is not set
        {
            type1 = types[Random.Range(0, types.Length)];    // generete new type immediately
            selectedType = type1;            
        }
        else
        {
            if (selected == 0) selectedType = type1;
            else if (selected == 1) selectedType = type2;
            else selectedType = type3;
        }
        currentObject = PhotonNetwork.Instantiate(selectedType.name, transform.position, Quaternion.identity);     // generating new object with type from "type"

        // generating new color what is not the same to the last used color + next color        
        if (nextColor == noColor)
        {            
            int temp = Random.Range(0, colorArray.Length);
            currentColor = colorArray[temp];
        }
        else
        {
            currentColor = nextColor;
        }
        do
        {
            int temp = Random.Range(0, colorArray.Length);
            nextColor = colorArray[temp];
        }
        while (nextColor == noColor || currentColor == nextColor);

        Colorize(currentColor, currentObject.transform);
        type1 = types[Random.Range(0, types.Length)];    // generate new type for the next tetro
        type2 = types[Random.Range(0, types.Length)];
        type3 = types[Random.Range(0, types.Length)];

        for (int i = 0; i < ObjectPool.Length && ObjectPool[i] != null; i++)
        {
            PhotonNetwork.Destroy(ObjectPool[i]);
        }
        ObjectPool[0] = PhotonNetwork.Instantiate(type1.name, nextSpawn.transform.position, Quaternion.identity);
        ObjectPool[1] = PhotonNetwork.Instantiate(type2.name, nextSpawn.transform.position+new Vector3(5, 0, 0), Quaternion.identity);
        ObjectPool[2] = PhotonNetwork.Instantiate(type3.name, nextSpawn.transform.position+new Vector3(10, 0, 0), Quaternion.identity);// and spawn new one with generated type
        for (int i = 0; i < ObjectPool.Length; i++)
        {
            ObjectPool[i].GetComponent<BlockBehavior>().enabled = false;
        }
        int[] data = new int[3] {
            ObjectPool[0].GetPhotonView().ViewID,
            ObjectPool[1].GetPhotonView().ViewID,
            ObjectPool[2].GetPhotonView().ViewID
        };
        Debug.Log(data[0] + " // " +  data[1] + " // " + data[2]);
        PhotonNetwork.RaiseEvent(43, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        selected = 0;
        nextObject = ObjectPool[selected];
        Colorize(nextColor, nextObject.transform);
        
    }
 
    private void Colorize (Color color, Transform obj)
    {
        foreach (Transform child in obj)    // fill all elements of current tetro to the new generated color
        {
            child.GetComponent<SpriteRenderer>().color = color;
        }
    }
    public void OnEvent(EventData photonEvent)
    {

        Debug.Log(selected);
        switch (photonEvent.Code)
        {
            case 42:
                Debug.Log("selected changed");
                selected = (int)photonEvent.CustomData;
                break;
            case 43:
                Debug.Log("pool changed");
                int[] temp = (int[])photonEvent.CustomData;
                ObjectPool[0] = PhotonView.Find(temp[0]).gameObject;
                ObjectPool[1] = PhotonView.Find(temp[1]).gameObject;
                ObjectPool[2] = PhotonView.Find(temp[2]).gameObject;
                break;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
