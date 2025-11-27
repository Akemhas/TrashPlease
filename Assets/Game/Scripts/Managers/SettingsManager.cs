using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        public static event Action<string> LanguageChanged;

        [SerializeField] private TextMeshProUGUI _titleTmp;
        [SerializeField] private GameObject _overlayPanel;
        [SerializeField] private GameObject _settingsPanel;

        [SerializeField] private Button _exitButton;
        [SerializeField] private Toggle _audioToggleButton;
        [SerializeField] private Button _deLanguageButton;
        [SerializeField] private Button _enLanguageButton;

        private void Awake()
        {
            _audioToggleButton.SetIsOnWithoutNotify(!AudioManager.Instance.MuteStatus);
            _audioToggleButton.onValueChanged.AddListener(OnAudioButtonClicked);
            _deLanguageButton.onClick.AddListener(OnDeLanguageButtonClicked);
            _enLanguageButton.onClick.AddListener(OnEnLanguageButtonClicked);
            _exitButton.onClick.AddListener(CloseSettingsMenu);
            LanguageChanged += OnLanguageChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LanguageChanged -= OnLanguageChanged;
        }

        private void OnEnLanguageButtonClicked() => LanguageChanged?.Invoke("en");

        private void OnDeLanguageButtonClicked() => LanguageChanged?.Invoke("de");

        private void OnAudioButtonClicked(bool isAudioOn) => AudioManager.Instance.ToggleMute();

        public void OpenSettingsMenu()
        {
            Time.timeScale = 0;
            _overlayPanel.SetActive(true);
            _settingsPanel.SetActive(true);
        }

        private void CloseSettingsMenu()
        {
            Time.timeScale = 1;
            _overlayPanel.SetActive(false);
            _settingsPanel.SetActive(false);
        }

        private void OnLanguageChanged(string languageCode) { }
    }
}