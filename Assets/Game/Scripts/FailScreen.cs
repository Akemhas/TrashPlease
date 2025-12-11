using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FailScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _highestScore;
    [SerializeField] private TextMeshProUGUI _currentScore;
    [SerializeField] private RectTransform _overlay;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private Button _retryButton;

    private void Awake()
    {
        _retryButton.onClick.AddListener(OnRetryButtonClicked);
    }

    public void Open()
    {
        _overlay.gameObject.SetActive(true);
        _panel.gameObject.SetActive(true);
        _currentScore.SetText($"Current Score\n{GameManager.Score}");
        _highestScore.SetText($"Highest Score\n{GameManager.HighestScore}");
        AudioManager.Instance.PlaySoundTrack(SoundTrackType.Lose);
    }

    private void OnRetryButtonClicked()
    {
        var highestScore = GameManager.HighestScore;
        PlayerPrefs.DeleteAll();
        GameManager.HighestScore = highestScore;

        var a = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        a!.completed += _ => { Time.timeScale = 1; };
    }
}