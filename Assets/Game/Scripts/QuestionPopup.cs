using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class QuestionPopup : MonoBehaviour
{
    public event Action PopupClosed;

    [SerializeField] private QuestionData _questionData;
    [SerializeField] private GameObject _panelHolder;
    [SerializeField] private GameObject _overlay;
    [SerializeField] private Button _closeButton;

    [Header("Question")] [SerializeField] private TextMeshProUGUI _questionTMP;
    [SerializeField] private TextMeshProUGUI _explanationTMP;
    [SerializeField] private List<Answer> _answers;

    private Image _overlayImage;

    private Question _currentQuestion;
    private readonly WaitForSeconds _waitForSeconds = new(3f);
    private Coroutine _closeCoroutine;

    private int QuestionIndex
    {
        get => PlayerPrefs.GetInt(nameof(QuestionIndex), 0);
        set => PlayerPrefs.SetInt(nameof(QuestionIndex), value);
    }

    private void Awake()
    {
        _overlayImage = _overlay.GetComponent<Image>();

        _closeButton.onClick.AddListener(Close);
        foreach (var answer in _answers)
        {
            answer.Clicked += OnAnswerClicked;
        }
    }

    private void OnAnswerClicked(Answer clickedAnswer)
    {
        if (clickedAnswer.IsCorrect)
        {
            clickedAnswer.SetCorrect();
            UIManager.Instance.IncreaseCounter(10);
        }
        else clickedAnswer.SetWrong();

        foreach (var answer in _answers)
        {
            answer.ToggleButtonActiveness(false);
            if (!clickedAnswer.IsCorrect && answer.IsCorrect) answer.SetCorrect();
        }

        if (string.IsNullOrEmpty(_currentQuestion.Explanation))
        {
            _explanationTMP.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            _explanationTMP.transform.parent.gameObject.SetActive(true);
            _explanationTMP.SetText(_currentQuestion.Explanation);
        }

        if (_closeCoroutine != null) StopCoroutine(_closeCoroutine);
        _closeCoroutine = StartCoroutine(CloseTimer());
    }

    private IEnumerator CloseTimer()
    {
        yield return _waitForSeconds;
        Close();
    }

    public void Open()
    {
        SetupQuestion(GetQuestion());
        _overlay.SetActive(true);
        _panelHolder.SetActive(true);

        foreach (var answer in _answers)
        {
            answer.ToggleButtonActiveness(true);
            answer.SetNeutral();
        }

        Tween.Scale(_panelHolder.transform, Vector3.zero, Vector3.one, .2f, Ease.OutBack);
        Tween.Alpha(_overlayImage, 0, .6f, .2f, Ease.OutBack);
    }

    private void Close()
    {
        if (_closeCoroutine != null) StopCoroutine(_closeCoroutine);
        Tween.Alpha(_overlayImage, .6f, 0, .2f, Ease.InBack);
        Tween.Scale(_panelHolder.transform, Vector3.one, Vector3.zero, .2f, Ease.InBack).OnComplete(() =>
        {
            _panelHolder.SetActive(false);
            _overlay.SetActive(false);
            PopupClosed?.Invoke();
        });
    }

    private void SetupQuestion(Question question)
    {
        int wrongAnswerCount = question.WrongAnswers.Count;
        int correctIndex = wrongAnswerCount == 1 ? Random.Range(0, 2) : Random.Range(0, wrongAnswerCount + 1);

        _questionTMP.SetText(question.QuestionText);
        _explanationTMP.transform.parent.gameObject.SetActive(false);

        int answerIndex = 0;

        for (int i = 0; i < _answers.Count; i++)
        {
            bool isUsed = i < wrongAnswerCount + 1;
            var answer = _answers[i];

            if (!isUsed)
            {
                answer.AnswerPanel.gameObject.SetActive(false);
                continue;
            }

            answer.AnswerPanel.gameObject.SetActive(true);
            answer.IsCorrect = i == correctIndex;
            answer.AnswerTMP.SetText(i == correctIndex ? question.CorrectAnswer : question.WrongAnswers[answerIndex]);
            if (i != correctIndex) answerIndex++;
        }

        QuestionIndex++;
    }

    private Question GetQuestion()
    {
        _currentQuestion = QuestionIndex < _questionData.Questions.Count
            ? _questionData.Questions[QuestionIndex]
            : _questionData.Questions[Random.Range(0, _questionData.Questions.Count)];
        return _currentQuestion;
    }
}