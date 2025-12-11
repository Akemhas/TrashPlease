using PrimeTween;
using UnityEngine;

namespace Utilities
{
    public class RotateTween : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Vector3 _targetRotation;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private CycleMode _cycleMode;
        [SerializeField] private int _loopCount = -1;
        [SerializeField] private Ease _ease = Ease.Linear;

        private void OnEnable()
        {
            Tween.StopAll(_rectTransform);
            _rectTransform.eulerAngles = Vector3.zero;
            Tween.Rotation(_rectTransform, _targetRotation, _duration, _ease, _loopCount, _cycleMode);
        }
    }
}