using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrashController : MonoBehaviour
{
    [SerializeField] private TrashLoader _trashLoader;
    private Transform _parentTransform;
    private List<GameObject> _instantiatedTrashList = new();

    private void Awake()
    {
        _parentTransform = UIManager.Instance.transform;
    }

    [ContextMenu("Load Trash")]
    public void LoadTrash(string[] _trashAddresses)
    {
        _trashLoader.LoadMultipleTrash(_trashAddresses);
    }

    [ContextMenu("Instantiate Trashes")]
    public void InstantiateTrash(string[] trashAddresses)
    {
        foreach (var address in trashAddresses)
        {
            InstantiateTrash(address);
        }
    }

    public void InstantiateTrash(string address)
    {
        if (!_trashLoader.ReferenceCache.TryGetValue(address, out GameObject prefab)) return;
        var randomXPos = Random.Range(-5f, 5f);
        var randomYPos = Random.Range(-5f, 5f);
        var randomQuaternion = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        var instantiatedTrash = Instantiate(prefab, new Vector3(randomXPos, randomYPos), randomQuaternion, _parentTransform);
        _instantiatedTrashList.Add(instantiatedTrash);
    }
}