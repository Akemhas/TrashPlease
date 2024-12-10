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
    [SerializeField, ChildGameObjectsOnly] private ConveyorBeltController _beltController;
    [SerializeField, ChildGameObjectsOnly] private Transform _startPoint;
    [SerializeField, ChildGameObjectsOnly] private Transform _scanner;

    private int BinCounter
    {
        get => PlayerPrefs.GetInt(nameof(BinCounter), 0);
        set => PlayerPrefs.SetInt(nameof(BinCounter), value % _binFrequencyData.LoopCounterNumber);
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

    public void Initialize()
    {
        _topBinPool.Initialize(_startPoint.position);
    }

    public void Tick()
    {
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
        var bin = _topBinQueue.Dequeue();
        UIManager.Instance.Timer.StopTimer();

        try
        {
            var distance = _scanner.position.x - bin.transform.position.x;
            await Tween.PositionX(bin.transform, _scanner.position.x, distance / 2, Ease.Linear);
            MoveBins();
            _topBinPool.Release(bin);
            BinCounter++;
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

        var topMostBin = _topBinQueue.Peek();

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
            Tween.PositionX(topBin.transform, beltPosition, distance / 2, ease: Ease.Linear).OnComplete(() =>
            {
                if (topBin.GetInstanceID() == topMostBin.GetInstanceID())
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
        var sortTypeTrashCount = _binFrequencyData.GetSortType(BinCounter);
        _spawnInterval = _binFrequencyData.GetSpawnInterval(BinCounter);

        var topBin = _topBinPool.Get(sortTypeTrashCount.Item1);
        topBin.TrashCount = sortTypeTrashCount.Item2;
        _topBinQueue.Enqueue(topBin);
        _timeCapped = true;

        if (_topBinQueue.Count >= _beltController.ConveyorBelts.Count)
        {
            UIManager.Instance.Timer.StartTimer();
        }

        MoveBins();
    }

    public (TrashSortType, int) CreateTopBin(int index)
    {
        var sortTypeTrashCount = _binFrequencyData.GetSortType(BinCounter);
        var topBin = _topBinPool.Get(sortTypeTrashCount.Item1);
        topBin.TrashCount = sortTypeTrashCount.Item2;
        _spawnInterval = _binFrequencyData.GetSpawnInterval(BinCounter);

        var pos = _beltController.ConveyorBelts[^index].transform.position;
        pos.y = _startPoint.position.y;
        topBin.transform.position = pos;

        _topBinQueue.Enqueue(topBin);
        _timeCapped = true;
        MoveBins();

        return new(topBin.SortType, topBin.TrashCount);
    }
}