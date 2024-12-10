using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FailScreen : MonoBehaviour
{
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
        PrimeTween.Tween.Alpha(_overlayCanvasGroup, 0, 1, 0.5f);
        AudioManager.Instance.PlaySoundTrack(SoundTrackType.Lose);
    }

    private void OnRetryButtonClicked()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}