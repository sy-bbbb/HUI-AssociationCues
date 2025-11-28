using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class ConditionManager : MonoBehaviourPunCallbacks
{
    [Header("Network Settings")]
    [SerializeField] private NetworkManager networkManager;

    [Header("Component References")]
    [SerializeField] private Transform phoneObject;
    [SerializeField] private PhoneLabelHandler phoneLabelHandler;

    [Header("Interaction Settings")]
    [SerializeField] private float rayLength = 10f;
    [SerializeField] private float proximityThreshold = 0.25f;
    [SerializeField] private LayerMask objectLayer;
    [SerializeField] private LayerMask buttonLayer;

    [Header("Visuals")]
    [SerializeField] private Material connectionLineMaterial;
    [SerializeField] private Material baseOutlineMaterial;
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private TextMeshPro conditionLabel;

    // State Management
    private TaskManager taskManager;
    private QuizManager quizManager;
    private PhotonView pv;
    private Player smartphone;
    private StudySettings.Condition currentCondition;
    private readonly List<GameObject> sceneObjects = new List<GameObject>();
    private readonly Dictionary<GameObject, Color> objectColors = new Dictionary<GameObject, Color>();

    // Selection State
    private GameObject selectedObject;
    private GameObject lastHoveredButton;

    //Interaction State
    private bool isConnectedToPhone = false;
    private bool isPointing = false;
    private bool selectActionTriggered = false;

    //Visual Components
    private LineRenderer connectionLine;
    private LineRenderer pointingRayLine;

    private string[] cachedLabelTexts;
    private string[] cachedLabelTitles;

    public Player Smartphone => smartphone;


    private void Start()
    {
        InitialiseComponents();
        InitialisePointingRay();
    }

    private void Update()
    {
        HandleRaySelection();
        UpdateVisualCues();
    }

    private void InitialiseComponents()
    {
        taskManager = GetComponent<TaskManager>();
        quizManager = GetComponent<QuizManager>();
        pv = PhotonView.Get(this);
    }

    public void Initialise(StudySettings.Condition condition, StudySettings.Task task, int block, List<GameObject> allSceneObjects)
    {
        currentCondition = condition;
        sceneObjects.Clear();
        sceneObjects.AddRange(allSceneObjects);

        cachedLabelTexts = phoneLabelHandler.LabelContents.ToArray();
        cachedLabelTitles = phoneLabelHandler.LabelTitles.ToArray();

        ConfigureCondition();
        phoneLabelHandler.InitializePhoneUI(sceneObjects, objectColors, currentCondition);

        if (smartphone != null)
            SyncCompleteStateToPhone();

        conditionLabel.transform.parent.gameObject.SetActive(true);
        UpdateConditionLabel(condition);
    }

    public void InitialisePractice(StudySettings.Condition condition, StudySettings.Task task, int block, List<GameObject> allSceneObjects)
    {
        Initialise(condition, task, block, allSceneObjects);
    }
    public void SwitchCondition(StudySettings.Condition newCondition)
    {
        CleanupCurrentCondition();
        if (selectedObject != null)
            DeselectObject();

        currentCondition = newCondition;
        objectColors.Clear();
        ConfigureCondition();

        phoneLabelHandler.InitializePhoneUI(sceneObjects, objectColors, currentCondition);

        if (smartphone != null)
            SyncCompleteStateToPhone();

        UpdateConditionLabel(newCondition);
    }

    private void UpdateConditionLabel(StudySettings.Condition condition)
    {
        if (conditionLabel == null) return;

        switch (condition)
        {
            case StudySettings.Condition.Proximity:
                conditionLabel.text = "근접 모드";
                break;
            case StudySettings.Condition.Line:
                conditionLabel.text = "선 모드";
                break;
            case StudySettings.Condition.Color:
                conditionLabel.text = "색상 모드";
                break;
            case StudySettings.Condition.Highlight:
                conditionLabel.text = "하이라이트 모드";
                break;
            default:
                conditionLabel.text = condition.ToString();
                break;
        }
    }
    private void CleanupCurrentCondition()
    {
        switch (currentCondition)
        {
            case StudySettings.Condition.Color:
                CleanupColorCondition();
                break;
            case StudySettings.Condition.Line:
                CleanupLineCondition();
                break;
            case StudySettings.Condition.Highlight:
                CleanupHighlightCondition();
                break;
        }
    }

    private void CleanupColorCondition()
    {
        foreach (var obj in sceneObjects)
        {
            var outline = obj.GetComponent<MeshOutlineHierarchy>();
            if (outline != null)
            {
                if (outline.OutlineMaterial != null)
                    outline.OutlineMaterial.color = highlightColor; // Reset to white
                outline.enabled = false;
                var meshOutlines = obj.GetComponentsInChildren<MeshOutline>();
                foreach (var meshOutline in meshOutlines)
                {
                    if (meshOutline != null)
                        meshOutline.enabled = false;
                }
            }
        }
    }

    private void CleanupLineCondition()
    {
        if (connectionLine != null)
            connectionLine.enabled = false;
    }

    private void CleanupHighlightCondition()
    {
        foreach (var obj in sceneObjects)
        {
            var outline = obj.GetComponent<MeshOutlineHierarchy>();
            if (outline != null)
            {
                var outlines = obj.GetComponentsInChildren<MeshOutline>();
                foreach (var meshOutline in outlines)
                {
                    meshOutline.enabled = false;
                }
            }
        }
    }

    private void ConfigureCondition()
    {
        switch (currentCondition)
        {
            case StudySettings.Condition.Color:
                AssignUniqueColorsToObjects();
                break;
            case StudySettings.Condition.Line:
                InitialiseLineCue();
                break;
        }
    }

    #region Connection Management

    public override void OnJoinedRoom()
    {
        CheckForExistingPhoneConnection();
    }
    private void CheckForExistingPhoneConnection()
    {
        Player existingPhone = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.NickName == NetworkManager.SMARTPHONE_NICKNAME);
        if (existingPhone != null)
            EstablishPhoneConnection(existingPhone);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer.NickName == NetworkManager.SMARTPHONE_NICKNAME && !isConnectedToPhone)
            EstablishPhoneConnection(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == smartphone && isConnectedToPhone)
            HandlePhoneDisconnection();
    }

    private void EstablishPhoneConnection(Player phonePlayer)
    {
        isConnectedToPhone = true;
        smartphone = phonePlayer;
        taskManager.ReportPhoneConnected();

        if (taskManager.IsExperimentRunning)
        {
            SyncCompleteStateToPhone();
        }
    }

    private void HandlePhoneDisconnection()
    {
        isConnectedToPhone = false;
        smartphone = null;
        Debug.Log("Phone disconnected");
    }

    public void SyncCompleteStateToPhone()
    {
        if (smartphone == null) return;
        int selectedIndex = selectedObject != null ? sceneObjects.IndexOf(selectedObject) : -1;
        float[][] allColors = ExtractAllColors();


        pv.RPC("SyncCompletePhoneState", smartphone,
            cachedLabelTexts,
            cachedLabelTitles,
            allColors,
            selectedIndex,
            (int)currentCondition);

    }

    private float[][] ExtractAllColors()
    {
        float[][] colors = new float[sceneObjects.Count][];
        for (int i = 0; i < sceneObjects.Count; i++)
        {
            Color color = objectColors.ContainsKey(sceneObjects[i]) ? objectColors[sceneObjects[i]] : Color.white;
            colors[i] = new float[] { color.r, color.g, color.b };
        }
        return colors;
    }

    #endregion

    #region Interaction Handlers

    private void HandleRaySelection()
    {
        if (phoneObject == null) return;

        pointingRayLine.enabled = isPointing;

        if (!isPointing)
        {
            ResetLastHoveredButton();
            return;
        }

        Vector3 rayOrigin = PhoneRayOrigin;
        Vector3 rayDirection = phoneObject.forward;
        Ray ray = new(rayOrigin, rayDirection);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, rayLength, objectLayer | buttonLayer);

        UpdateRayVisuals(didHit, hit.point, ray.origin, ray.direction);
        ProcessRaycastHit(didHit, hit);

        if (selectActionTriggered) selectActionTriggered = false;
    }

    private void UpdateVisualCues()
    {
        if (currentCondition == StudySettings.Condition.Proximity) HandleProximitySelection();
        if (currentCondition == StudySettings.Condition.Line && selectedObject != null) UpdateLineCue();
    }

    private void UpdateRayVisuals(bool didHit, Vector3 hitPoint, Vector3 origin, Vector3 direction)
    {
        pointingRayLine.material.color = didHit ? Color.red : Color.cyan;
        Vector3 endPoint = didHit ? hitPoint : origin + direction * rayLength;
        pointingRayLine.SetPosition(0, origin);
        pointingRayLine.SetPosition(1, endPoint);
    }

    private void ProcessRaycastHit(bool didHit, RaycastHit hit)
    {
        if (!didHit)
        {
            ResetLastHoveredButton();
            return;
        }

        if (((1 << hit.collider.gameObject.layer) & buttonLayer) != 0)
            ProcessButtonHit(hit.collider.gameObject);
        else
        {
            ResetLastHoveredButton();
            if (selectActionTriggered)
                ToggleSelection(GetRootObject(hit.collider.gameObject));
        }
    }

    private void ProcessButtonHit(GameObject buttonObject)
    {
        var interactable = buttonObject.GetComponent<Interactable>();
        if (interactable == null) return;
        if (!interactable.IsEnabled) return;

        if (lastHoveredButton != buttonObject)
        {
            ResetLastHoveredButton();
            lastHoveredButton = buttonObject;
            interactable.HasFocus = true;
        }

        if (selectActionTriggered)
        {
            if (buttonObject.name == "StartButton")
            {
                taskManager.StartExperiment();
                return;
            }

            interactable.OnClick.Invoke();
            //interactable.HasPress = true;
        }
        //else
        //{
        //    if (!IsQuizAnswerButton(buttonObject))
        //        interactable.HasPress = false;
        //}
    }


    private bool IsQuizAnswerButton(GameObject buttonObject)
    {
        var buttonHelper = buttonObject.GetComponent<ButtonConfigHelper>();

        foreach (var helper in quizManager.ButtonHelpers)
        {
            if (helper == buttonHelper)
                return true;
        }
        return false;
    }

    private void ResetLastHoveredButton()
    {
        if (lastHoveredButton != null)
        {
            var interactable = lastHoveredButton.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.HasFocus = false;
                //if (!IsQuizAnswerButton(lastHoveredButton))
                //    interactable.HasPress = false;
            }
            lastHoveredButton = null;
        }
    }

    private GameObject GetClosestObjectInProximity()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(phoneObject.position, proximityThreshold, objectLayer);
        GameObject closestObject = null;
        float minSqrDistance = float.MaxValue;

        foreach (var col in nearbyColliders)
        {
            GameObject root = GetRootObject(col.gameObject);
            if (root != null)
            {
                float sqrDistance = (phoneObject.position - col.ClosestPoint(phoneObject.position)).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    closestObject = root;
                }
            }
        }

        return closestObject;
    }

    public void HandleProximitySelection()
    {
        GameObject closestObject = GetClosestObjectInProximity();

        if (closestObject != null && selectedObject != closestObject)
        {
            int id = sceneObjects.IndexOf(closestObject);
            if (!taskManager.IsPracticeSession)
                StudyLogger.Instance.LogInteraction("proximity", id.ToString(), "AutoSelect");
            SelectObject(closestObject);
        }
            
        else if (closestObject == null && selectedObject != null)
        {
            if (!taskManager.IsPracticeSession)
                StudyLogger.Instance.LogInteraction("proximity", null, "AutoDeselect");
            DeselectObject();
        }  
    }
    #endregion

    #region Object Selection

    private void ToggleSelection(GameObject obj)
    {
        if (obj == null) return;
        int id = sceneObjects.IndexOf(obj);

        if (currentCondition == StudySettings.Condition.Proximity)
        {
            if (!taskManager.IsPracticeSession)
                StudyLogger.Instance.LogInteraction("object", id.ToString(), "ApproachPrompt");
            StartCoroutine(ShowApproachThenEvaluate());
            return;
        }

        if (selectedObject == obj)
        {
            if (!taskManager.IsPracticeSession)
                StudyLogger.Instance.LogInteraction("object", null, "Deselect");
            DeselectObject();
        }
        else
        {
            if (!taskManager.IsPracticeSession)
                StudyLogger.Instance.LogInteraction("object", id.ToString(), "Select");
            SelectObject(obj);
        }
    }

    public IEnumerator ShowApproachThenEvaluate()
    {
        SendApproachMessageToPhone();
        phoneLabelHandler.ShowApproachMessage();

        yield return new WaitForSeconds(3f);

        GameObject closestObject = GetClosestObjectInProximity();

        if (closestObject != null)
            SelectObject(closestObject);
        else
        {
            phoneLabelHandler.HideLabel();
            SendHideMessageToPhone();
        }
    }

    private void SelectObject(GameObject obj)
    {
        if (obj == null) return;

        if (selectedObject != null)
            DeselectObject();
        selectedObject = obj;

        switch (currentCondition)
        {
            case StudySettings.Condition.Line:
                connectionLine.enabled = true;
                UpdateLineCue();
                break;
            case StudySettings.Condition.Highlight:
                ApplyHighlight(obj, true);
                break;
        }

        int id = sceneObjects.IndexOf(selectedObject);
        if (id == -1) return;

        phoneLabelHandler.ShowLabelFullscreen(id);
        if (smartphone != null)
            pv.RPC("ShowFullLabelDirect", smartphone, id);
    }

    private void DeselectObject()
    {
        if (selectedObject == null) return;

        switch (currentCondition)
        {
            case StudySettings.Condition.Line when connectionLine != null:
                connectionLine.enabled = false;
                break;
            case StudySettings.Condition.Highlight:
                ApplyHighlight(selectedObject, false);
                break;
        }

        selectedObject = null;
        phoneLabelHandler.HideLabel();

        if (smartphone != null)
            pv.RPC("ShowOverviewPageDirect", smartphone);

    }

    private void ApplyHighlight(GameObject obj, bool enabled)
    {
        var outlineManager = obj.GetComponent<MeshOutlineHierarchy>();
        if (outlineManager == null && enabled)
        {
            outlineManager = obj.AddComponent<MeshOutlineHierarchy>();
            outlineManager.OutlineMaterial = new Material(baseOutlineMaterial);
            outlineManager.OutlineWidth = 0.01f;
            outlineManager.OutlineMaterial.color = highlightColor;
        }
        if (outlineManager != null)
        {
            var outlines = obj.GetComponentsInChildren<MeshOutline>();
            foreach (var outline in outlines)
                outline.enabled = enabled;
        }
    }
    public void SelectObjectExternally(GameObject obj) => SelectObject(obj);
    public void DeselectObjectExternally() => DeselectObject();

    public void SendApproachMessageToPhone()
    {
        if (smartphone != null)
            pv.RPC("ControlLabelOnPhone", smartphone, true, -1, null,
                "<size=200%>※<br>설명을 보려면<br>가까이 다가가주세요.</size>"
                );
    }

    public void SendHideMessageToPhone()
    {
        if (smartphone != null)
            pv.RPC("ControlLabelOnPhone", smartphone, false, -1, null, "");
    }

    #endregion

    #region Initialisation
    private void AssignUniqueColorsToObjects()
    {
        Color[] palette = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };

        for (int i = 0; i < sceneObjects.Count && i < palette.Length; i++)
        {
            GameObject obj = sceneObjects[i];
            Color color = palette[i];
            objectColors[obj] = color;

            Material matInstance = new Material(baseOutlineMaterial) { color = color };
            var outline = obj.GetComponent<MeshOutlineHierarchy>() ?? obj.AddComponent<MeshOutlineHierarchy>();
            outline.OutlineMaterial = matInstance;
            outline.OutlineWidth = 0.01f;
        }
    }

    private void InitialisePointingRay()
    {
        pointingRayLine = new GameObject("PhonePointingRay").AddComponent<LineRenderer>();
        pointingRayLine.material = new Material(Shader.Find("Mixed Reality Toolkit/Standard")) { color = Color.cyan };
        pointingRayLine.startWidth = 0.002f;
        pointingRayLine.endWidth = 0.002f;
        pointingRayLine.positionCount = 2;
        pointingRayLine.enabled = false;
    }

    private void InitialiseLineCue()
    {
        connectionLine = new GameObject("SelectionLineCue").AddComponent<LineRenderer>();
        connectionLine.material = connectionLineMaterial;
        connectionLine.startWidth = 0.0001f;
        connectionLine.endWidth = 0.02f;
        connectionLine.positionCount = 2;
        //connectionLine.textureMode = LineTextureMode.Tile;
        connectionLine.enabled = false;
    }
    #endregion

    #region Utility Methods

    private void UpdateLineCue()
    {
        if (connectionLine != null && selectedObject != null)
        {
            connectionLine.SetPosition(0, PhoneRayOrigin);
            connectionLine.SetPosition(1, selectedObject.transform.position);
        }
    }

    private GameObject GetRootObject(GameObject hitObject)
    {
        Transform current = hitObject.transform;
        while (current != null)
        {
            if (sceneObjects.Contains(current.gameObject))
                return current.gameObject;
            current = current.parent;
        }
        return null;
    }
    private Vector3 PhoneRayOrigin
    {
        get
        {
            float offset = 0.025f;
            Vector3 localHeadPosition = new Vector3(0, 0, 0.1633f / 2.0f + offset);
            return phoneObject.transform.TransformPoint(localHeadPosition);
        }
    }
    #endregion

    #region PUN RPCs

    [PunRPC]
    public void RequestSelectObjectFromPhone(int index)
    {
        if (currentCondition == StudySettings.Condition.Proximity)
        {
            if (!taskManager.IsPracticeSession)
                StudyLogger.Instance.LogInteraction("object", index.ToString(), "ApproachPrompt");
            StartCoroutine(ShowApproachThenEvaluate());
            return;
        }
        if (index >= 0 && index < sceneObjects.Count)
            SelectObject(sceneObjects[index]);

        if (!taskManager.IsPracticeSession)
            StudyLogger.Instance.LogInteraction("phone", index.ToString(), "Select");
    }

    [PunRPC]
    public void RequestDeselectFromPhone()
    {
        DeselectObject();
        if (!taskManager.IsPracticeSession)
            StudyLogger.Instance.LogInteraction("phone", null, "Deselect");
    }

    [PunRPC]
    public void SetRayHold(bool isHeld)
    {
        isPointing = isHeld;
    }

    [PunRPC]
    public void SelectWithRay()
    {
        if (!isPointing) return;
        selectActionTriggered = true;
    }
    #endregion
}