using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [Header("External Managers")]
    private TaskManager taskManager;
    [SerializeField] private PracticeManager practiceManager;
    
    [Header("Quiz Content")]
    [SerializeField] private List<Question> questions = new List<Question>();

    [Header("Button References")]
    [SerializeField] private ButtonConfigHelper[] buttonHelpers = new ButtonConfigHelper[4];
    [SerializeField] private ButtonConfigHelper nextButton;
    [SerializeField] private TextMeshPro questionLabel;

    private int currentQuestionIndex = 0;
    private int selectedAnswerIndex = -1;
    private bool hasSelectedAnswer = false;

    public ButtonConfigHelper[] ButtonHelpers => buttonHelpers;

    private void Start()
    {
        taskManager = GetComponent<TaskManager>();

        if (nextButton != null)
        {
            var nextInteractable = nextButton.GetComponent<Interactable>();
            nextInteractable.OnClick.AddListener(OnNextButtonPressed);
            nextInteractable.IsEnabled = false;
        }
    }

    void ShowQuestion(int index)
    {
        int maxQuestions = taskManager.IsPracticeSession ? 8 : questions.Count;

        if (index >= maxQuestions)
        {
            foreach (var btn in buttonHelpers)
            {
                btn.MainLabelText = "";
                var interactable = btn.GetComponent<Interactable>();
                interactable.IsEnabled = false;
            }

            if (nextButton != null)
            {
                var nextInteractable = nextButton.GetComponent<Interactable>();
                nextInteractable.IsEnabled = false;
            }

            questionLabel.text = "ÄûÁî Á¾·á!";
            if (!taskManager.IsPracticeSession)
                StudyLogger.Instance.StopLogging();
            taskManager.DestroySceneObjects();
            return;
        }

        selectedAnswerIndex = -1;
        hasSelectedAnswer = false;

        if (nextButton != null)
        {
            var nextInteractable = nextButton.GetComponent<Interactable>();
            nextInteractable.IsEnabled = false;
        }


        Question q = questions[index];
        questionLabel.text = q.questionText;

        for (int i = 0; i < buttonHelpers.Length; i++)
        {
            int choiceIndex = i;
            buttonHelpers[i].MainLabelText = q.options[i];
            var interactable = buttonHelpers[i].GetComponent<Interactable>();
            interactable.IsEnabled = true;
            
            interactable.OnClick.RemoveAllListeners();
            interactable.OnClick.AddListener(() => OnAnswerSelected(choiceIndex));

            interactable.IsToggled = false;

            interactable.HasFocus = false;
            interactable.HasPress = false;
        }
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        // A selection has been made, so enable the next button.
        hasSelectedAnswer = true;
        if (nextButton != null)
        {
            nextButton.GetComponent<Interactable>().IsEnabled = true;
        }

        // If the user clicks a new answer, update the index.
        // If they click the same answer, the index just gets set to itself.
        selectedAnswerIndex = selectedIndex;

        // Now, tell all buttons to update their visuals based on the definitive selectedAnswerIndex.
        UpdateButtonSelectionVisuals();
        //if (selectedAnswerIndex != selectedIndex)
        //{
        //    selectedAnswerIndex = selectedIndex;
        //    hasSelectedAnswer = true;
        //    UpdateButtonSelectionVisuals(selectedIndex);
        //}
        ////selectedAnswerIndex = selectedIndex;
        ////hasSelectedAnswer = true;

        ////UpdateButtonSelectionVisuals(selectedIndex);

        //if (nextButton != null)
        //{
        //    var nextInteractable = nextButton.GetComponent<Interactable>();
        //    nextInteractable.IsEnabled = true;
        //}
    }

    private void OnNextButtonPressed()
    {
        if (!hasSelectedAnswer) return;
        var correctIndex = questions[currentQuestionIndex].correctIndex;
        StudyLogger.Instance.LogQuizAnswer(
            currentQuestionIndex.ToString(),
            selectedAnswerIndex + 1,
            correctIndex,
            correctIndex == (selectedAnswerIndex + 1)
            );

        currentQuestionIndex++;

        if (taskManager.IsPracticeSession && practiceManager != null)
            practiceManager.OnQuestionAdvanced(currentQuestionIndex);
        ShowQuestion(currentQuestionIndex);
    }

    private void UpdateButtonSelectionVisuals()
    {
        // Loop through all answer buttons.
        for (int i = 0; i < buttonHelpers.Length; i++)
        {
            var interactable = buttonHelpers[i].GetComponent<Interactable>();
            if (interactable != null)
            {
                // The button is toggled ON if its index matches the one we've stored.
                // Otherwise, it is forced OFF.
                interactable.IsToggled = (i == selectedAnswerIndex);
            }
        }
    }

    private void UpdateButtonSelectionVisuals(int selectedIndex)
    {
        for (int i = 0; i < buttonHelpers.Length; i++)
        {
            var interactable = buttonHelpers[i].GetComponent<Interactable>();
            if (interactable != null)
            {
                // Set the toggle state. True if it's the selected button, false otherwise.
                interactable.IsToggled = (i == selectedIndex);
            }
        }
        //for (int i = 0; i < buttonHelpers.Length; i++)
        //{
        //    var interactable = buttonHelpers[i].GetComponent<Interactable>();
        //    if (interactable != null)
        //    {
        //        interactable.HasFocus = false;
        //        interactable.HasPress = false;
        //    }
        //}

        //if (selectedIndex >= 0 && selectedIndex < buttonHelpers.Length)
        //{
        //    var selectedInteractable = buttonHelpers[selectedIndex].GetComponent<Interactable>();
        //    if (selectedInteractable != null)
        //    {
        //        selectedInteractable.HasPress = true;
        //    }
        //}
    }


    public void InitializeQuiz(List<Question> loadedQuestions)
    {
        this.questions = loadedQuestions;
        currentQuestionIndex = 0;
        ShowQuestion(currentQuestionIndex);
    }

}
