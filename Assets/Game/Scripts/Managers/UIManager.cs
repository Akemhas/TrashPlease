using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public event Action QuestionPopupClosed;
    public event Action<int> SwipeButtonClicked;

    public Timer Timer;

    [SerializeField] private TextMeshProUGUI _temperatureTMP;
    [SerializeField] private InspectController _inspectionController;
    [SerializeField] private QuestionPopup _questionPopup;
    [SerializeField] private FailScreen _failScreen;

    [Space, SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] private Button _okButton;

    [SerializeField] private Button _leftSwipeButton;
    [SerializeField] private Button _centerSwipeButton;
    [SerializeField] private Button _rightSwipeButton;

    [SerializeField] private Image _leftSwipe;
    [SerializeField] private Image _centerSwipe;
    [SerializeField] private Image _rightSwipe;

    private int Score
    {
        get => PlayerPrefs.GetInt(nameof(Score), 0);
        set => PlayerPrefs.SetInt(nameof(Score), value);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        _questionPopup.PopupClosed += OnQuestionPopupClosed;
        _okButton.onClick.AddListener(OnOkButtonClicked);
        _leftSwipeButton.onClick.AddListener(OnLeftSwipe);
        _rightSwipeButton.onClick.AddListener(OnRightSwipe);
        _centerSwipeButton.onClick.AddListener(OnCenterSwipe);
        InputManager.TrashPickedFromInspectTable += OnTrashPickedFromInspectTable;
        TemperatureManager.TemperatureChanged += OnTemperatureChanged;
    }

    private void OnDisable()
    {
        _questionPopup.PopupClosed -= OnQuestionPopupClosed;
        _okButton.onClick.RemoveListener(OnOkButtonClicked);
        _leftSwipeButton.onClick.RemoveListener(OnLeftSwipe);
        _rightSwipeButton.onClick.RemoveListener(OnRightSwipe);
        _centerSwipeButton.onClick.RemoveListener(OnCenterSwipe);
        InputManager.TrashPickedFromInspectTable -= OnTrashPickedFromInspectTable;
        TemperatureManager.TemperatureChanged -= OnTemperatureChanged;
    }

    private void OnLeftSwipe()
    {
        SwipeButtonClicked?.Invoke(-1);
    }

    private void OnRightSwipe()
    {
        SwipeButtonClicked?.Invoke(1);
    }

    private void OnCenterSwipe()
    {
        SwipeButtonClicked?.Invoke(0);
    }

    private void OnTemperatureChanged(float temp)
    {
        float tempToShow = temp + 18f;
        _temperatureTMP.SetText($"{tempToShow:0.0}°C");
    }

    private void Start()
    {
        _counter.SetText(Score.ToString());
    }

    private void OnQuestionPopupClosed() => QuestionPopupClosed?.Invoke();

    public void OpenQuestionPopup()
    {
        AudioManager.Instance.PlaySoundEffect(SoundEffectType.QuizPopup);
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

    public void Fail()
    {
        _failScreen.Open();
    }

    public void ChangeSwipeIndicatorVisual(int posIndex)
    {
        switch (posIndex)
        {
            case -1:
                _leftSwipe.color = Color.white;
                _centerSwipe.color = Color.gray;
                _rightSwipe.color = Color.gray;
                return;
            case 0:
                _leftSwipe.color = Color.gray;
                _centerSwipe.color = Color.white;
                _rightSwipe.color = Color.gray;
                return;
            case 1:
                _leftSwipe.color = Color.gray;
                _centerSwipe.color = Color.gray;
                _rightSwipe.color = Color.white;
                return;
        }
    }
}