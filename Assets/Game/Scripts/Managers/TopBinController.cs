using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using SerializableDictionary.Scripts;
using Random = UnityEngine.Random;
using PrimeTween;

public class TopBinController : Singleton<TopBinController>
{
    public SerializableDictionary<TrashSortType, TopBin> BinDictionary = new();

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
            float duration = Mathf.Lerp(.3f, 1.5f, iLerp);
            Tween.StopAll(topBin);
            PlayingAnimationCount++;
            Tween.PositionX(topBin.transform, beltPosition, duration).OnComplete(() => PlayingAnimationCount--);
            i++;
        }
    }

    private void CreateTopBin()
    {
        var sortType = GetRandomSortType();
        var topBin = InstantiateTopBin(sortType);
        _topBinQueue.Enqueue(topBin);
        _timeCapped = true;
        MoveBins();
    }

    private TopBin InstantiateTopBin(TrashSortType sortType)
    {
        return Instantiate(BinDictionary.Get(sortType), _startPoint.position, Quaternion.identity, transform);
    }

    [Button]
    private TopBin PopBin()
    {
        var bin = _topBinQueue.Dequeue();
        Destroy(bin.gameObject);
        return bin;
    }

    private TrashSortType GetRandomSortType()
    {
        var values = Enum.GetValues(typeof(TrashSortType));
        var randomIndex = Random.Range(0, values.Length);
        return (TrashSortType)values.GetValue(randomIndex);
    }
}