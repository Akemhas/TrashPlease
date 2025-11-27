using Managers;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private Button _settingsButton;

    private void Awake()
    {
        _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    private void OnSettingsButtonClicked()
    {
        SettingsManager.Instance.OpenSettingsMenu();
    }
}