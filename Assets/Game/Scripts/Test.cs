using UnityEngine;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            Debug.Log(hit.transform,hit.transform);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Down");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Begin Drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
}
