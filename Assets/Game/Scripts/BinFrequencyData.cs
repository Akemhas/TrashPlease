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

    public (TrashSortType, int) GetSortType(int binCounter)
    {
        (TrashSortType, int) result = new(TrashSortType.Black, 3);
        if (_difficultyIntervals.Count <= 0) return result;
        if (_difficultyIntervals.Count == 1)
        {
            result.Item1 = _difficultyIntervals[0].GetRandomSortType();
            result.Item2 = _difficultyIntervals[0].TrashCount;
            return result;
        }

        int underLimit = 0;

        for (var i = 0; i < _difficultyIntervals.Count; i++)
        {
            var difficultyInterval = _difficultyIntervals[i];

            if (binCounter >= underLimit && binCounter < difficultyInterval.BinCounter)
            {
                result.Item1 = difficultyInterval.GetRandomSortType();
                result.Item2 = difficultyInterval.TrashCount;
                return result;
            }

            underLimit = difficultyInterval.BinCounter;
        }

        Debug.LogError($"Couldn't Get Random Probability");
        return result;
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
            return TrashSortType.Black;
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
        [PropertyRange(0, 100)] public int Probability;
    }
}