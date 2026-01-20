using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using PrimeTween;

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
    [SerializeField, Required] public LevelManager _levelManager;

    private TrashSortType _currentSortType;
    private int _currentTrashCount;

    public bool IsScannerEmpty = true;

    private bool _isBinMoving;
    private bool _checking;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
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
        if (_levelManager)
        {
            _levelManager.AllLevelsCompleted += HandleAllLevelsCompleted;
            _levelManager.LevelProgressChanged += OnLevelProgressChanged;
            _levelManager.LevelStarted += OnLevelStarted;
            _levelManager.LevelCompleted += OnLevelCompleted;
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
        if (_levelManager)
        {
            _levelManager.AllLevelsCompleted -= HandleAllLevelsCompleted;
            _levelManager.LevelProgressChanged -= OnLevelProgressChanged;
            _levelManager.LevelStarted -= OnLevelStarted;
            _levelManager.LevelCompleted -= OnLevelCompleted;
        }
    }

    private void Start()
    {
        AudioManager.Instance.PlaySoundTrack(SoundTrackType.Gameplay);
        _topBinController.Initialize();
        _levelManager?.StartLevel();
        if (_levelManager != null && UIManager.Instance)
        {
            UIManager.Instance.SetLevelProgress(_levelManager.BinsSortedThisLevel, _levelManager.BinsToComplete());
        }
        CreateInitialBins();
        TemperatureManager.SetTemperature(0);
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
        _levelManager?.RegisterBinSorted();
        IsScannerEmpty = true;
        if (_currentGameState != GameState.Paused)
        {
            _currentGameState = GameState.WaitingBin;
        }
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

    private enum GameState
    {
        WaitingBin,
        SortingBin,
        Paused,
    }

    [Button]
    public void Fail()
    {
        Tween.StopAll();
        Time.timeScale = 0;
        UIManager.Instance.Fail();
    }

    private void HandleAllLevelsCompleted()
    {
        _currentGameState = GameState.Paused;
        Time.timeScale = 0;
        if (UIManager.Instance)
        {
            UIManager.Instance.Timer.StopTimer();
            UIManager.Instance.ShowAllLevelsComplete(OnRestartFromBeginning);
        }
    }

    private void OnLevelProgressChanged(int sorted, int total)
    {
        if (UIManager.Instance)
        {
            UIManager.Instance.SetLevelProgress(sorted, total);
        }
    }

    private void OnLevelStarted(int levelIndex)
    {
        if (UIManager.Instance)
        {
            UIManager.Instance.SetLevelProgress(0, _levelManager.BinsToComplete());
        }
        _topBinController.ResetForNewLevel();
        CreateInitialBins();
    }

    private void OnLevelCompleted(int levelIndex)
    {
        _currentGameState = GameState.Paused;
        if (UIManager.Instance)
        {
            UIManager.Instance.ShowLevelComplete(_levelManager.IsCurrentLevelLast, OnNextLevel, OnRestartLevel);
        }
    }

    private void CreateInitialBins()
    {
        if (_topBinController.TryCreateTopBin(5, out var topBinData))
        {
            _currentSortType = topBinData.Item1;
            _currentTrashCount = topBinData.Item2;
            _trashController.LoadTrash(_currentSortType, _currentTrashCount);
            _topBinController.TryCreateTopBin(7, out _);
            _currentGameState = GameState.WaitingBin;
        }
        else
        {
            HandleAllLevelsCompleted();
        }
    }

    private void OnNextLevel()
    {
        Time.timeScale = 1;
        if (_levelManager.TryAdvanceToNextLevel())
        {
            _currentGameState = GameState.WaitingBin;
        }
        else
        {
            HandleAllLevelsCompleted();
        }
    }

    private void OnRestartLevel()
    {
        Time.timeScale = 1;
        _levelManager.RestartCurrentLevel();
        _currentGameState = GameState.WaitingBin;
    }

    private void OnRestartFromBeginning()
    {
        Time.timeScale = 1;
        Score = 0;
        _levelManager.ResetProgress();
        _currentGameState = GameState.WaitingBin;
    }
}
