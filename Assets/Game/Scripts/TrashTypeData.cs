using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TrashTypeData", menuName = "TrashTypeData", order = 0)]
public class TrashTypeData : ScriptableObject
{
    [SerializeField, OnValueChanged("ValueChanged")] private List<TrashData> _trashData;

    public SerializableDictionary<TrashSortType, List<TrashData>> TrashLookup = new();

    private void ValueChanged()
    {
        TrashLookup.Clear();

        if (_trashData.Count <= 0)
        {
            Debug.Log($"No Data");
            return;
        }

        foreach (var data in _trashData)
        {
            if (!TrashLookup.ContainsKey(data.SortType))
            {
                TrashLookup.Add(data.SortType, new List<TrashData>());
            }

            TrashLookup[data.SortType].Add(data);
        }
    }
}