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

    public TrashSortType GetSortType(int binCounter)
    {
        if (_difficultyIntervals.Count <= 0) return TrashSortType.Black;
        if (_difficultyIntervals.Count == 1)
        {
            return _difficultyIntervals[0].GetRandomSortType();
        }

        int underLimit = 0;

        for (var i = 0; i < _difficultyIntervals.Count; i++)
        {
            var difficultyInterval = _difficultyIntervals[i];

            if (binCounter >= underLimit && binCounter < difficultyInterval.BinCounter)
            {
                return difficultyInterval.GetRandomSortType();
            }

            underLimit = difficultyInterval.BinCounter;
        }

        Debug.LogError($"Couldn't Get Random Probability");
        return TrashSortType.Black;
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