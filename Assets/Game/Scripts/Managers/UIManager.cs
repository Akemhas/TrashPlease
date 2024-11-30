using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public event Action QuestionPopupClosed;

    [SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] private QuestionPopup _questionPopup;
    [SerializeField] private Button _okButton;

    private int Score
    {
        get => PlayerPrefs.GetInt(nameof(Score), 0);
        set => PlayerPrefs.SetInt(nameof(Score), value);
    }

    private void Awake()
    {
        _okButton.onClick.AddListener(OnOkButtonClicked);
        _questionPopup.PopupClosed += OnQuestionPopupClosed;
    }

    private void Start()
    {
        _counter.SetText(Score.ToString());
    }

    private void OnQuestionPopupClosed() => QuestionPopupClosed?.Invoke();

    public void OpenQuestionPopup()
    {
        _questionPopup.Open();
    }

    private void OnOkButtonClicked()
    {
        GameManager.Instance.ProgressBin();
    }

    public void IncreaseCounter(int increaseAmount = 1)
    {
        Score += increaseAmount;
        _counter.SetText(Score.ToString());
    }
}