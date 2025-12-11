using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllLevelsCompletePopup : MonoBehaviour
{
    [SerializeField] private GameObject _panel, _overlay;
    [SerializeField] private TextMeshProUGUI _highestScore;
    [SerializeField] private TextMeshProUGUI _currentScore;
    [SerializeField] private Button _restartButton;

    private Action _restartAction;

    private void Awake()
    {
        if (_panel != null) _panel.SetActive(false);
        if (_overlay != null) _overlay.SetActive(false);
        if (_restartButton != null) _restartButton.onClick.AddListener(OnRestart);
    }

    public void Open(Action restartFromBeginning)
    {
        _restartAction = restartFromBeginning;
        _currentScore.SetText($"Current Score\n{GameManager.Score}");
        _highestScore.SetText($"Highest Score\n{GameManager.HighestScore}");
        _panel?.SetActive(true);
        _overlay?.SetActive(true);
    }

    private void OnRestart()
    {
        _panel?.SetActive(false);
        _overlay?.SetActive(false);
        _restartAction?.Invoke();
    }
}