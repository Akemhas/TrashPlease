using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Managers
{
    public class SettingsManager : MonoBehaviour
    {
        public static event Action<string> LanguageChanged;

        [SerializeField] private TextMeshProUGUI _titleTmp;
        [SerializeField] private GameObject _overlayPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _howToPlayPanel;

        [SerializeField] private Button _exitButton;
        [SerializeField] private Toggle _audioToggleButton;
        [SerializeField] private Button _deLanguageButton;
        [SerializeField] private Button _enLanguageButton;
        [SerializeField] private Button _howToPlayButton;

        private void Awake()
        {
            _audioToggleButton.SetIsOnWithoutNotify(!AudioManager.Instance.MuteStatus);
            _audioToggleButton.onValueChanged.AddListener(OnAudioButtonClicked);
            _deLanguageButton.onClick.AddListener(OnDeLanguageButtonClicked);
            _enLanguageButton.onClick.AddListener(OnEnLanguageButtonClicked);
            _exitButton.onClick.AddListener(CloseSettingsMenu);
            _howToPlayButton.onClick.AddListener(OpenHowToPlayPanel);
            LanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            LanguageChanged -= OnLanguageChanged;
            _audioToggleButton.onValueChanged.RemoveListener(OnAudioButtonClicked);
            _deLanguageButton.onClick.RemoveListener(OnDeLanguageButtonClicked);
            _enLanguageButton.onClick.RemoveListener(OnEnLanguageButtonClicked);
            _exitButton.onClick.RemoveListener(CloseSettingsMenu);
            _howToPlayButton.onClick.RemoveListener(OpenHowToPlayPanel);
        }

        private void OnEnLanguageButtonClicked() => LanguageChanged?.Invoke("en");

        private void OnDeLanguageButtonClicked() => LanguageChanged?.Invoke("de");

        private void OnAudioButtonClicked(bool isAudioOn) => AudioManager.Instance.ToggleMute();

        private void OpenHowToPlayPanel() => _howToPlayPanel.SetActive(true);

        public void OpenSettingsMenu()
        {
            Time.timeScale = 0;
            _overlayPanel.SetActive(true);
            _settingsPanel.SetActive(true);
            InputManager.SetInputPaused?.Invoke(true);
        }

        private void CloseSettingsMenu()
        {
            Time.timeScale = 1;
            _overlayPanel.SetActive(false);
            _settingsPanel.SetActive(false);
            InputManager.SetInputPaused?.Invoke(false);
        }

        private void OnLanguageChanged(string languageCode)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales.Find(r => r.CustomFormatterCode == languageCode);
            LocalizationSettings.SelectedLocale = locale;
        }
    }
}