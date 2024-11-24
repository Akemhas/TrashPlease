using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    [ReadOnly] private GameState _currentGameState;

    [SerializeField] private BinController _binController;
    [SerializeField] private TopBinController _topBinController;
    [SerializeField] private TrashController _trashController;

    public bool IsScannerEmpty = true;

    private void Awake()
    {
        _currentGameState = GameState.WaitingBin;
    }

    private void OnEnable()
    {
        _topBinController.BinReachedEnd += OnBinReachedEnd;
    }

    private void OnDisable()
    {
        _topBinController.BinReachedEnd -= OnBinReachedEnd;
    }

    private void Start()
    {
        _topBinController.Initialize();
        _topBinController.CreateTopBin();
    }

    private void Update()
    {
        if (_currentGameState == GameState.Paused) return;

        _topBinController.Tick();
    }

    private void OnBinReachedEnd()
    {
        if (_currentGameState == GameState.SortingBin) return;

        _currentGameState = GameState.SortingBin;
        IsScannerEmpty = false;
        var bin = _topBinController.PopBin();
        _binController.StartCreatingBin(bin.SortType);
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

    private enum GameState
    {
        WaitingBin,
        SortingBin,
        Paused,
    }
}