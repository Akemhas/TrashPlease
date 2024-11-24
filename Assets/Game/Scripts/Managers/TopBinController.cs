using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using PrimeTween;

public class TopBinController : Singleton<TopBinController>
{
    [SerializeField] private TopBinPool _topBinPool;
    [SerializeField, ChildGameObjectsOnly] private ConveyorBeltController _beltController;
    [SerializeField, Space] private float _spawnInterval;
    [SerializeField, ChildGameObjectsOnly] private Transform _startPoint, _endPoint;

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

    private void Awake()
    {
        _topBinPool.Initialize(_startPoint.position);
    }

    private void Start()
    {
        CreateTopBin();
    }

    private void Update()
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

    private void MoveBins()
    {
        int i = 1;

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
            Tween.PositionX(topBin.transform, beltPosition, duration).OnComplete(() => PlayingAnimationCount--);
            i++;
        }
    }

    private void CreateTopBin()
    {
        var sortType = GetRandomSortType();
        var topBin = _topBinPool.Get(sortType);
        _topBinQueue.Enqueue(topBin);
        _timeCapped = true;
        MoveBins();
    }

    [Button]
    private void TestBinDestroy()
    {
        var topBin = PopBin();
        Tween.CompleteAll(topBin);
        Destroy(topBin.gameObject);
        MoveBins();
    }

    private TopBin PopBin() => _topBinQueue.Dequeue();

    private TrashSortType GetRandomSortType()
    {
        var values = Enum.GetValues(typeof(TrashSortType));
        var randomIndex = Random.Range(0, values.Length);
        return (TrashSortType)values.GetValue(randomIndex);
    }
}