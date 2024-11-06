using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBin : MonoBehaviour, IDropHandler
{
    public TrashSortType BinTrashSortType;
    public void OnDrop(PointerEventData eventData)
    {
        var trash = eventData.pointerDrag.GetComponent<Trash>();
        if (trash.TrashSortType == BinTrashSortType || BinTrashSortType == TrashSortType.Black)
        {
            TrashController.Instance.ManualRecycleTrash(trash);
        }
        else
        {
            trash.ReturnToStartPosition();
        }
    }
}