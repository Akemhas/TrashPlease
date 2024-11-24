using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using PrimeTween;

public class TopBinController : MonoBehaviour
{
    public event Action BinReachedEnd;

    [SerializeField] private TopBinPool _topBinPool;
    [SerializeField] private BinFrequencyData _binFrequencyData;
    [SerializeField, ChildGameObjectsOnly] private ConveyorBeltController _beltController;
    [SerializeField, Space] private float _spawnInterval;
    [SerializeField, ChildGameObjectsOnly] private Transform _startPoint;
    [SerializeField, ChildGameObjectsOnly] private Transform _scanner;

    private int BinCounter
    {
        get => PlayerPrefs.GetInt(nameof(BinCounter), 0);
        set => PlayerPrefs.SetInt(nameof(BinCounter), value % _binFrequencyData.LoopCounterNumber);
    }

    private readonly Queue<TopBin> _topBinQueue = new();

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
        if (_timeCapped && _elapsedTime < _spawnInterval)
        {
            _elapsedTime += Time.deltaTime;
            return;
        }

        _timeCapped = false;
        _elapsedTime = 0;

        if (_topBinQueue.Count >= _beltController.ConveyorBelts.Count) return;

        CreateTopBin();
    }


    public TopBin PopBin() => _topBinQueue.Dequeue();

    private void MoveBins()
    {
        int i = 1;

        var topMostBin = _topBinQueue.Peek();

        foreach (var topBin in _topBinQueue)
        {
            var belt = _beltController.ConveyorBelts[^i];
            var beltPosition = belt.transform.position.x;
            if (beltPosition - topBin.transform.position.x < .0001f)
            {
                i++;
                continue;
            }

            float iLerp = Mathf.InverseLerp(0, 4, beltPosition - topBin.transform.position.x);
            float duration = Mathf.Lerp(.8f, 1.5f, iLerp);
            Tween.StopAll(topBin);
            PlayingAnimationCount++;
            Tween.PositionX(topBin.transform, beltPosition, duration).OnComplete(() =>
            {
                if (topBin.GetInstanceID() == topMostBin.GetInstanceID())
                {
                    BinReachedEnd?.Invoke();
                }

                PlayingAnimationCount--;
            });
            i++;
        }
    }

    public void CreateTopBin()
    {
        var sortType = _binFrequencyData.GetSortType(BinCounter);
        BinCounter++;
        var topBin = _topBinPool.Get(sortType);
        _topBinQueue.Enqueue(topBin);
        _timeCapped = true;
        MoveBins();
    }
}