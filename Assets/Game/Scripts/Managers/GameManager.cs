using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindFirstObjectByType<GameManager>();
            return _instance;
        }

        private set => _instance = value;
    }

    public static int HighestScore
    {
        get => PlayerPrefs.GetInt(nameof(HighestScore), 0);
        set => PlayerPrefs.SetInt(nameof(HighestScore), value);
    }

    public static int Score
    {
        get => PlayerPrefs.GetInt(nameof(Score), 0);
        set
        {
            if (value > HighestScore)
            {
                HighestScore = value;
            }

            PlayerPrefs.SetInt(nameof(Score), value);
        }
    }

    [ReadOnly] private GameState _currentGameState;

    [SerializeField] private BinController _binController;
    [SerializeField] private TopBinController _topBinController;
    [SerializeField] private TrashController _trashController;

    private TrashSortType _currentSortType;
    private int _currentTrashCount;

    public bool IsScannerEmpty = true;

    private bool _isBinMoving;
    private bool _checking;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        _currentGameState = GameState.WaitingBin;
    }

    private void OnEnable()
    {
        _topBinController.BinReachedEnd += OnTopBinReachedEnd;
        _binController.BinCreated += OnCenterBinCreated;
        _binController.BinBeforeDestroy += OnCenterBinBeforeDestroy;
        _binController.BinReachedCenter += OnCenterBinReachedCenter;
        _trashController.TrashCreated += OnTrashCreated;
        _trashController.AllTrashDestroyed += OnAllTrashDestroyed;
        if (UIManager.Instance)
        {
            UIManager.Instance.QuestionPopupClosed += OnQuestionPopupClosed;
        }
    }

    private void OnDisable()
    {
        _topBinController.BinReachedEnd -= OnTopBinReachedEnd;
        _binController.BinCreated -= OnCenterBinCreated;
        _binController.BinBeforeDestroy -= OnCenterBinBeforeDestroy;
        _binController.BinReachedCenter -= OnCenterBinReachedCenter;
        _trashController.TrashCreated -= OnTrashCreated;
        _trashController.AllTrashDestroyed -= OnAllTrashDestroyed;
        if (UIManager.Instance)
        {
            UIManager.Instance.QuestionPopupClosed -= OnQuestionPopupClosed;
        }
    }

    private void Start()
    {
        AudioManager.Instance.PlaySoundTrack(SoundTrackType.Gameplay);
        _topBinController.Initialize();
        var topBinData = _topBinController.CreateTopBin(5);
        _currentSortType = topBinData.Item1;
        _currentTrashCount = topBinData.Item2;
        _trashController.LoadTrash(_currentSortType, _currentTrashCount);
        _topBinController.CreateTopBin(7);
    }

    private void Update()
    {
        switch (_currentGameState)
        {
            case GameState.Paused:
                return;
            case GameState.WaitingBin:
                if (_topBinController.TryPeek(out var topBin))
                {
                    OnTopBinReachedEnd(topBin);
                }

                break;
        }

        _topBinController.Tick();
    }

    public void ProgressBin()
    {
        if (_isBinMoving) return;
        if (_currentGameState != GameState.SortingBin) return;
        if (_binController.CurrentBin == null) return;
        if (_checking) return;

        AudioManager.Instance.PlaySoundEffect(SoundEffectType.SendButton);
        StartCoroutine(CheckTrash());
    }

    IEnumerator CheckTrash()
    {
        _checking = true;

        _trashController.CheckTrashSorting(_currentSortType);
        yield return new WaitForSeconds(.6f);
        _isBinMoving = true;

        _binController.DestroyBin();

        _checking = false;
    }

    public void Pause()
    {
        if (_currentGameState == GameState.Paused) return;

        _currentGameState = GameState.Paused;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        if (_currentGameState != GameState.Paused) return;

        _currentGameState = IsScannerEmpty ? GameState.WaitingBin : GameState.SortingBin;
        Time.timeScale = 1;
    }

    private void OnTopBinReachedEnd(TopBin topBin)
    {
        if (_currentGameState == GameState.SortingBin) return;

        _currentGameState = GameState.SortingBin;
        _currentSortType = topBin.SortType;
        _currentTrashCount = topBin.TrashCount;
        IsScannerEmpty = false;
        _trashController.LoadTrash(_currentSortType, _currentTrashCount);
        _topBinController.SendBinToScanner().GetAwaiter().OnCompleted(OnTopBinReachedToScanner);
    }

    private void OnCenterBinBeforeDestroy()
    {
        IsScannerEmpty = true;
        _currentGameState = GameState.WaitingBin;
        _trashController.DestroyAllTrash();
    }

    private void OnTopBinReachedToScanner()
    {
        if (_currentSortType != TrashSortType.Question)
        {
            _binController.StartCreatingBin(_currentSortType);
        }
        else
        {
            UIManager.Instance.OpenQuestionPopup();
        }
    }

    private void OnCenterBinCreated()
    {
        _isBinMoving = true;
        _trashController.InstantiateTrashWhenReady(_binController.CurrentBin);
    }

    private void OnAllTrashDestroyed()
    {
        ProgressBin();
    }

    private void OnTrashCreated()
    {
    }

    private void OnCenterBinReachedCenter()
    {
        _isBinMoving = false;
    }

    private void OnQuestionPopupClosed()
    {
        _currentGameState = GameState.WaitingBin;
    }

    private enum GameState
    {
        WaitingBin,
        SortingBin,
        Paused,
    }

    public void Fail()
    {
        UIManager.Instance.Fail();
    }
}