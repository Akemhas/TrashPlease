using UnityEngine;
using UnityEngine.EventSystems;

public class TrashReturner : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var trash = eventData.pointerDrag.GetComponent<Trash>();
        trash.ReturnToStartPosition();
    }
}