using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Create BinFrequencyData", fileName = "BinFrequencyData", order = 0)]
public class BinFrequencyData : ScriptableObject
{
    public int LoopCounterNumber => _difficultyIntervals.Count > 0 ? _difficultyIntervals[^1].BinCounter : 0;
    [SerializeField, DelayedProperty, ValidateInput("Validate")] private List<DifficultyInterval> _difficultyIntervals;
    [NonSerialized] private readonly Dictionary<int, DifficultyInterval> _difficultyIntervalLookupTable = new();

    public (TrashSortType, int) GetSortTypeForBinIndex(int binIndexInLevel, IReadOnlyCollection<TrashSortType> allowedSortTypes = null, IReadOnlyDictionary<TrashSortType, float> biasLookup = null)
    {
        (TrashSortType, int) result = new(TrashSortType.Residual, 3);
        if (_difficultyIntervals.Count <= 0) return result;
        if (_difficultyIntervals.Count == 1)
        {
            result.Item1 = _difficultyIntervals[0].GetRandomSortType(allowedSortTypes, biasLookup);
            result.Item2 = _difficultyIntervals[0].TrashCount;
            return result;
        }

        var difficultyInterval = GetDifficultyInterval(binIndexInLevel);
        if (difficultyInterval != null)
        {
            result.Item1 = difficultyInterval.GetRandomSortType(allowedSortTypes, biasLookup);
            result.Item2 = difficultyInterval.TrashCount;
        }

        return result;
    }

    public float GetSpawnIntervalForBinIndex(int binIndexInLevel)
    {
        var difficultyInterval = GetDifficultyInterval(binIndexInLevel);
        return difficultyInterval?.SpawnInterval ?? 1.5f;
    }

    private DifficultyInterval GetDifficultyInterval(int binIndexInLevel)
    {
        if (_difficultyIntervalLookupTable.TryGetValue(binIndexInLevel, out var interval))
        {
            return interval;
        }

        if (_difficultyIntervals.Count == 0) return null;

        int underLimit = 0;
        DifficultyInterval lastInterval = _difficultyIntervals[^1];

        foreach (var difficultyInterval in _difficultyIntervals)
        {
            if (binIndexInLevel >= underLimit && binIndexInLevel < difficultyInterval.BinCounter)
            {
                _difficultyIntervalLookupTable.TryAdd(binIndexInLevel, difficultyInterval);
                return difficultyInterval;
            }

            underLimit = difficultyInterval.BinCounter;
        }

        _difficultyIntervalLookupTable.TryAdd(binIndexInLevel, lastInterval);
        return lastInterval;
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

        public TrashSortType GetRandomSortType(IReadOnlyCollection<TrashSortType> allowedSortTypes, IReadOnlyDictionary<TrashSortType, float> biasLookup)
        {
            HashSet<TrashSortType> allowedLookup = null;
            if (allowedSortTypes != null && allowedSortTypes.Count > 0)
            {
                allowedLookup = new HashSet<TrashSortType>(allowedSortTypes);
            }

            List<TrashProbability> eligibleProbabilities = TrashProbabilities;

            if (allowedLookup != null)
            {
                eligibleProbabilities = new List<TrashProbability>();
                foreach (var trashProbability in TrashProbabilities)
                {
                    if (allowedLookup.Contains(trashProbability.SortType))
                    {
                        eligibleProbabilities.Add(trashProbability);
                    }
                }
            }

            if (eligibleProbabilities.Count == 0)
            {
                eligibleProbabilities = TrashProbabilities;
            }

            float totalProbability = 0;

            foreach (var trashProbability in eligibleProbabilities)
            {
                float weight = trashProbability.Probability;
                if (biasLookup != null && biasLookup.TryGetValue(trashProbability.SortType, out var bias))
                {
                    weight += bias;
                }

                totalProbability += weight;
            }

            if (totalProbability <= 0)
            {
                Debug.LogError($"Couldn't Get Random Probability");
                return TrashSortType.Residual;
            }

            float randomProbability = Random.Range(0f, totalProbability);
            float probabilityCounter = 0;

            foreach (var trashProbability in eligibleProbabilities)
            {
                float weight = trashProbability.Probability;
                if (biasLookup != null && biasLookup.TryGetValue(trashProbability.SortType, out var bias))
                {
                    weight += bias;
                }

                probabilityCounter += weight;

                if (randomProbability <= probabilityCounter)
                {
                    return trashProbability.SortType;
                }
            }

            Debug.LogError($"Couldn't Get Random Probability");
            return TrashSortType.Residual;
        }

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
    }

    [Serializable]
    private class TrashProbability
    {
        public TrashSortType SortType;
        [PropertyRange(0, 100), Delayed] public int Probability;
    }
}
