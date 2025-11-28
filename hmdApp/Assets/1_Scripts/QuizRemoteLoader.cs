using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QuizRemoteLoader : MonoBehaviour
{
    public QuizManager quizManager;
    public ConditionManager conditionManager;
    public TaskManager taskManager;

    private string sheetID = "1EfZgXCWRBTnIy_1__mlUA8t8wrCc8-WshBPf00HuWJU";
    private string apiKey = "AIzaSyDb0Q5TexdVQ5WW946GDumvCEfwkGj38Ms";

    public void StartFetchingQuizzes()
    {
        StartCoroutine(LoadQuestionsFromJson());
    }

    private IEnumerator LoadQuestionsFromJson()
    {
        string targetSheetName;
        Debug.Log("fetch");

        switch (taskManager.CurrentTask)
        {
            case StudySettings.Task.task1:
                targetSheetName = "Questions1";
                break;
            case StudySettings.Task.task2:
                targetSheetName = "Questions2";
                break;
            case StudySettings.Task.practice:
                targetSheetName = "Practice";
                break;
            default:
                Debug.LogError("Unhandled task type: " + taskManager.CurrentTask + ". Cannot load questions.", this);
                yield break;
        }
 
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{sheetID}/values/{targetSheetName}?key={apiKey}";

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load questions: " + www.error);
            //taskManager.ReportQuizzesLoaded();
            yield break;
        }

        string json = www.downloadHandler.text;
        var data = JSON.Parse(json);

        var values = data["values"];
        if (values == null || values.Count < 2)
        {
            Debug.LogError("No data found in the sheet.");
            yield break;
        }

        List<Question> questions = new List<Question>();
        
        const int headerRows = 1;

        if (taskManager.IsPracticeSession)
        {
            const int practiceQuestions = 8;
            for (int i = headerRows; i < headerRows + practiceQuestions && i < values.Count; i++)
            {
                var col = values[i];
                if (col.Count < 8) continue;

                Question q = new Question
                {
                    questionText = col[2],
                    options = new string[] { col[3], col[4], col[5], col[6] },
                    correctIndex = int.TryParse(col[7], out int correct) ? correct : 0
                };
                questions.Add(q);
            }
        }

        else
        {
            const int numQuestionsPerBlock = 4;
            int startRow = (taskManager.BlockID - 1) * numQuestionsPerBlock + headerRows;
            int endRow = startRow + numQuestionsPerBlock;

            for (int i = startRow; i < endRow && i < values.Count; i++)
            {
                var col = values[i];
                if (col.Count < 8) continue;

                Question q = new Question
                {
                    questionText = col[2],
                    options = new string[]
                    {
                    col[3], col[4], col[5], col[6]
                    },
                    correctIndex = int.TryParse(col[7], out int correct) ? correct : 0
                };
                questions.Add(q);
            }

        }
        quizManager.InitializeQuiz(questions);
        taskManager.ReportQuizzesLoaded();
    }
}
