using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Trash : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    internal string TrashAddress;
    public TrashSortType TrashSortType;

    [SerializeField] private Image _image;

    private Vector3 _startPosition;
    private Vector3 _diffVector;

    private void Awake()
    {
        _startPosition = transform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var pos = new Vector3(eventData.position.x, eventData.position.y);
        _diffVector = transform.position - pos;
        _image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var pos = new Vector3(eventData.position.x, eventData.position.y);
        transform.position = pos + _diffVector;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _diffVector = Vector3.zero;
        _image.raycastTarget = true;
    }

    public void SavePosition()
    {
        _startPosition = transform.position;
    }

    public void ReturnToStartPosition()
    {
        transform.position = _startPosition;
    }
}

public enum TrashSortType
{
    Brown,
    Yellow,
    Blue,
    Black,
}