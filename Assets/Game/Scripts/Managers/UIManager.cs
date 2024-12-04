using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public event Action QuestionPopupClosed;
    public event Action<int> SwipeButtonClicked;

    [SerializeField] private TextMeshProUGUI _temperatureTMP;
    [SerializeField] private InspectController _inspectionController;
    [SerializeField] private QuestionPopup _questionPopup;

    [Space, SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] private Button _okButton;

    [SerializeField] private Button _rightSwipeButton;
    [SerializeField] private Button _centerSwipeButton;

    [SerializeField] private Image _centerSwipe;
    [SerializeField] private Image _rightSwipe;


    private int Score
    {
        get => PlayerPrefs.GetInt(nameof(Score), 0);
        set => PlayerPrefs.SetInt(nameof(Score), value);
    }

    private void Awake()
    {
        _okButton.onClick.AddListener(OnOkButtonClicked);
        _questionPopup.PopupClosed += OnQuestionPopupClosed;
        TemperatureManager.Instance.TemperatureChanged += OnTemperatureChanged;
        InputManager.Instance.TrashPickedFromInspectTable += OnTrashPickedFromInspectTable;
        
        _rightSwipeButton.onClick.AddListener(() => SwipeButtonClicked?.Invoke(1));
        _centerSwipeButton.onClick.AddListener(() => SwipeButtonClicked?.Invoke(0));
    }

    private void OnTemperatureChanged(float temp)
    {
        _temperatureTMP.SetText($"{temp:00.0}°C");
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

    public void OnTrashDroppedOnInspectTable(Trash trash)
    {
        if (!trash.inspect) return;
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

    public void ChangeSwipeIndicatorVisual(int posIndex)
    {
        _centerSwipe.color = posIndex == 0 ? Color.gray : Color.white;
        _rightSwipe.color = posIndex == 1 ? Color.gray : Color.white;
    }
}