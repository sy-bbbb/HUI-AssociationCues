using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using static StudySettings;

public class StudyConfigurationManager : MonoBehaviourPunCallbacks
{
    [Header("Study Configuration")]
    [SerializeField] private int pID = 0;
    [SerializeField] private string participantID = "P0";
    [SerializeField] private StudySettings.Task currentTask = StudySettings.Task.task1;
    [SerializeField] private int blockID = 1;
    [SerializeField] private StudySettings.Condition currentCondition = StudySettings.Condition.Proximity;

    [Header("Manual Send Configuration")]
    [SerializeField] private bool sendConfigButton;

    [Header("Status Display")]
    [SerializeField] private TextMeshProUGUI statusText;

    private Player hmdPlayer;
    private bool isHMDConnected = false;
    private PhotonView pv;

    public Player HmdPlayer => hmdPlayer;

    private int[,] conditionSet = new int[4, 4]
    {
        {1,2,4,3},
        {2,3,1,4},
        {3,4,2,1},
        {4,1,3,2}
    };

    void Start()
    {
        pv = PhotonView.Get(this);
        UpdateStatusText("Waiting for HMD connection...");
        ValidateConfiguration();
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(participantID))
            Debug.LogWarning("Participant ID is empty!");

        if (blockID <= 0 || blockID > 4)
            Debug.LogWarning("Block ID should be between 1~4");
    }

    #region Photon Callbacks
    public override void OnJoinedRoom()
    {
        CheckForExistingHMD();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player entered: {newPlayer.NickName}");
        if (newPlayer.NickName == "hmd" && !isHMDConnected)
            EstablishHMDConnection(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == hmdPlayer && isHMDConnected)
            HandleHMDDisconnection();
    }

    private void CheckForExistingHMD()
    {
        Player existingHMD = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.NickName == "hmd");
        if (existingHMD != null)
            EstablishHMDConnection(existingHMD);
    }

    private void EstablishHMDConnection(Player hmdPlayerRef)
    {
        isHMDConnected = true;
        hmdPlayer = hmdPlayerRef;

        UpdateStatusText("HMD connected! Ready to send configuration.");
    }

    private void HandleHMDDisconnection()
    {
        isHMDConnected = false;
        hmdPlayer = null;

        UpdateStatusText("HMD disconnected. Waiting for reconnection...");
    }
    #endregion

    #region Configuration Management
    [ContextMenu("Send Configuration to HMD")]
    public void SendConfigurationToHMD()
    {
        if (!isHMDConnected || hmdPlayer == null)
        {
            UpdateStatusText("Error: HMD not connected!");
            return;
        }

        if (string.IsNullOrEmpty(participantID))
        {
            UpdateStatusText("Error: Participant ID cannot be empty!");
            return;
        }

        if (blockID <= 0 || blockID > 4)
        {
            UpdateStatusText("Error: Block ID must be between 1~4!");
            return;
        }

        if (pv != null)
        {
            participantID = "P" + pID.ToString();
            int conditionID = conditionSet[(pID - 1) % 4, blockID - 1] - 1;
            currentCondition = (StudySettings.Condition)conditionID;
            pv.RPC("ReceiveStudyConfiguration", hmdPlayer, participantID, (int)currentTask, blockID, (int)currentCondition);
            UpdateStatusText($"Configuration sent!<br><br> <size=20> Participant : {participantID} <br> Task : {currentTask} <br> Block : {blockID} <br> CueType : {currentCondition} </size>");
        }
        else
        {
            UpdateStatusText("Error: PhotonView not found!");
        }
    }

    [ContextMenu("Print Current Configuration")]
    public void PrintCurrentConfiguration()
    {
        Debug.Log($"Current Configuration: {GetCurrentConfigurationString()}");
    }

    public void SetConfiguration(string participantId, StudySettings.Task task, int block, StudySettings.Condition condition)
    {
        participantID = participantId;
        currentTask = task;
        blockID = block;
        currentCondition = condition;

        Debug.Log($"Configuration updated: {GetCurrentConfigurationString()}");
    }

    public string GetCurrentConfigurationString()
    {
        return $"Participant: {participantID}, Task: {currentTask}, Block: {blockID}, Condition: {currentCondition}";
    }

    public string ParticipantID => participantID;
    public StudySettings.Task CurrentTask => currentTask;
    public int BlockID => blockID;
    public StudySettings.Condition CurrentCondition => currentCondition;
    public bool IsHMDConnected => isHMDConnected;
    #endregion

    #region Utility Methods
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = $"Status: {message}";
    }

    private void OnValidate()
    {
        if (blockID < 0 || blockID > 4)
            blockID = 1;

        if (string.IsNullOrEmpty(participantID))
            participantID = "P01";

        if (sendConfigButton)
        {
            sendConfigButton = false;
            if (Application.isPlaying)
            {
                SendConfigurationToHMD();
            }
        }
    }
    #endregion
}