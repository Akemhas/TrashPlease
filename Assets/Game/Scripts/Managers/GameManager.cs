using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : Singleton<GameManager>
{
    [ReadOnly] private GameState _currentGameState;

    [SerializeField] private BinController _binController;
    [SerializeField] private TopBinController _topBinController;
    [SerializeField] private TrashController _trashController;

    private TrashSortType _currentSortType;

    public bool IsScannerEmpty = true;

    private void Awake()
    {
        _currentGameState = GameState.WaitingBin;
    }

    private void OnEnable()
    {
        _topBinController.BinReachedEnd += OnBinReachedEnd;
        _binController.BinCreated += OnBinCreated;
        _binController.BinBeforeDestroy += OnBinBeforeDestroy;
    }

    private void OnDisable()
    {
        _topBinController.BinReachedEnd -= OnBinReachedEnd;
        _binController.BinCreated -= OnBinCreated;
    }

    private void Start()
    {
        _topBinController.Initialize();
        var sortType = _topBinController.CreateTopBin();
        _trashController.LoadTrash(sortType);
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
                    OnBinReachedEnd(topBin.SortType);
                }
                break;
        }


        _topBinController.Tick();
    }

    public void ProgressBin()
    {
        if (_trashController.CheckTrashSorting(_currentSortType))
        {
            _binController.DestroyBin();
        }
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

    private void OnBinReachedEnd(TrashSortType sortType)
    {
        if (_currentGameState == GameState.SortingBin) return;

        _currentGameState = GameState.SortingBin;
        _currentSortType = sortType;
        IsScannerEmpty = false;
        _topBinController.SendBinToScanner().GetAwaiter().OnCompleted(OnBinReachedToScanner);
    }

    private void OnBinBeforeDestroy()
    {
        IsScannerEmpty = true;
        _currentGameState = GameState.WaitingBin;
        _trashController.DestroyAllTrash();
        _trashController.LoadTrash(_currentSortType);
    }

    private void OnBinReachedToScanner()
    {
        _binController.StartCreatingBin(_currentSortType);
    }

    private void OnBinCreated()
    {
        _trashController.InstantiateTrashWhenReady(_binController.CurrentBin);
    }

    private enum GameState
    {
        WaitingBin,
        SortingBin,
        Paused,
    }
}