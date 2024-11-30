using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public event Action QuestionPopupClosed;

    [SerializeField] private InspectController _inspectionController;
    [SerializeField] private QuestionPopup _questionPopup;

    [Space, SerializeField] private TextMeshProUGUI _counter;
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
        InputManager.Instance.TrashDroppedOnInspectTable += OnTrashDroppedOnInspectTable;
        InputManager.Instance.TrashPickedFromInspectTable += OnTrashPickedFromInspectTable;
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

    private void OnTrashDroppedOnInspectTable(Trash trash)
    {
        _inspectionController.ActivateInspect(trash.Data);
        _inspectionController.AddTrashToTable(trash);
        _okButton.interactable = false;
    }

    private void OnTrashPickedFromInspectTable(Trash trash)
    {
        _inspectionController.RemoveTrashFromTable(trash);
        _okButton.interactable = _inspectionController.TrashCount <= 0;
    }

    public void IncreaseCounter(int increaseAmount = 1)
    {
        Score += increaseAmount;
        _counter.SetText(Score.ToString());
    }
}