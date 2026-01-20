using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class QuestionPopup : MonoBehaviour
{
    public event Action PopupClosed;

    private QuestionData QuestionData => SettingsManager.ActiveLanguage != "de" ? _questionDataDe : _questionDataEn;
    [SerializeField] private QuestionData _questionDataEn;
    [SerializeField] private QuestionData _questionDataDe;
    [SerializeField] private GameObject _panelHolder;
    [SerializeField] private GameObject _overlay;
    [SerializeField] private Button _closeButton;

    [Header("Question")]
    [SerializeField] private TextMeshProUGUI _questionTMP;
    [SerializeField] private TextMeshProUGUI _explanationTMP;
    [SerializeField] private List<Answer> _answers;

    private Image _overlayImage;

    private Question _currentQuestion;
    private readonly WaitForSecondsRealtime _waitForSeconds = new(0.3f);
    private Coroutine _closeCoroutine;
    [SerializeField] private Vector2 _closeButtonDisabledPosition;
    [SerializeField] private Vector2 _closeButtonEnabledPosition;

    private int QuestionIndex
    {
        get => PlayerPrefs.GetInt(nameof(QuestionIndex), 0);
        set => PlayerPrefs.SetInt(nameof(QuestionIndex), value);
    }

    private int _answerCount;

    private void Awake()
    {
        _overlayImage = _overlay.GetComponent<Image>();
    }

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(OnCloseClicked);
        foreach (var answer in _answers)
        {
            answer.Clicked += OnAnswerClicked;
        }
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(OnCloseClicked);
        foreach (var answer in _answers)
        {
            answer.Clicked -= OnAnswerClicked;
        }
    }

    private void OnCloseClicked()
    {
        AudioManager.Instance.PlaySoundEffect(SoundEffectType.Click);
        Close();
    }

    private void OnAnswerClicked(Answer clickedAnswer)
    {
        _answerCount++;
        if (clickedAnswer.IsCorrect)
        {
            clickedAnswer.SetCorrect();
            AudioManager.Instance.PlaySoundEffect(SoundEffectType.QuizPopup);
            if (_answerCount < 4)
            {
                UIManager.Instance.IncreaseCounter(6 - _answerCount);
            }

            if (_closeCoroutine != null) StopCoroutine(_closeCoroutine);
            _closeCoroutine = StartCoroutine(CloseTimer());

            foreach (var answer in _answers)
            {
                answer.ToggleButtonActiveness(false);
            }
        }
        else clickedAnswer.SetWrong();

        if (string.IsNullOrEmpty(_currentQuestion.Explanation))
        {
            _explanationTMP.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            _explanationTMP.transform.parent.gameObject.SetActive(true);
            _explanationTMP.SetText(_currentQuestion.Explanation);
        }
    }

    private IEnumerator CloseTimer()
    {
        yield return _waitForSeconds;
        Close(true);
    }

    public void Open(bool isInstant = false)
    {
        _answerCount = 0;
        Time.timeScale = 0;
        ((RectTransform)_closeButton.transform).anchoredPosition = _closeButtonDisabledPosition;
        SetupQuestion(GetQuestion());
        // _overlay.SetActive(true);
        _panelHolder.SetActive(true);

        foreach (var answer in _answers)
        {
            answer.ToggleButtonActiveness(true);
            answer.SetNeutral();
        }

        if (isInstant)
        {
            _panelHolder.transform.localScale = Vector3.one;
        }
        else
        {
            Tween.Scale(_panelHolder.transform, Vector3.zero, Vector3.one, .2f, Ease.OutBack, useUnscaledTime: true);
            // Tween.Alpha(_overlayImage, 0, .6f, .2f, Ease.OutBack, useUnscaledTime: true);
        }
    }

    public void Close(bool isInstant = false, bool invokeActions = true)
    {
        Time.timeScale = 1;
        if (_closeCoroutine != null) StopCoroutine(_closeCoroutine);
        if (isInstant)
        {
            _panelHolder.transform.localScale = Vector3.zero;
            _panelHolder.SetActive(false);
            // _overlay.SetActive(false);
            if (invokeActions) PopupClosed?.Invoke();
            return;
        }

        // Tween.Alpha(_overlayImage, .6f, 0, .2f, Ease.InBack);
        Tween.Scale(_panelHolder.transform, Vector3.one, new Vector3(.3f, .3f, .3f), .2f, Ease.InBack)
            .OnComplete(() =>
            {
                _panelHolder.SetActive(false);
                // _overlay.SetActive(false);
                if (invokeActions) PopupClosed?.Invoke();
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
        _currentQuestion = QuestionIndex < QuestionData.Questions.Count
            ? QuestionData.Questions[QuestionIndex]
            : QuestionData.Questions[Random.Range(0, QuestionData.Questions.Count)];
        return _currentQuestion;
    }
}