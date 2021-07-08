using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Threading;
using UnityEngine.UI;

public class SpawnTetromino : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public GameObject[] types;  // array, which contains all tetromino types prefabs
    [SerializeField]
    GameObject nextSpawn;   // gameobject - position of spawning "next figure" preview
    [SerializeField]
    Text timerField;
    private Color currentColor; // color of the current tetromino
    private Color nextColor;
    private Color noColor = new Color();
    private int nextColorIndex = 0, currentColorIndex = 0;
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
    private static float maxTime = 3f, timeLeft;
    private static bool wait = false,
        move = true;


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
        timerField.text = "";
        PhotonNetwork.RaiseEvent(40, timerField.text, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        if (PhotonNetwork.IsMasterClient)
        {
            Begin();
        }
    }

    public void Begin()
    {
        ChangeAvalible(true);
        GenerateNext(); // if game is started - spaw new tetromino
        WaitStart();
    }

    private void WaitStart()
    {
        timeLeft = maxTime;
        wait = true;
    }

    public void ChangeAvalible(bool canMove)
    {
        move = canMove;
    }

    void Update()
    {
        if (wait)
        {
            if (timeLeft > 0)
            {
                timerField.text = Mathf.Round(timeLeft)+"...";                
                PhotonNetwork.RaiseEvent(40, timerField.text, RaiseEventOptions.Default, SendOptions.SendUnreliable);
                timeLeft -= Time.deltaTime;

            }
            else
            {
                timerField.text = "";
                PhotonNetwork.RaiseEvent(40, timerField.text, RaiseEventOptions.Default, SendOptions.SendUnreliable);
                timeLeft = maxTime;
                wait = false;
                SpawnNew();
            }
        }

        if (nextObject != ObjectPool[selected])
        {
            Colorize(Color.white, nextObject.transform);
            nextObject = ObjectPool[selected];
            Colorize(nextColor, nextObject.transform);
        }

        if (!PhotonNetwork.IsMasterClient &&
            move &&
            Input.GetKeyDown(KeyCode.A))
        {
            if (selected != 0)
            {
                selected = selected - 1;
                PhotonNetwork.RaiseEvent(42, selected, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            }
        }
        if (!PhotonNetwork.IsMasterClient &&
            move &&
            Input.GetKeyDown(KeyCode.D))
        {
            if (selected != 2)
            {
                selected = selected + 1;
                PhotonNetwork.RaiseEvent(42, selected, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            }
        }

    }

    public void GenerateNext()
    {
        do
        {
            int temp = Random.Range(0, colorArray.Length);
            nextColor = colorArray[temp];
            nextColorIndex = temp;
        }
        while (nextColor == noColor || currentColor == nextColor);

        type1 = types[Random.Range(0, types.Length)];    // generate new type for the next tetro
        do
        {
            type2 = types[Random.Range(0, types.Length)];
        }
        while (type2 == type1);
        do
        {
            type3 = types[Random.Range(0, types.Length)];
        }
        while (type3 == type1 || type3 == type2);

        for (int i = 0; i < ObjectPool.Length && ObjectPool[i] != null; i++)
        {
            PhotonNetwork.Destroy(ObjectPool[i]);
        }
        ObjectPool[0] = PhotonNetwork.Instantiate(type1.name, nextSpawn.transform.position, Quaternion.identity);
        ObjectPool[1] = PhotonNetwork.Instantiate(type2.name, nextSpawn.transform.position + new Vector3(5, 0, 0), Quaternion.identity);
        ObjectPool[2] = PhotonNetwork.Instantiate(type3.name, nextSpawn.transform.position + new Vector3(10, 0, 0), Quaternion.identity);// and spawn new one with generated type
        for (int i = 0; i < ObjectPool.Length; i++)
        {
            ObjectPool[i].GetComponent<BlockBehavior>().enabled = false;
        }
        int[] data = new int[4] {
            ObjectPool[0].GetPhotonView().ViewID,
            ObjectPool[1].GetPhotonView().ViewID,
            ObjectPool[2].GetPhotonView().ViewID,
            nextColorIndex
        };
        PhotonNetwork.RaiseEvent(43, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        selected = 0;
        nextObject = ObjectPool[selected];
        Colorize(nextColor, nextObject.transform);
    }
    /// <summary>
    /// Spawn new Tetromino on the game screen and show which one will be next at the right of the game screen.
    /// Tetros can't use one color twice, but can have same form.
    /// </summary>
    public void SpawnNew()
    {
        GameObject selectedType = null;
        if (selected == 0) selectedType = type1;
        else if (selected == 1) selectedType = type2;
        else selectedType = type3;
        currentObject = PhotonNetwork.Instantiate(selectedType.name, transform.position, Quaternion.identity);     // generating new object with type from "type"
                                                                                                                   // generating new color what is not the same to the last used color + next color        
        currentColor = nextColor;
        currentColorIndex = nextColorIndex;
        Colorize(currentColor, currentObject.transform);
        int[] colorData = new int[1] { currentColorIndex };
        PhotonNetwork.RaiseEvent(44, colorData, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        GenerateNext();
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
        switch (photonEvent.Code)
        {
            case 40:
                timerField.text = (string)photonEvent.CustomData;
                break;
            case 42:
                selected = (int)photonEvent.CustomData;
                break;
            case 43:
                int[] temp = (int[])photonEvent.CustomData;
                ObjectPool[0] = PhotonView.Find(temp[0]).gameObject;
                ObjectPool[1] = PhotonView.Find(temp[1]).gameObject;
                ObjectPool[2] = PhotonView.Find(temp[2]).gameObject;
                nextColor = colorArray[temp[3]];
                selected = 0;
                nextObject = ObjectPool[selected];
                Colorize(nextColor, nextObject.transform);
                break;
            case 44:
                Transform blockActive = FindObjectOfType<BlockBehavior>().transform;
                int[] colorData = (int[])photonEvent.CustomData;
                currentColor = colorArray[colorData[0]];
                Colorize(currentColor, blockActive);
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
