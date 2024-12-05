using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Create BinFrequencyData", fileName = "BinFrequencyData", order = 0)]
public class BinFrequencyData : ScriptableObject
{
    public int LoopCounterNumber => _difficultyIntervals[^1].BinCounter;
    [SerializeField, DelayedProperty, ValidateInput("Validate")] private List<DifficultyInterval> _difficultyIntervals;
    [NonSerialized] private readonly Dictionary<int, DifficultyInterval> _difficultyIntervalLookupTable = new();

    public (TrashSortType, int) GetSortType(int binCounter)
    {
        (TrashSortType, int) result = new(TrashSortType.Residual, 3);
        if (_difficultyIntervals.Count <= 0) return result;
        if (_difficultyIntervals.Count == 1)
        {
            result.Item1 = _difficultyIntervals[0].GetRandomSortType();
            result.Item2 = _difficultyIntervals[0].TrashCount;
            return result;
        }

        var difficultyInterval = GetDifficultyInterval(binCounter);
        result.Item1 = difficultyInterval.GetRandomSortType();
        result.Item2 = difficultyInterval.TrashCount;

        return result;
    }

    public float GetSpawnInterval(int binCounter)
    {
        var difficultyInterval = GetDifficultyInterval(binCounter);
        return difficultyInterval.SpawnInterval;
    }

    private DifficultyInterval GetDifficultyInterval(int binCounter)
    {
        if (_difficultyIntervalLookupTable.ContainsKey(binCounter))
        {
            return _difficultyIntervalLookupTable[binCounter];
        }

        int underLimit = 0;

        foreach (var difficultyInterval in _difficultyIntervals)
        {
            if (binCounter >= underLimit && binCounter < difficultyInterval.BinCounter)
            {
                _difficultyIntervalLookupTable.TryAdd(binCounter, difficultyInterval);
                return difficultyInterval;
            }

            underLimit = difficultyInterval.BinCounter;
        }

        Debug.LogError($"Couldn't find the Difficulty interval for {binCounter}");
        return null;
    }

    private bool Validate()
    {
        if (_difficultyIntervals.Count <= 0) return false;

        var binCounter = _difficultyIntervals[0].BinCounter;

        bool hasIssue = false;

        foreach (var difficultyInterval in _difficultyIntervals)
        {
            if (difficultyInterval.BinCounter < binCounter)
            {
                difficultyInterval.BinCounter = binCounter + 1;
            }

            if (!difficultyInterval.Validate())
            {
                Debug.LogError($"Probability of {difficultyInterval} is not summing up to 100", this);
                hasIssue = true;
            }
        }

        return !hasIssue;
    }

    [Serializable]
    private class DifficultyInterval
    {
        public int BinCounter;
        public int TrashCount = 3;
        public float SpawnInterval = 1.5f;

        [Delayed] public List<TrashProbability> TrashProbabilities;

        public TrashSortType GetRandomSortType()
        {
            int randomProbability = Random.Range(0, 101);
            int totalProbability = 0;

            foreach (var trashProbability in TrashProbabilities)
            {
                totalProbability += trashProbability.Probability;

                if (randomProbability <= totalProbability)
                {
                    return trashProbability.SortType;
                }
            }

            Debug.LogError($"Couldn't Get Random Probability");
            return TrashSortType.Residual;
        }

#if UNITY_EDITOR
        public bool Validate()
        {
            int totalProbability = 0;

            foreach (var trashProbability in TrashProbabilities)
            {
                if (trashProbability == null) continue;
                totalProbability += trashProbability.Probability;
            }

            return totalProbability == 100;
        }
#endif
    }

    [Serializable]
    private class TrashProbability
    {
        public TrashSortType SortType;
        [PropertyRange(0, 100), Delayed] public int Probability;
    }
}