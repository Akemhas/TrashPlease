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

    private bool _isBinMoving;

    private void Awake()
    {
        _currentGameState = GameState.WaitingBin;
    }

    private void OnEnable()
    {
        _topBinController.BinReachedEnd += OnTopBinReachedEnd;
        _binController.BinCreated += OnCenterBinCreated;
        _binController.BinBeforeDestroy += OnCenterBinBeforeDestroy;
        _binController.BinReachedCenter += OnCenterBinReachedCenter;
        _trashController.TrashCreated += OnTrashCreated;
    }

    private void OnDisable()
    {
        _topBinController.BinReachedEnd -= OnTopBinReachedEnd;
        _binController.BinCreated -= OnCenterBinCreated;
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
                    OnTopBinReachedEnd(topBin.SortType);
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

        if (!_trashController.CheckTrashSorting(_currentSortType)) return;

        _isBinMoving = true;
        _binController.DestroyBin();
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

    private void OnTopBinReachedEnd(TrashSortType sortType)
    {
        if (_currentGameState == GameState.SortingBin) return;

        _currentGameState = GameState.SortingBin;
        _currentSortType = sortType;
        IsScannerEmpty = false;
        _topBinController.SendBinToScanner().GetAwaiter().OnCompleted(OnTopBinReachedToScanner);
    }

    private void OnCenterBinBeforeDestroy()
    {
        IsScannerEmpty = true;
        _currentGameState = GameState.WaitingBin;
        _trashController.DestroyAllTrash();
        _trashController.LoadTrash(_currentSortType);
    }

    private void OnTopBinReachedToScanner()
    {
        if (_currentSortType != TrashSortType.Question)
        {
            _binController.StartCreatingBin(_currentSortType);
        }
    }

    private void OnCenterBinCreated()
    {
        _isBinMoving = true;
        _trashController.InstantiateTrashWhenReady(_binController.CurrentBin);
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
}