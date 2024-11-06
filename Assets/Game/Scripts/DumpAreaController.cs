using UnityEngine;
using UnityEngine.EventSystems;

public class DumpAreaController : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var trash = eventData.pointerDrag.GetComponent<Trash>();
        trash.SavePosition();
    }
}