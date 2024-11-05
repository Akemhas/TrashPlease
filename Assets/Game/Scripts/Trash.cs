using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Trash : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TrashType TypeOfTrash;

    [SerializeField] private Image _image;

    public event Action<Trash> OnTrashPicked;
    public event Action<Trash, Vector2> OnTrashDropped;

    private Vector3 _startPosition;
    private Vector3 _diffVector;

    private void Awake()
    {
        _startPosition = transform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var pos = new Vector3(eventData.position.x, eventData.position.y);
        _startPosition = transform.position;
        _diffVector = transform.position - pos;
        _image.raycastTarget = false;
        OnTrashPicked?.Invoke(this);
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
        OnTrashDropped?.Invoke(this, eventData.position);
    }
}

public enum TrashType
{
    Brown,
    Yellow,
    Blue,
    Black,
}