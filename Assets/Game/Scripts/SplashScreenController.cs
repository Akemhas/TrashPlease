using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private RectTransform _titleHolder;
    [SerializeField] private TextMeshProUGUI _fillTMP;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Button _playButton;
    [SerializeField] private Vector2 _titleHolderStartPos;
    [SerializeField] private Vector2 _titleHolderEndPos;

    private const int GameplaySceneIndex = 1;
    private const float FakeWaitTimer = 1.2f;
    private float _elapsedTime;

    private AsyncOperation _loadOperation;

    private void Awake()
    {
        _playButton.gameObject.SetActive(false);
        _fillImage.fillAmount = 0;
        _playButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySoundEffect(SoundEffectType.Click);
            _loadOperation.allowSceneActivation = true;
        });
    }

    private void Start()
    {
        AudioManager.Instance.PlaySoundTrack(SoundTrackType.MainMenu);
        Tween.UIAnchoredPosition(_titleHolder, _titleHolderStartPos, _titleHolderEndPos, .5f, Ease.OutBack);
        StartLoadingGameplayScene();
    }

    private void StartLoadingGameplayScene()
    {
        StartCoroutine(SceneLoadRoutine());
    }

    private IEnumerator SceneLoadRoutine()
    {
        _loadOperation = SceneManager.LoadSceneAsync(GameplaySceneIndex);
        if (_loadOperation == null) yield break;

        _loadOperation.allowSceneActivation = false;

        while (_loadOperation.progress < .9f || _elapsedTime < FakeWaitTimer)
        {
            float timeLeap = Random.Range(0.05f, 0.25f);
            _elapsedTime += timeLeap;
            yield return new WaitForSeconds(timeLeap);
            UpdateFillVisuals(_loadOperation.progress);
        }

        UpdateFillVisuals(1);
        _playButton.gameObject.SetActive(true);

        void OnComplete()
        {
            Tween.Scale(_playButton.transform, Vector3.one * 1.1f, .6f, Ease.InOutSine, -1, CycleMode.Yoyo);
        }

        Tween.Scale(_playButton.transform, Vector3.zero, Vector3.one, .4f, Ease.OutBack).OnComplete(OnComplete);
    }

    private void UpdateFillVisuals(float progress)
    {
        float fakeFill = _elapsedTime / FakeWaitTimer;
        var clampedFakeFill = Mathf.Min(fakeFill, 1);
        float realProgress = Mathf.InverseLerp(progress, .9f, 0f);
        float fakeRatio = .7f;
        float fillAmount = clampedFakeFill * fakeRatio + realProgress * (1 - fakeRatio);
        _fillImage.fillAmount = fillAmount;
        _fillTMP.SetText($"{fillAmount * 0x64:N0}");
    }
}