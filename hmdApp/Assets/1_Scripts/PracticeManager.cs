using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PracticeManager : MonoBehaviour
{
    [Header("Settings")]
    private StudySettings.Condition[] conditionOrder;

    [Header("References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private ConditionManager conditionManager;
    [SerializeField] private QuizManager quizManager;

    [Header("UI labels")]
    private int currentConditionIndex = 0;

    void Start()
    {
        conditionOrder = (StudySettings.Condition[])System.Enum.GetValues(typeof(StudySettings.Condition));
    }

    public void OnQuestionAdvanced(int questionNumber)
    {
        if (questionNumber > 0 && questionNumber % 2 == 0)
            SwitchToNextCondition();
    }

    private void SwitchToNextCondition()
    {
        currentConditionIndex++;

        if (currentConditionIndex < conditionOrder.Length)
        {
            var newCondition = conditionOrder[currentConditionIndex];

            if (conditionManager != null)
                conditionManager.SwitchCondition(newCondition);
        }
    }


}
