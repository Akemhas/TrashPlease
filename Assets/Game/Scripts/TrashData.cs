using UnityEngine;

[CreateAssetMenu(menuName = "Create TrashData", fileName = "TrashData", order = 0)]
public class TrashData : ScriptableObject
{
    public TrashSortType SortType;
    public string Name;
    public string Information;
}