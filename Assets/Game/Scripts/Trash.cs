using PrimeTween;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Trash : MonoBehaviour
{
    public TrashData Data;
    [SerializeField] private BoxCollider2D _boxCollider;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public TrashSortType TrashSortType => Data.SortType;
    private Vector3 _startPosition;

    private readonly TweenSettings _ts = new(.2f, Ease.InOutCubic);

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Data && string.IsNullOrEmpty(Data.Address))
        {
            Undo.RecordObject(Data,$"Data {Data} Address Change");
            Data.Address = name;
            PrefabUtility.RecordPrefabInstancePropertyModifications(Data);
        }
        if (_boxCollider != null && _spriteRenderer != null)
        {
            var size = _spriteRenderer.sprite.bounds.size;
            if (_boxCollider.size == new Vector2(size.x, size.y)) return;
            
            // Update the BoxCollider2D size to match the sprite's bounds
            _boxCollider.size = size;
            _boxCollider.offset = _spriteRenderer.sprite.bounds.center;
        }
    }
#endif

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
        _boxCollider.enabled = false;
        Tween.LocalPosition(transform, _startPosition, _ts).OnComplete(() => _boxCollider.enabled = true);
    }

    public void ToggleCollider2D(bool enabled)
    {
        _boxCollider.enabled = enabled;
    }
}

public enum TrashSortType
{
    Brown,
    Yellow,
    Blue,
    Black,
}