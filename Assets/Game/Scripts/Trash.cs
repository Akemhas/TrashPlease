using PrimeTween;
using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] private Collider2D _collider2D;
    public SpriteRenderer Renderer;
    internal string TrashAddress;

    public TrashSortType TrashSortType;
    private Vector3 _startPosition;
    private Vector3 _diffVector;
    private int _defaultSortingLayer;

    private readonly TweenSettings _ts = new(.2f, Ease.InOutCubic);

    private void Awake()
    {
        _startPosition = transform.position;
    }

    public void SavePosition()
    {
        Renderer.sortingOrder = _defaultSortingLayer;
        _startPosition = transform.position;
    }

    public void ReturnToStartPosition()
    {
        _collider2D.enabled = false;
        Tween.Position(transform, _startPosition, _ts).OnComplete(() => _collider2D.enabled = true);
        Renderer.sortingOrder = _defaultSortingLayer;
    }
}

public enum TrashSortType
{
    Brown,
    Yellow,
    Blue,
    Black,
}