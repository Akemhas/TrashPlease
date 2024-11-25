using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public event Action QuestionPopupClosed;

    [SerializeField] private QuestionPopup _questionPopup;
    [SerializeField] private Button _okButton;

    private void Awake()
    {
        _okButton.onClick.AddListener(OnOkButtonClicked);
        _questionPopup.PopupClosed += OnQuestionPopupClosed;
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
}