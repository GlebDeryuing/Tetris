using Photon.Pun;
using Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField]
    Text playerText = null;
    private static float maxTime = 3f, timeLeft;
    private static bool isStarting = false;

    private static string connectedText = "Connected successfull. Starting in ";

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
        Debug.Log("Player's name - " + PhotonNetwork.NickName);

        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

        timeLeft = maxTime;
        isStarting = false;
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && isStarting)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                playerText.text = connectedText + Mathf.Round(timeLeft) + "...";
                PhotonNetwork.RaiseEvent(60, playerText.text, RaiseEventOptions.Default, SendOptions.SendUnreliable);

            }
            else
            {
                timeLeft = maxTime;
                isStarting = false;
                PhotonNetwork.LoadLevel("SampleScene");
            }
        }
    }

    private void Log (string message)
    {
        Debug.Log(message);
    }

  
    public override void OnConnectedToMaster()
    {
        Log("Successfully connected to Master");
        SceneManager.LoadScene(1);
    }

    public override void OnJoinedRoom()
    {
        Log("Joined room");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("WaitingRoom");
        }

        if (playerText != null) playerText.text = connectedText;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (playerText != null) playerText.text = connectedText;
            isStarting = true;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {        
        string currentScene = SceneManager.GetActiveScene().name;
        if (PhotonNetwork.IsMasterClient && currentScene == "SampleScene")
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        else
        {
            if (playerText != null) playerText.text = "Waiting for the second player...";
            isStarting = false;
            timeLeft = maxTime;
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1);
    }

    public void CreateRoom()
    {
        try
        {
            PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions
            {
                MaxPlayers = 2
            });
        }
        catch
        {
            Log("Room creating error...");
        }
    }
    public void JoinRoom()
    {
        try
        {
            PhotonNetwork.JoinRandomRoom();
        }
        catch
        {
            Log("Room joining error...");
        }
    }

    public void Leave()
    {
        try
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (PhotonNetwork.IsMasterClient&&currentScene == "SampleScene")
            {
                PhotonNetwork.RaiseEvent(61, true, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            }
            PhotonNetwork.LeaveRoom();
        }
        catch
        {
            Log("Room leaving error...");
        }
    }


    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 60:
                playerText.text = (string)photonEvent.CustomData;
                break;
            case 61:
                PhotonNetwork.LeaveRoom();
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
