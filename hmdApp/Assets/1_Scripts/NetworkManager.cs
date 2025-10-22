using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Network Settings")]
    [SerializeField] private AppDeviceType device;

    private const int MAX_PLAYER_COUNT = 3;
    private const string ROOM_NAME = "myRoom";
    public const string SMARTPHONE_NICKNAME = "smartphone";
    public const string DESKTOP_NICKNAME = "desktop";

    private void Awake()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
        PhotonNetwork.PhotonServerSettings.DevRegion = "kr";
    }
    void Start()
    {
        PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
        PhotonNetwork.NetworkingClient.AppVersion = Application.version;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = device.ToString();
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("connected");
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = MAX_PLAYER_COUNT,
            IsOpen = true,
            IsVisible = true
        };
        PhotonNetwork.JoinOrCreateRoom(ROOM_NAME, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"failed to join room: error code = {returnCode}, msg = {message}");
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.ReconnectAndRejoin();
    }

}