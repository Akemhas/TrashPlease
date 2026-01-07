using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using PrimeTween;

public class TopBinController : MonoBehaviour
{
    public event Action<TopBin> BinReachedEnd;

    [SerializeField] private TopBinPool _topBinPool;
    [SerializeField] private BinFrequencyData _binFrequencyData;
    [SerializeField, Required] private LevelManager _levelManager;
    [SerializeField, ChildGameObjectsOnly] private ConveyorBeltController _beltController;
    [SerializeField, ChildGameObjectsOnly] private Transform _startPoint;
    [SerializeField, ChildGameObjectsOnly] private Transform _scanner;

    public static int BinCounter
    {
        get => PlayerPrefs.GetInt(nameof(BinCounter), 0);
        set => PlayerPrefs.SetInt(nameof(BinCounter), Mathf.Max(0, value));
    }

    private readonly Queue<TopBin> _topBinQueue = new();

    private float _spawnInterval;
    private float _elapsedTime;
    private bool _timeCapped;

    private int _playingAnimationCount;

    private int PlayingAnimationCount
    {
        get => _playingAnimationCount;
        set
        {
            if (_playingAnimationCount == 0 && value > 0)
            {
                _beltController.ToggleBeltAnimations(true);
            }

            if (value == 0)
            {
                _beltController.ToggleBeltAnimations(false);
            }

            _playingAnimationCount = value;
        }
    }

    private void OnEnable()
    {
        if (_levelManager)
        {
            _levelManager.LevelStarted += OnLevelStarted;
        }
    }

    private void OnDisable()
    {
        if (_levelManager)
        {
            _levelManager.LevelStarted -= OnLevelStarted;
        }
    }

    public void Initialize()
    {
        _topBinPool.Initialize(_startPoint.position);
    }

    public void Tick()
    {
        if (_levelManager != null && !_levelManager.CanSpawnBin()) return;
        if (_topBinQueue.Count >= _beltController.ConveyorBelts.Count) return;

        if (_timeCapped && _elapsedTime < _spawnInterval)
        {
            _elapsedTime += Time.deltaTime;
            return;
        }

        _timeCapped = false;
        _elapsedTime = 0;

        CreateTopBin();
    }

    public async Awaitable SendBinToScanner()
    {
        if (_topBinQueue.Count == 0) return;

        var bin = _topBinQueue.Dequeue();
        UIManager.Instance.Timer.StopTimer();

        try
        {
            var distance = _scanner.position.x - bin.transform.position.x;
            await Tween.PositionX(bin.transform, _scanner.position.x, distance / 2, Ease.Linear);
            MoveBins();
            _topBinPool.Release(bin);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void MoveBins()
    {
        int i = 1;

        if (!_topBinQueue.TryPeek(out var topMostBin))
        {
            if (_spawnInterval - _elapsedTime > 3)
            {
                _elapsedTime = _spawnInterval - 3;
            }

            return;
        }

        foreach (var topBin in _topBinQueue)
        {
            var belt = _beltController.ConveyorBelts[^i];
            var beltPosition = belt.transform.position.x;
            var distance = beltPosition - topBin.transform.position.x;
            if (distance < .0001f)
            {
                i++;
                continue;
            }

            Tween.StopAll(topBin);
            PlayingAnimationCount++;
            Tween.PositionX(topBin.transform, beltPosition, distance / 3, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    if (topBin == topMostBin)
                    {
                        BinReachedEnd?.Invoke(topBin);
                    }

                    PlayingAnimationCount--;
                });
            i++;
        }
    }

    public TopBin Peek() => _topBinQueue.Peek();

    public bool TryPeek(out TopBin result) => _topBinQueue.TryPeek(out result);

    public void CreateTopBin()
    {
        if (!CanSpawnMoreBins()) return;

        var frequencyData = CurrentFrequencyData();
        if (frequencyData == null) return;

        int binIndexInLevel = _levelManager != null ? _levelManager.CurrentBinIndexInLevel : BinCounter;

        var sortTypeTrashCount = frequencyData.GetSortTypeForBinIndex(binIndexInLevel, GetAllowedSorts(), GetBiasLookup());
        _spawnInterval = frequencyData.GetSpawnIntervalForBinIndex(binIndexInLevel);

        var topBin = _topBinPool.Get(sortTypeTrashCount.Item1);
        topBin.TrashCount = sortTypeTrashCount.Item2;
        _topBinQueue.Enqueue(topBin);
        _timeCapped = true;
        _elapsedTime = 0;
        _levelManager?.RegisterBinSpawned();

        // if (_topBinQueue.Count >= _beltController.ConveyorBelts.Count)
        // {
        //     UIManager.Instance.Timer.StartTimer();
        // }

        MoveBins();
    }

    public bool TryCreateTopBin(int index, out (TrashSortType, int) createdBinData)
    {
        createdBinData = default;

        if (!CanSpawnMoreBins()) return false;

        var frequencyData = CurrentFrequencyData();
        if (frequencyData == null) return false;

        int binIndexInLevel = _levelManager != null ? _levelManager.CurrentBinIndexInLevel : BinCounter;

        var sortTypeTrashCount = frequencyData.GetSortTypeForBinIndex(binIndexInLevel, GetAllowedSorts(), GetBiasLookup());
        var topBin = _topBinPool.Get(sortTypeTrashCount.Item1);
        topBin.TrashCount = sortTypeTrashCount.Item2;
        _spawnInterval = frequencyData.GetSpawnIntervalForBinIndex(binIndexInLevel);

        var pos = _beltController.ConveyorBelts[^index].transform.position;
        pos.y = _startPoint.position.y;
        topBin.transform.position = pos;

        _topBinQueue.Enqueue(topBin);
        _timeCapped = true;
        _levelManager?.RegisterBinSpawned();
        MoveBins();

        createdBinData = new(topBin.SortType, topBin.TrashCount);
        return true;
    }

    private void OnLevelStarted(int levelIndex)
    {
        _elapsedTime = 0;
        _timeCapped = false;
    }

    public void ResetForNewLevel()
    {
        while (_topBinQueue.Count > 0)
        {
            var bin = _topBinQueue.Dequeue();
            _topBinPool.Release(bin);
        }

        _elapsedTime = 0;
        _timeCapped = false;
    }

    private IReadOnlyCollection<TrashSortType> GetAllowedSorts()
    {
        if (_levelManager == null) return null;

        var unlockedBins = _levelManager.GetUnlockedBins();
        return unlockedBins.Count > 0 ? unlockedBins : null;
    }

    private bool CanSpawnMoreBins()
    {
        if (_levelManager == null) return true;
        return _levelManager.CanSpawnBin();
    }

    private BinFrequencyData CurrentFrequencyData()
    {
        if (_levelManager != null)
        {
            var data = _levelManager.GetBinFrequencyData();
            if (data != null) return data;
        }

        return _binFrequencyData;
    }

    private IReadOnlyDictionary<TrashSortType, float> GetBiasLookup()
    {
        if (_levelManager == null) return null;
        return _levelManager.GetBiasLookup();
    }
}