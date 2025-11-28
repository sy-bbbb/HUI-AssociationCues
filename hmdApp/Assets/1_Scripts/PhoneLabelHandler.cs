using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System.Linq;
using static StudySettings;

public class PhoneLabelHandler : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private bool phoneDebugging;
    [SerializeField] private GameObject overviewPage;
    [SerializeField] private GameObject fullLabelPage;
    [SerializeField] private TextMeshProUGUI fullLabelText;
    [SerializeField] private Image colorTag;
    [SerializeField] private Button closeButton;

    [Header("External Manager")]
    [SerializeField] private ConditionManager conditionManager;
    [SerializeField] private TaskManager taskManager;


    // --- Google Sheets Configuration ---
    private const string GOOGLE_SHEETS_API_KEY = "AIzaSyDb0Q5TexdVQ5WW946GDumvCEfwkGj38Ms";
    private const string SHEET_ID = "1e_xp0G0IIslPq5JrR9QZJd2s6Qy11CYhfIF4Ef3xpo0";
    private const string SHEET_NAME = "descriptions";

    // Private state
    private Condition currentCondition;
    private int selectedIndex = -1;
    private List<GameObject> linkedObjects;
    private Dictionary<GameObject, Color> objectColors;
    private List<Button> labelThumbnails = new List<Button>();

    // Data fetched from Google Sheets
    [Header("Label Data")]
    [TextArea, SerializeField] private List<string> labelContents;
    [SerializeField] private List<string> labelTitles;
    public List<string> LabelContents => labelContents;
    public List<string> LabelTitles => labelTitles;

    private void Start()
    {
        if (!phoneDebugging)
        {
            overviewPage.SetActive(false);
            fullLabelPage.SetActive(false);
        }
    }

    public void StartFetchingLabels()
    {
        List<string> requiredNames = taskManager.CurrentBlockPrefabNames;
        StartCoroutine(FetchSheetDataCoroutine(requiredNames));
    }


    public void InitializePhoneUI(List<GameObject> objects, Dictionary<GameObject, Color> colors, Condition condition)
    {
        linkedObjects = objects;
        objectColors = colors;
        currentCondition = condition;

        if (!phoneDebugging) return;

        labelThumbnails = new List<Button>(overviewPage.GetComponentsInChildren<Button>(true));

        for (int i = 0; i < labelThumbnails.Count; i++)
        {
            int index = i;
            labelThumbnails[i].onClick.AddListener(() => OnLabelThumbnailClicked(index));
        }
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        ShowOverviewPage();
    }

    #region UI Event Handlers

    private void OnLabelThumbnailClicked(int index)
    {
        if (!phoneDebugging) return;
        if (index < 0 || index >= linkedObjects.Count) return;

        selectedIndex = index;
        var targetObject = linkedObjects[index];

        switch (currentCondition)
        {
            case Condition.Proximity:
                StartCoroutine(conditionManager.ShowApproachThenEvaluate());
                ShowApproachMessage();
                break;
            case Condition.Highlight:
            case Condition.Line:
            case Condition.Color:
                conditionManager.SelectObjectExternally(targetObject);
                break;
        }
    }

    private void OnCloseButtonClicked()
    {
        if (!phoneDebugging) return;
        ShowOverviewPage();
        conditionManager.DeselectObjectExternally();
        selectedIndex = -1;
    }

    #endregion

    #region UI State Management

    public void ShowLabelFullscreen(int index)
    {
        if (!phoneDebugging) return;
        if (index < 0 || index >= linkedObjects.Count || index >= labelContents.Count) return;

        selectedIndex = index;
        fullLabelText.text = labelContents[index];

        Color tagColor = default;
        bool useColorTag = currentCondition == Condition.Color &&
                           objectColors.TryGetValue(linkedObjects[index], out tagColor);

        colorTag.gameObject.SetActive(useColorTag);
        if (useColorTag)
            colorTag.color = tagColor;

        overviewPage.SetActive(false);
        fullLabelPage.SetActive(true);
    }

    public void HideLabel() => ShowOverviewPage();
    private void ShowOverviewPage()
    {
        if (!phoneDebugging) return;
        overviewPage.SetActive(true);
        fullLabelPage.SetActive(false);
    }

    public void ShowApproachMessage()
    {
        if (!phoneDebugging) return;
        overviewPage.SetActive(false);
        fullLabelPage.SetActive(true);
        fullLabelText.text = "설명을 보려면 가까이 다가가주세요.";
        colorTag.gameObject.SetActive(false);
    }

    #endregion

    #region Google Sheets Data Fetching

    private IEnumerator FetchSheetDataCoroutine(List<string> requiredPrefabNames)
    {
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{SHEET_ID}/values/{SHEET_NAME}?key={GOOGLE_SHEETS_API_KEY}";

        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error fetching Google Sheet data: {www.error}");
            yield break;
        }

        labelContents.Clear();
        try
        {
            var data = JSON.Parse(www.downloadHandler.text);
            var values = data["values"];
            const int nameColumnIndex = 1;
            const int titleColumnIndex = 6;
            const int contentColumnIndex = 4;

            var foundLabels = new Dictionary<string, string>();
            var foundTitles = new Dictionary<string, string>();

            for (int i = 1; i < values.Count; i++)
            {
                var row = values[i];
                string sheetPrefabName = row[nameColumnIndex].Value;

                if (requiredPrefabNames.Contains(sheetPrefabName))
                {
                    string content = row[contentColumnIndex].Value ?? "Data not found";
                    string title = row[titleColumnIndex].Value ?? "No Title"; //

                    foundLabels[sheetPrefabName] = content;
                    foundTitles[sheetPrefabName] = title;
                }
            }

            labelContents.Clear();
            labelTitles.Clear();

            foreach (string requiredName in requiredPrefabNames)
            {
                if (foundLabels.TryGetValue(requiredName, out string content))
                {
                    labelContents.Add(content);
                    if (foundTitles.TryGetValue(requiredName, out string title))
                        labelTitles.Add(title);
                    else
                        labelTitles.Add("No Title");

                }
                    
                else
                    Debug.LogWarning($"Label for prefab '{requiredName}' not found in Google Sheet.");
            }

            UpdateThumbnailLabels();
            Debug.Log("Successfully fetched and parsed Google Sheets data by matching names.");
            taskManager.ReportLabelsLoaded();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse Google Sheets JSON. Error: {ex.Message}");
        }
    }

    private void UpdateThumbnailLabels()
    {
        if (!phoneDebugging) return;
        for (int i = 0; i < labelThumbnails.Count; i++)
        {
            if (i < labelTitles.Count)
            {
                var textComponent = labelThumbnails[i].GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null) textComponent.text = labelTitles[i];
            }
        }
    }
    #endregion
}