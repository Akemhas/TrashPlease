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

    public List<TrashDataExportStorage> TrashExports = new List<TrashDataExportStorage>();
    public string ExportJson;
    public string ImportJson;

    [Button]
    public void ExportData()
    {
        TrashExports.Clear();
        foreach (var data in _trashData)
        {
            TrashExports.Add(new TrashDataExportStorage
            {
                Address = data.Address,
                NameInEn = data.Name,
                ExplanationInEn = data.Information
            });
        }

        ExportJson = Newtonsoft.Json.JsonConvert.SerializeObject(TrashExports);
    }

    [Button]
    public void ImportData()
    {
        TrashExports = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrashDataExportStorage>>(ImportJson);
        foreach (var export in TrashExports)
        {
            var data = _trashData.Find(x => x.Address == export.Address);
            data.NameInDe = export.NameInDe;
            data.Erlauterung = export.ExplanationInDe;
        }
    }

    public string GetRandomTrashAddress(TrashSortType sortType)
    {
        var data = TrashLookup[sortType];
        return data[Random.Range(0, data.Count)].Address;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        ValueChanged();
    }

    private void ValueChanged()
    {
        Undo.RecordObject(this, "DictionaryChange");
        TrashLookup.Clear();

        if (_trashData.Count <= 0)
        {
            Debug.Log($"No Data");
            return;
        }

        _trashData.Sort((x, y) => String.Compare(x.name, y.name, StringComparison.Ordinal));
        ClearDuplicates();

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

    private void ClearDuplicates()
    {
        List<TrashData> newList = new List<TrashData>();
        for (int i = _trashData.Count - 1; i >= 0; i--)
        {
            if (newList.Contains(_trashData[i]))
            {
                _trashData.Remove(_trashData[i]);
                continue;
            }

            newList.Add(_trashData[i]);
        }
    }
#endif
}

[Serializable]
public class TrashSortTypeTrashDataListDictionary : SerializableDictionary<TrashSortType, List<TrashData>, TrashDataListStorage> { }

[Serializable]
public class TrashDataListStorage : SerializableDictionary.Storage<List<TrashData>> { }

[Serializable]
public class TrashDataExportStorage
{
    public string Address;
    public string NameInEn;
    public string NameInDe;
    public string ExplanationInEn;
    public string ExplanationInDe;
}