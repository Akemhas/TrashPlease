using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public static UIManager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindFirstObjectByType<UIManager>();
            return _instance;
        }

        private set => _instance = value;
    }

    public event Action<int> SwipeButtonClicked;

    public Timer Timer;

    [SerializeField] private QuizController _quizController;
    [SerializeField] private TextMeshProUGUI _temperatureTMP;
    [SerializeField] private InspectController _inspectionController;
    [SerializeField] private FailScreen _failScreen;
    [SerializeField] private LevelCompletePopup _levelCompletePopup;
    [SerializeField] private AllLevelsCompletePopup _allLevelsCompletePopup;

    [Space, SerializeField] private TextMeshProUGUI _counter;
    [SerializeField] private Button _okButton;

    [Header("Level Progress")]
    [SerializeField] private Image _levelProgressFill;
    [SerializeField] private TextMeshProUGUI _levelProgressLabel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        OnTemperatureChanged(TemperatureManager.Temperature);
        _counter.SetText(GameManager.Score.ToString());
    }

    private void OnEnable()
    {
        _okButton.onClick.AddListener(OnOkButtonClicked);
        InputManager.TrashPickedFromInspectTable += OnTrashPickedFromInspectTable;
        TemperatureManager.TemperatureChanged += OnTemperatureChanged;
    }

    private void OnDisable()
    {
        _okButton.onClick.RemoveListener(OnOkButtonClicked);
        InputManager.TrashPickedFromInspectTable -= OnTrashPickedFromInspectTable;
        TemperatureManager.TemperatureChanged -= OnTemperatureChanged;
    }

    private void OnTemperatureChanged(float temp)
    {
        float tempToShow = temp + 18f;
        _temperatureTMP.SetText($"{tempToShow:0.0}°C");
    }

    public void OpenQuestionPopup()
    {
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
        var score = GameManager.Score + increaseAmount;
        GameManager.Score = score;
        _counter.SetText(score.ToString());
    }

    public void Fail()
    {
        _failScreen.Open();
    }

    public void SetLevelProgress(int sorted, int total)
    {
        if (_levelProgressFill == null) return;
        float fill = total > 0 ? (float)sorted / total : 0;
        Tween.StopAll(_levelProgressFill);
        Tween.UIFillAmount(_levelProgressFill, fill, .1f);

        if (_levelProgressLabel != null)
        {
            _levelProgressLabel.SetText($"{sorted}/{Mathf.Max(total, 1)}");
        }
    }

    public void ShowLevelComplete(bool isLastLevel, Action nextLevel, Action restartLevel)
    {
        if (_levelCompletePopup == null) return;
        nextLevel += OnNextLevel;
        _levelCompletePopup.Open(isLastLevel, nextLevel, restartLevel);
    }

    private void OnNextLevel()
    {
        if (GameManager.Instance._levelManager.CurrentLevelIndex % 2 == 1)
        {
            AudioManager.Instance.PlaySoundEffect(SoundEffectType.QuizPopup);
            _quizController.Open();
        }
    }

    public void ShowAllLevelsComplete(Action restartFromBeginning)
    {
        if (_allLevelsCompletePopup == null) return;
        _allLevelsCompletePopup.Open(restartFromBeginning);
    }
}