using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Constants
    private const int MAX_PLAYER_COUNT = 3;
    private const string ROOM_NAME = "myRoom";
    public const string HMD_NICKNAME = "hmd";
    public const string DESKTOP_NICKNAME = "desktop";
    #endregion

    #region Serialized Fields
    [SerializeField] private AppDeviceType device;
    [SerializeField] private TaskManager taskManager;
    #endregion

    #region Private Fields
    public bool isRefreshing = false;
    #endregion

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

    #region Refresh Functionality
    public void RefreshConnection()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        if (taskManager != null)
        {
            // taskManager.ClearDataForRefresh();
        }

        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        else
            StartCoroutine(DelayedConnect());
    }

    private IEnumerator DelayedConnect()
    {
        yield return new WaitForSeconds(0.5f);

        PhotonNetwork.NickName = device.ToString();
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

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

    public override void OnJoinedRoom()
    {
        if (isRefreshing)
        {
            isRefreshing = false;
            if (taskManager != null)
            {
                // taskManager.OnRefreshComplete();
            }
        }
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"failed to join room: error code = {returnCode}, msg = {message}");
        if (isRefreshing)
            isRefreshing = false;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: error code = {returnCode}, msg = {message}");

        if (isRefreshing)
            isRefreshing = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isRefreshing)
        {
            Debug.Log("Disconnected during refresh - reconnecting...");
            StartCoroutine(DelayedConnect());
        }
        else if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            Debug.Log("Unexpected disconnection - attempting to reconnect...");
            PhotonNetwork.ReconnectAndRejoin();
        }
    }

    #region Unity Lifecycle

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && !PhotonNetwork.IsConnected && !isRefreshing)
            RefreshConnection();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !PhotonNetwork.IsConnected && !isRefreshing)
            RefreshConnection();
    }
    #endregion
}