using UnityEngine;
using System.Threading.Tasks;
using System.Diagnostics; // Used for the high-precision Stopwatch

public class StudyLogger : MonoBehaviour
{
    public static StudyLogger Instance { get; private set; }

    // --- Log Files ---
    private FileWriter sessionLog;
    private FileWriter quizLog;
    private FileWriter interactionLog;
    private FileWriter movementLog;

    // --- State & Timing ---
    private Stopwatch stopwatch = new Stopwatch();
    private bool isLogging = false;


    // --- Scene References ---
    private Transform cameraTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cameraTransform = Camera.main.transform;

        int targetFPS = 60;
        Application.targetFrameRate = targetFPS;
        Time.fixedDeltaTime = 1.0f / targetFPS;
    }

    public async Task MakeLogFile(string participantID, string taskID, string conditionID, string blockID)
    {
        if (isLogging) return;

        //string sessionFolderName = $"{participantID}_{taskID}_{conditionID}_{blockID}";
        string baseFileName = $"{participantID}_{taskID}_{conditionID}_{blockID}";

        var tasks = new[]
        {
        FileWriter.CreateAsync(participantID, $"{baseFileName}_SessionLog.csv", "TotalTimeSeconds"),
        FileWriter.CreateAsync(participantID, $"{baseFileName}_QuizLog.csv", "Timestamp, QuestionIndex,SelectedAnswer,IsCorrect"),
        FileWriter.CreateAsync(participantID, $"{baseFileName}_InteractionLog.csv", "Timestamp,InteractionType,TargetObject,Details"),
        FileWriter.CreateAsync(participantID, $"{baseFileName}_MovementLog.csv", "Timestamp,HeadPosX,HeadPosY,HeadPosZ,HeadRotX,HeadRotY,HeadRotZ,HeadRotW")
        };

        var results = await Task.WhenAll(tasks);

        sessionLog = results[0];
        quizLog = results[1];
        interactionLog = results[2];
        movementLog = results[3];

        //sessionLog = new FileWriter(participantID, $"{baseFileName}_SessionLog.csv", "TotalTimeSeconds");
        //quizLog = new FileWriter(participantID, $"{baseFileName}_QuizLog.csv", "Timestamp, QuestionIndex,SelectedAnswer,IsCorrect");
        //interactionLog = new FileWriter(participantID, $"{baseFileName}_InteractionLog.csv", "Timestamp,InteractionType,TargetObject,Details");
        //movementLog = new FileWriter(participantID, $"{baseFileName}_MovementLog.csv", "Timestamp,HeadPosX,HeadPosY,HeadPosZ,HeadRotX,HeadRotY,HeadRotZ,HeadRotW");
    }

    public void StartLogging()
    {
        stopwatch.Restart();
        isLogging = true;
        UnityEngine.Debug.Log("Logging started.");
        sessionLog.WriteLine($"{stopwatch.Elapsed.ToString()}, start");
    }

    public void StopLogging()
    {
        if (!isLogging) return;

        isLogging = false;
        stopwatch.Stop();

        sessionLog.WriteLine(stopwatch.ElapsedMilliseconds.ToString());

        sessionLog?.Close();
        quizLog?.Close();
        interactionLog?.Close();
        movementLog?.Close();
        UnityEngine.Debug.Log("Logging stopped and files saved.");
    }

    private void FixedUpdate()
    {
        if (isLogging)
        {
            LogHeadMovement();
        }
    }

    public void LogQuizAnswer(string question, int selectedAnswer, int correctAnswer, bool isCorrect)
    {
        if (!isLogging) return;
        long timestamp = stopwatch.ElapsedMilliseconds;
        quizLog.WriteLine($"{timestamp},{question},{selectedAnswer},{correctAnswer}, {isCorrect}");
    }

    public void LogInteraction(string interactionType, string targetObject, string details = "")
    {
        if (!isLogging) return;
        long timestamp = stopwatch.ElapsedMilliseconds;
        interactionLog.WriteLine($"{timestamp},{interactionType},{targetObject},{details}");
    }

    private void LogHeadMovement()
    {
        long timestamp = stopwatch.ElapsedMilliseconds;

        Vector3 pos = cameraTransform.position;
        Quaternion rot = cameraTransform.rotation;

        string posString = $"{pos.x:F4},{pos.y:F4},{pos.z:F4}";
        string rotString = $"{rot.x:F4},{rot.y:F4},{rot.z:F4},{rot.w:F4}";

        movementLog.WriteLine($"{timestamp},{posString},{rotString}");
    }

    private void OnApplicationQuit()
    {
        StopLogging();
    }
}