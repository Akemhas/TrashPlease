using System.Collections.Generic;
using UnityEngine;

public class QuizController : MonoBehaviour
{
    [SerializeField] private GameObject _overlay;
    [SerializeField] private List<QuestionPopup> _questions;

    private int _questionIndex = 0;
    private const int QuestionLimit = 5;

    public void Open()
    {
        _questionIndex = 0;
        _overlay.SetActive(true);
        _questionIndex = Mathf.Clamp(_questionIndex, 0, _questions.Count - 1);
        _questions[_questionIndex].Open();
        foreach (var question in _questions)
        {
            question.PopupClosed += OnPopupClosed;
        }
    }

    private void OnPopupClosed()
    {
        _questionIndex = Mathf.Clamp(_questionIndex, 0, _questions.Count - 1);
        _questions[_questionIndex].Close(false, false);
        _questionIndex++;
        if (_questionIndex >= QuestionLimit)
        {
            Close();
        }
        else
        {
            _questions[_questionIndex].Open(false);
        }
    }

    public void Close()
    {
        foreach (var question in _questions)
        {
            question.PopupClosed -= OnPopupClosed;
        }
        _overlay.SetActive(false);
    }
}