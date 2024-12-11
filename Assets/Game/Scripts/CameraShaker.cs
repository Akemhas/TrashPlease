using PrimeTween;
using UnityEngine;

public class CameraShaker : Singleton<CameraShaker>
{
    [SerializeField] private float _shakeStrength = 0.2f;
    [SerializeField] private float _shakeDuration = 0.15f;

    private Sequence _shakeTween;

    private Camera _mainCam;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _mainCam = Camera.main;
    }

    public void Shake(float shakeMultiplier = 1)
    {
        if (!_mainCam) _mainCam = Camera.main;
        
        if (_shakeTween.isAlive)
        {
            _shakeTween.Complete();
        }

        _shakeTween = Tween.ShakeCamera(_mainCam, _shakeStrength * shakeMultiplier, duration: _shakeDuration);
    }
}