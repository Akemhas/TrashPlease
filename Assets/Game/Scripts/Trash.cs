using UnityEngine;

public class Trash : MonoBehaviour
{
    internal string TrashAddress;
    
    public TrashSortType TrashSortType;
    private Vector3 _startPosition;
    private Vector3 _diffVector;

    private void Awake()
    {
        _startPosition = transform.position;
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