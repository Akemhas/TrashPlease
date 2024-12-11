using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PrimeTween;

public class Timer : MonoBehaviour
{
    [SerializeField] private Transform _panel;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _countdownTime;
    private float _elapsedTime;
    private readonly WaitForSeconds _oneSecondWait = new(1);
    private Tween _fillTween;
    private Coroutine _timerRoutine;

    private void Awake()
    {
        UpdateTimerText(TimeSpan.FromSeconds(_countdownTime));
    }

    public void StartTimer()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }

        if (!_panel.gameObject.activeSelf) OpenTimer();
        _fillTween.Stop();
        _timerRoutine = StartCoroutine(CountDownRoutine());
    }

    public void StopTimer()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }

        if (_panel.gameObject.activeSelf)
        {
            _fillTween.Stop();
            _timerRoutine = StartCoroutine(CountUpRoutine());
        }
    }

    private void OnDestroy()
    {
        _fillTween.Stop();
        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
        _timerRoutine = null;
    }

    private void UpdateTimerText(TimeSpan timeSpan)
    {
        _timerText.SetText(timeSpan.ToString(@"ss"));
    }

    private IEnumerator CountDownRoutine()
    {
        UpdateTimerText(TimeSpan.FromSeconds(_countdownTime - _elapsedTime));
        while (_elapsedTime < _countdownTime)
        {
            _elapsedTime += 1;
            _fillTween = Tween.UIFillAmount(_fillImage, _fillImage.fillAmount, _elapsedTime / _countdownTime, 1);
            yield return _oneSecondWait;
            if (_elapsedTime >= _countdownTime)
            {
                _elapsedTime = _countdownTime;
                UpdateTimerText(TimeSpan.FromSeconds(_countdownTime - _elapsedTime));
                GameManager.Instance.Fail();
                yield break;
            }

            UpdateTimerText(TimeSpan.FromSeconds(_countdownTime - _elapsedTime));
        }
    }

    private IEnumerator CountUpRoutine()
    {
        UpdateTimerText(TimeSpan.FromSeconds(_countdownTime - _elapsedTime));
        while (_elapsedTime > 0)
        {
            _elapsedTime -= 2;
            _fillTween = Tween.UIFillAmount(_fillImage, _fillImage.fillAmount, _elapsedTime / _countdownTime, 1, Ease.Linear);
            yield return _oneSecondWait;
            if (_elapsedTime <= 0)
            {
                _elapsedTime = 0;
            }

            var seconds = TimeSpan.FromSeconds(Mathf.Clamp(_countdownTime - _elapsedTime,0,_countdownTime)); 
            UpdateTimerText(seconds);
        }

        CloseTimer();
    }

    private void OpenTimer()
    {
        _panel.gameObject.SetActive(true);
        Tween.Scale(_panel, Vector3.zero, Vector3.one, .2f, Ease.OutBack);
    }

    private void CloseTimer()
    {
        Tween.Scale(_panel, Vector3.one, Vector3.zero, .2f, Ease.InBack)
            .OnComplete(() => _panel.gameObject.SetActive(false));
    }
}