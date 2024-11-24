using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "TrashTypeData", menuName = "TrashTypeData", order = 0)]
public class TrashTypeData : ScriptableObject
{
    [SerializeField, OnValueChanged("ValueChanged")] private List<TrashData> _trashData;

    public TrashSortTypeTrashDataListDictionary TrashLookup = new();

    public string GetRandomTrashAddress(TrashSortType sortType)
    {
        var data = TrashLookup[sortType];
        return data[Random.Range(0, data.Count)].Address;
    }

#if UNITY_EDITOR
    private void ValueChanged()
    {
        Undo.RecordObject(this, "DictionaryChange");
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

        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif
}


[Serializable]
public class TrashSortTypeTrashDataListDictionary : SerializableDictionary<TrashSortType, List<TrashData>, TrashDataListStorage>
{
}

[Serializable]
public class TrashDataListStorage : SerializableDictionary.Storage<List<TrashData>>
{
}