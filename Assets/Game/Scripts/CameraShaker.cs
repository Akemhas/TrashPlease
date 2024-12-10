using PrimeTween;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;

    [SerializeField] private float _shakeStrength = 0.2f;
    [SerializeField] private float _shakeDuration = 0.15f;

    private Sequence _shakeTween;

    private Camera _mainCam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _mainCam = Camera.main;
        DontDestroyOnLoad(gameObject);
    }

    public void Shake(float shakeMultiplier = 1)
    {
        if (_shakeTween.isAlive)
        {
            _shakeTween.Complete();
        }

        _shakeTween = Tween.ShakeCamera(_mainCam, _shakeStrength * shakeMultiplier, duration: _shakeDuration);
    }
}