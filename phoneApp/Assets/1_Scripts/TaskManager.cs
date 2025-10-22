using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskManager : MonoBehaviourPunCallbacks
{
    [Header("UI Pages")]
    [SerializeField] private GameObject overviewPage;
    [SerializeField] private GameObject fullLabelPage;
    [SerializeField] private GameObject customPage;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI fullLabelText;
    [SerializeField] private Image colorTag;
    [SerializeField] private Button backButton;
    [SerializeField] private List<Button> labelThumbnails;
    [SerializeField] private List<Image> thumbnailColorTags;

    private PhotonView pv;
    private List<string> allLabelTexts = new List<string>();
    private List<string> allLabelTitles = new List<string>();
    private List<Color> allColors = new List<Color>();
    private Player hmd;
    [SerializeField] private int currentCondition = 0; // 0: Proximity, 1: Line, 2: Color, 3: Highlight

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        AssignThumbnailColorTags();
        backButton.onClick.AddListener(OnBackButtonClicked);
        PopulateAndSetupThumbnails();
        ShowInitialPage();
        FindHmdPlayer();
    }

    private void AssignThumbnailColorTags()
    {
        for (int i = 0; i < labelThumbnails.Count; i++)
        {
            Button thumbnail = labelThumbnails[i];
            Transform colorTagTransform = thumbnail.transform.Find("ColorTag");
            if (colorTagTransform == null) return;

            thumbnailColorTags.Add(colorTagTransform.GetComponentInChildren<Image>());
        }
    }

    #region PUN Callbacks
    public override void OnJoinedRoom()
    {
        CheckForExistingHMDConnection();
    }

    private void CheckForExistingHMDConnection()
    {
        Player player = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.NickName == NetworkManager.HMD_NICKNAME);
        hmd = player;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer.NickName == NetworkManager.HMD_NICKNAME && hmd == null)
            hmd = newPlayer;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == hmd)
            HandleHmdDisconnection();
    }

    private void FindHmdPlayer()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == NetworkManager.HMD_NICKNAME)
            {
                hmd = player;
                return;
            }
        }
    }
    private void HandleHmdDisconnection()
    {
        hmd = null;
        ShowInitialPage();
        currentCondition = 0;
        allLabelTexts.Clear();
        allLabelTitles.Clear();
        allColors.Clear();
    }

    #endregion

    #region UI Control Methods

    public void ShowInitialPage()
    {
        overviewPage.SetActive(false);
        fullLabelPage.SetActive(false);
        customPage.SetActive(false);
    }

    private void ShowOverviewPage()
    {
        overviewPage.SetActive(true);
        fullLabelPage.SetActive(false);
        customPage.SetActive(false);
    }

    private void ShowFullLabelPage()
    {
        overviewPage.SetActive(false);
        fullLabelPage.SetActive(true);
        customPage.SetActive(false);
    }

    private void ShowCustomPage()
    {
        overviewPage.SetActive(false);
        fullLabelPage.SetActive(false);
        customPage.SetActive(true);
    }

    private void ShowFullLabelPageWithData(int index)
    {
        if (index < 0 || index >= allLabelTexts.Count)
        {
            fullLabelText.text = "Error: Invalid Data";
            ShowFullLabelPage();
            return;
        }

        fullLabelText.text = allLabelTexts[index];

        bool showColorTag = currentCondition == 2; // Color condition
        colorTag.gameObject.SetActive(showColorTag);
        if (showColorTag && index < allColors.Count)
            colorTag.color = allColors[index];

        ShowFullLabelPage();
        StartCoroutine(RebuildLayout());
    }
    #endregion

    #region RPC Senders
    public void SendRayHoldState(bool isHeld)
    {
        if (hmd != null)
            pv.RPC("SetRayHold", hmd, isHeld);
    }

    public void SendRaySelectionRequest()
    {
        if (hmd != null)
            pv.RPC("SelectWithRay", hmd);
    }
    #endregion

    #region Private Helper Methods
    private void PopulateAndSetupThumbnails()
    {
        if (labelThumbnails == null || labelThumbnails.Count == 0)
        {
            labelThumbnails = new List<Button>();
            foreach (Transform child in overviewPage.transform)
            {
                if (child.TryGetComponent<Button>(out Button button))
                    labelThumbnails.Add(button);
            }
        }

        for (int i = 0; i < labelThumbnails.Count; i++)
        {
            int index = i;
            labelThumbnails[i].onClick.AddListener(() => OnThumbnailClicked(index));
        }
    }

    private void OnThumbnailClicked(int index)
    {
        if (currentCondition == 2 && index < allLabelTexts.Count)
        {
            Color tagColor = index < allColors.Count ? allColors[index] : Color.white;
            //ShowFullLabelPageWithData(index, tagColor, allLabelTexts[index]);
            ShowFullLabelPageWithData(index);
        }
        else
            ShowFullLabelPage();

        if (hmd != null)
            pv.RPC("RequestSelectObjectFromPhone", hmd, index);
        else
            Debug.LogWarning("Cannot send RPC: HMD player not found.");
    }

    private void OnBackButtonClicked()
    {
        ShowOverviewPage();

        if (hmd != null)
            pv.RPC("RequestDeselectFromPhone", hmd);
        else
            Debug.LogWarning("Cannot send RPC: HMD player not found.");
    }

    private IEnumerator RebuildLayout()
    {
        yield return new WaitForEndOfFrame();

        var contentRect = fullLabelText.transform.parent as RectTransform;
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            contentRect.localPosition = new Vector3(contentRect.localPosition.x, 0, contentRect.localPosition.z);
        }
    }

    private void UpdateThumbnailVisuals()
    {
        for (int i = 0; i < labelThumbnails.Count; i++)
        {
            bool hasData = i < allLabelTexts.Count;
            labelThumbnails[i].gameObject.SetActive(hasData);

            if (!hasData) continue;

            TextMeshProUGUI labelText = labelThumbnails[i].GetComponentInChildren<TextMeshProUGUI>();

            if (labelText != null)
            {
                string displayText = i < allLabelTitles.Count ? allLabelTitles[i] : allLabelTexts[i];
                labelText.text = displayText;
            }

            UpdateThumbnailColorTag(i);
        }
    }

    private void UpdateThumbnailColorTag(int index)
    {
        bool showColorTag = currentCondition == 2 && index < allColors.Count;
        thumbnailColorTags[index].gameObject.SetActive(showColorTag);

        if (showColorTag)
            thumbnailColorTags[index].color = allColors[index];
    }
    #endregion

    #region PunRPC Receivers
    [PunRPC]
    public void SyncCompletePhoneState(string[] labels, string[] titles, float[][] colorArrays, int selectedIndex, int condition)
    {
        Debug.Log("Received complete state sync from HMD.");

        allLabelTexts.Clear();
        allLabelTexts.AddRange(labels);

        allLabelTitles.Clear();
        allLabelTitles.AddRange(titles);
        allColors.Clear();

        foreach (var colorArray in colorArrays)
        {
            if (colorArray.Length >= 3)
                allColors.Add(new Color(colorArray[0], colorArray[1], colorArray[2]));
            else
                allColors.Add(Color.white);
        }

        currentCondition = condition;
        UpdateThumbnailVisuals();

        if (selectedIndex >= 0)
            ShowFullLabelPageWithData(selectedIndex);
        else
            ShowOverviewPage();
    }

    [PunRPC]
    public void ShowFullLabelDirect(int id)
    {
        ShowFullLabelPageWithData(id);
    }

    [PunRPC]
    public void ShowOverviewPageDirect()
    {
        ShowOverviewPage();
    }

    [PunRPC]
    public void ShowCustomMessage(string message)
    {
        var customText = customPage.GetComponentInChildren<TextMeshProUGUI>();
        if (customText != null)
            customText.text = message;
        ShowCustomPage();
    }

    [PunRPC]
    public void InitialiseUIOnPhone()
    {
        ShowOverviewPage();
    }

    [PunRPC]
    public void ControlLabelOnPhone(bool show, int id, float[] colorArray, string customMessage)
    {
        if (show)
        {
            if (!string.IsNullOrEmpty(customMessage))
                ShowCustomMessage(customMessage);
            else
                ShowFullLabelPageWithData(id);
        }
        else
            ShowOverviewPage();
    }
    #endregion
}
