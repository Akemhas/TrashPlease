using PrimeTween;
using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] private Collider2D _collider2D;
    internal string TrashAddress;

    public TrashSortType TrashSortType;
    private Vector3 _startPosition;

    private readonly TweenSettings _ts = new(.2f, Ease.InOutCubic);

    private void Awake()
    {
        _startPosition = transform.localPosition;
    }

    public void SavePosition()
    {
        _startPosition = transform.localPosition;
    }

    public void SetZPosition(float z)
    {
        var pos = transform.localPosition;
        pos.z = z;
        _startPosition.z = z;
        transform.localPosition = pos;
    }

    public void ReturnToStartPosition()
    {
        _collider2D.enabled = false;
        Tween.LocalPosition(transform, _startPosition, _ts).OnComplete(() => _collider2D.enabled = true);
    }

    public void ToggleCollider2D(bool enabled)
    {
        _collider2D.enabled = enabled;
    }
}

public enum TrashSortType
{
    Brown,
    Yellow,
    Blue,
    Black,
}