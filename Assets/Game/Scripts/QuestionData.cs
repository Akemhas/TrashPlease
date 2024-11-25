using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Create QuestionData", fileName = "QuestionData", order = 0)]
public class QuestionData : ScriptableObject
{
    public List<Question> Questions = new();

    [Button]
    private void PrintJson()
    {
        var t = JsonConvert.SerializeObject(Questions, Formatting.Indented);
        GUIUtility.systemCopyBuffer = t;
        Debug.Log(t);
    }
}

[Serializable]
public class Question
{
    public string QuestionText;
    public string CorrectAnswer;
    public List<string> WrongAnswers;
}