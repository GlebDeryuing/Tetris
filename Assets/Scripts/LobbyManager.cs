using Photon.Pun;
using Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
        Debug.Log("Player's name - " + PhotonNetwork.NickName);

        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Log (string message)
    {
        Debug.Log(message);
    }

    public override void OnConnectedToMaster()
    {
        Log("Successfully connected to Master");
    }

    public override void OnJoinedRoom()
    {
        Log("Joined room");
        PhotonNetwork.LoadLevel("SampleScene");
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
}
