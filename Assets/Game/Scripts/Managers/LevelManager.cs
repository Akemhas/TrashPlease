using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public event Action<int> LevelStarted;
    public event Action<int> LevelCompleted;
    public event Action AllLevelsCompleted;
    public event Action<int, int> LevelProgressChanged;

    [SerializeField] private BinFrequencyData _fallbackBinFrequencyData;
    [SerializeField, Min(1)] private int _fallbackBinsPerLevel = 10;
    [SerializeField] private List<TrashSortType> _fallbackUnlockedBins;
    [SerializeField] private List<PlayerBin> _playerBins;
    [SerializeField] private List<LevelDefinition> _levels;

    private int _currentLevelIndex;
    private int _binsSpawnedThisLevel;
    private int _binsSortedThisLevel;
    private bool _allLevelsFinished;
    private readonly Dictionary<TrashSortType, float> _runtimeBias = new();

    private const string LevelIndexKey = "LevelIndex";

    public bool AreAllLevelsFinished => _allLevelsFinished;
    public int CurrentLevelIndex => _currentLevelIndex;
    public int CurrentBinIndexInLevel => _binsSpawnedThisLevel;
    public int BinsSortedThisLevel => _binsSortedThisLevel;
    public bool IsCurrentLevelLast => _levels.Count == 0 || _currentLevelIndex >= _levels.Count - 1;

    private void Awake()
    {
        _currentLevelIndex = Mathf.Clamp(PlayerPrefs.GetInt(LevelIndexKey, 0), 0, Mathf.Max(0, _levels.Count - 1));
    }

    public void StartLevel()
    {
        TopBinController.BinCounter = 0;
        _binsSpawnedThisLevel = 0;
        _binsSortedThisLevel = 0;
        _allLevelsFinished = false;
        _runtimeBias.Clear();
        ApplyPlayerBins();
        LevelStarted?.Invoke(_currentLevelIndex);
        LevelProgressChanged?.Invoke(_binsSortedThisLevel, BinsToComplete());
    }

    public IReadOnlyList<TrashSortType> GetUnlockedBins()
    {
        if (_levels.Count > 0 && _currentLevelIndex < _levels.Count)
        {
            var enabledBins = _levels[_currentLevelIndex].EnabledBins;
            if (enabledBins != null && enabledBins.Count > 0)
            {
                return enabledBins;
            }
        }

        if (_fallbackUnlockedBins != null && _fallbackUnlockedBins.Count > 0) return _fallbackUnlockedBins;

        return Array.Empty<TrashSortType>();
    }

    public int BinsToComplete()
    {
        if (_levels.Count > 0 && _currentLevelIndex < _levels.Count)
        {
            return Mathf.Max(1, _levels[_currentLevelIndex].BinsToComplete);
        }

        return _fallbackBinsPerLevel;
    }

    public bool CanSpawnBin()
    {
        if (_allLevelsFinished) return false;
        return _binsSpawnedThisLevel < BinsToComplete();
    }

    public void RegisterBinSpawned()
    {
        if (_allLevelsFinished) return;
        _binsSpawnedThisLevel = Mathf.Clamp(_binsSpawnedThisLevel + 1, 0, int.MaxValue);
    }

    public bool RegisterBinSorted()
    {
        if (_allLevelsFinished) return false;

        _binsSortedThisLevel++;
        DecayBias();
        LevelProgressChanged?.Invoke(_binsSortedThisLevel, BinsToComplete());
        if (_binsSortedThisLevel < BinsToComplete()) return false;

        LevelCompleted?.Invoke(_currentLevelIndex);
        return true;
    }

    private void AdvanceLevel()
    {
        if (_levels.Count == 0 || _currentLevelIndex + 1 >= _levels.Count)
        {
            ApplyPlayerBins();
            _allLevelsFinished = true;
            AllLevelsCompleted?.Invoke();
            return;
        }

        _currentLevelIndex++;
        PlayerPrefs.SetInt(LevelIndexKey, _currentLevelIndex);
        StartLevel();
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(LevelIndexKey, 0);
        _currentLevelIndex = 0;
        _allLevelsFinished = false;
        StartLevel();
    }

    public BinFrequencyData GetBinFrequencyData()
    {
        if (_levels.Count > 0 && _currentLevelIndex < _levels.Count)
        {
            var overrideData = _levels[_currentLevelIndex].BinFrequencyOverride;
            if (overrideData != null) return overrideData;
        }

        return _fallbackBinFrequencyData;
    }

    public IReadOnlyDictionary<TrashSortType, float> GetBiasLookup() => _runtimeBias;

    public void AddMistakeBias(TrashSortType sortType, float weight = 5f)
    {
        if (weight <= 0) return;
        if (_runtimeBias.ContainsKey(sortType))
        {
            _runtimeBias[sortType] += weight;
        }
        else
        {
            _runtimeBias[sortType] = weight;
        }
    }

    public void RestartCurrentLevel()
    {
        _allLevelsFinished = false;
        StartLevel();
    }

    public bool TryAdvanceToNextLevel()
    {
        if (IsCurrentLevelLast)
        {
            MarkAllLevelsFinished();
            return false;
        }

        _currentLevelIndex++;
        PlayerPrefs.SetInt(LevelIndexKey, _currentLevelIndex);
        StartLevel();
        return true;
    }

    public void MarkAllLevelsFinished()
    {
        _allLevelsFinished = true;
        AllLevelsCompleted?.Invoke();
    }

    private void ApplyPlayerBins()
    {
        if (_playerBins == null || _playerBins.Count == 0) return;

        var allowed = GetUnlockedBins();
        var lookup = new HashSet<TrashSortType>(allowed);

        foreach (var playerBin in _playerBins)
        {
            if (playerBin == null) continue;
            bool enable = lookup.Contains(playerBin.BinTrashSortType);
            if (playerBin.gameObject.activeSelf != enable)
            {
                playerBin.gameObject.SetActive(enable);
            }
        }
    }

    private void DecayBias()
    {
        if (_runtimeBias.Count == 0) return;
        const float decayFactor = 0.8f;
        var keysSnapshot = new List<TrashSortType>(_runtimeBias.Keys);
        for (int i = 0; i < keysSnapshot.Count; i++)
        {
            var key = keysSnapshot[i];
            _runtimeBias[key] *= decayFactor;
            if (_runtimeBias[key] < 0.1f)
            {
                _runtimeBias.Remove(key);
            }
        }
    }

    [Serializable]
    private class LevelDefinition
    {
        [Min(1)] public int BinsToComplete = 10;
        public List<TrashSortType> EnabledBins = new();
        public BinFrequencyData BinFrequencyOverride;
    }
}
