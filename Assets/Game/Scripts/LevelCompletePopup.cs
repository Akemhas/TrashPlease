using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletePopup : MonoBehaviour
{
    [SerializeField] private GameObject _panel, _overlay;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _currentScoreText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _restartButton;

    private Action _nextAction;
    private Action _restartAction;

    private void Awake()
    {
        if (_panel != null) _panel.SetActive(false);
        if (_overlay != null) _overlay.SetActive(false);
        if (_nextButton != null) _nextButton.onClick.AddListener(OnNext);
        if (_restartButton != null) _restartButton.onClick.AddListener(OnRestart);
    }

    public void Open(bool isLastLevel, Action nextLevel, Action restartLevel)
    {
        _nextAction = nextLevel;
        _restartAction = restartLevel;

        if (SettingsManager.ActiveLanguage == "de")
        {
            _titleText.SetText(isLastLevel ? "Letztes Level abgeschlossen!" : "Level abgeschlossen!");
            _currentScoreText.text = "Aktueller Punktestand: " + GameManager.Score;
        }
        else
        {
            _titleText.SetText(isLastLevel ? "Final Level Complete!" : "Level Complete!");
            _currentScoreText.text = "Current Score: " + GameManager.Score;
        }

        if (_nextButton != null) _nextButton.gameObject.SetActive(!isLastLevel);
        if (_panel != null) _panel.SetActive(true);
        if (_overlay != null) _overlay.SetActive(true);
    }

    private void OnNext()
    {
        _panel?.SetActive(false);
        _overlay?.SetActive(false);
        _nextAction?.Invoke();
    }

    private void OnRestart()
    {
        _panel?.SetActive(false);
        _overlay?.SetActive(false);
        _restartAction?.Invoke();
    }
}