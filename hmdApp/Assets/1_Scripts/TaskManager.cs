using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class TaskManager : MonoBehaviour
{
    [Header("Study Settings")]
    [SerializeField] private string participantID = "P01";
    [SerializeField] private StudySettings.Task currentTask = StudySettings.Task.task1;
    [SerializeField] private int blockID = 1;
    [SerializeField] private StudySettings.Condition currentCondition = StudySettings.Condition.Proximity;

    [Header("Component References")]
    [SerializeField] private ConditionManager conditionManager;
    [SerializeField] private PhoneLabelHandler phoneLabelHandler;
    [SerializeField] private QuizRemoteLoader quizRemoteLoader;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject quizPanel;

    [Header("Scene Setup")]
    [SerializeField] private Transform sceneObjectRoot;
    [SerializeField] private string prefabsResourcePath = "SceneObjects";

    private bool isPracticeSession = false;
    private List<GameObject> sceneObjects = new List<GameObject>();
    private bool isPhoneConnected = false;
    private bool areLabelsLoaded = false;
    private bool areQuizzesLoaded = false;
    public bool isConfigurationReceived = false;
    private bool isExperimentRunning = false;

    // --- Public properties ---
    public bool IsExperimentRunning => isExperimentRunning;
    public StudySettings.Task CurrentTask => currentTask;
    public int BlockID => blockID;
    public bool IsPracticeSession => isPracticeSession;
    public StudySettings.Condition CurrentCondition => currentCondition;
    public List<string> CurrentBlockPrefabNames => BlockDataManager.Instance.GetPrefabNamesForBlock(currentTask, blockID);

    void Start()
    {
        //Shader.WarmupAllShaders();
        InitialiseUI();
    }
   
    //private void TurnOffPointers()
    //{
    //    PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOff);
    //    PointerUtils.SetGazePointerBehavior(PointerBehavior.AlwaysOff);
    //}

    
    private void InitialiseUI()
    {
        if (startButton != null)
            startButton.GetComponent<Interactable>().IsEnabled = false;
        if (quizPanel != null)
            quizPanel.SetActive(false);
    }

    private void BeginPreloading()
    {
        phoneLabelHandler.StartFetchingLabels();
        quizRemoteLoader.StartFetchingQuizzes();
    }

    public void ReportPhoneConnected()
    {
        isPhoneConnected = true;
        Debug.Log("Phone connection ready.");
        CheckAllReady();
    }
    public void ReportLabelsLoaded()
    {
        areLabelsLoaded = true;
        Debug.Log("Label contents ready.");
        CheckAllReady();
    }

    public void ReportQuizzesLoaded()
    {
        areQuizzesLoaded = true;
        Debug.Log("Quiz questions ready.");
        CheckAllReady();
    }

    public async void CheckAllReady()
    {
        bool allSystemsReady = isPhoneConnected && areLabelsLoaded && areQuizzesLoaded && isConfigurationReceived;
        if (allSystemsReady && startButton != null)
            startButton.GetComponent<Interactable>().IsEnabled = true;      
    }

    public void StartExperiment()
    {
        isExperimentRunning = true;
        AssignSceneObjects();

        if (isPracticeSession)
            conditionManager.InitialisePractice(currentCondition, currentTask, blockID, sceneObjects);
        else
        {
            conditionManager.Initialise(currentCondition, currentTask, blockID, sceneObjects);
            StudyLogger.Instance.StartLogging();
        }
            
        
        if (startButton != null)
            startButton.SetActive(false);
        if (quizPanel != null)
            quizPanel.SetActive(true);
    }

    public void SwitchConditionForPractice(StudySettings.Condition newCondition)
    {
        if (!isPracticeSession || !isExperimentRunning) return;

        currentCondition = newCondition;
        //conditionManager.SwitchCondition(newCondition, sceneObjects);
    }

    private void AssignSceneObjects()
    {
        sceneObjects.Clear();

        RootTransform rootTransform = BlockDataManager.Instance.GetRootTransformForTask(currentTask);
        sceneObjectRoot.localPosition = rootTransform.Position;
        sceneObjectRoot.localScale = rootTransform.Scale;

        List<string> requiredNames = BlockDataManager.Instance.GetPrefabNamesForBlock(currentTask, blockID);
        List<ObjectTransform> prefabTransforms = BlockDataManager.Instance.GetPrefabTransforms(currentTask);

        if (requiredNames == null || prefabTransforms == null)
        {
            Debug.LogError($"Data for Task '{currentTask}' and Block ID '{blockID}' not defined in BlockDataManager.");
            return;
        }

        var prefabMap = Resources.LoadAll<GameObject>(prefabsResourcePath)
                                 .ToDictionary(prefab => prefab.name, prefab => prefab);

        for (int i = 0; i < requiredNames.Count; i++)
        {
            string nameToFind = requiredNames[i];
            ObjectTransform objectTransform = prefabTransforms[i];

            if (prefabMap.TryGetValue(nameToFind, out GameObject prefabToSpawn))
            {
                GameObject newObject = Instantiate(prefabToSpawn, sceneObjectRoot);
                newObject.transform.localPosition = objectTransform.Position;
                newObject.transform.localRotation = objectTransform.Rotation;
                newObject.transform.localScale = Vector3.one;
                newObject.name = prefabToSpawn.name;
                var renderers = newObject.GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    Material mat = renderer.material;
                    renderer.material = null;
                    renderer.material = mat;
                }
                sceneObjects.Add(newObject);
            }
            else
                Debug.LogWarning($"'{nameToFind}' not found in 'Resources/{prefabsResourcePath}'.");
        }
    }

    public void DestroySceneObjects() => Destroy(sceneObjectRoot.gameObject);

    [PunRPC]
    public async void ReceiveStudyConfiguration(string participantId, int taskValue, int blockId, int conditionValue)
    {
        if (taskValue == (int)StudySettings.Task.practice)
        {
            currentTask = StudySettings.Task.practice;
            isPracticeSession = true;
            participantID = "PRACTICE";
            blockID = 1;
            currentCondition = 0;
        }
        else
        {
            participantID = participantId;
            currentTask = (StudySettings.Task)taskValue;
            blockID = blockId;
            currentCondition = (StudySettings.Condition)conditionValue;
            await StudyLogger.Instance.MakeLogFile(participantID, currentTask.ToString(), currentCondition.ToString(), blockID.ToString());
        }

        isConfigurationReceived = true;
        BeginPreloading();
        CheckAllReady();
    }
}