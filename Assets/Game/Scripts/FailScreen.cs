using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FailScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _highestScore;
    [SerializeField] private TextMeshProUGUI _currentScore;
    [SerializeField] private RectTransform _overlay;
    [SerializeField] private CanvasGroup _overlayCanvasGroup;
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
        PrimeTween.Tween.Alpha(_overlayCanvasGroup, 0, 1, 0.5f);
        AudioManager.Instance.PlaySoundTrack(SoundTrackType.Lose);
    }

    private void OnRetryButtonClicked()
    {
        var highestScore = GameManager.HighestScore;
        PlayerPrefs.DeleteAll();
        GameManager.HighestScore = highestScore;

        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}