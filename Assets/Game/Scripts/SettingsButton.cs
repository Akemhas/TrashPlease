using Managers;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private Button _settingsButton;
    [SerializeField] private SettingsManager _settingsManager;

    private void Awake()
    {
        _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    private void OnDestroy()
    {
        _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
    }

    private void OnSettingsButtonClicked()
    {
        _settingsManager.OpenSettingsMenu();
    }
}