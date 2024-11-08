using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrashController : Singleton<TrashController>
{
    [SerializeField] private TrashLoader _trashLoader;
    private Transform _parentTransform;
    private List<GameObject> _instantiatedTrashList = new();

    [SerializeField] private string[] _dummyAddresses;

    private Bounds _trashSpawnBounds;

    private void Awake()
    {
        _parentTransform = UIManager.Instance.TrashParent;
        _trashSpawnBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(UIManager.Instance.GarbageDumpArea);
        _trashSpawnBounds.extents *= .5f;
    }

    [ContextMenu("Load Trash")]
    private void DummyLoadTrash()
    {
        LoadTrash(_dummyAddresses);
    }

    [ContextMenu("Ins Trash")]
    private void DummyInsTrash()
    {
        InstantiateTrash(_dummyAddresses);
    }

    public void LoadTrash(string[] _trashAddresses)
    {
        _trashLoader.LoadMultipleTrash(_trashAddresses);
    }

    public void ManualRecycleTrash(Trash trash)
    {
        DestroyTrash(trash);
    }

    private void DestroyTrash(Trash trash)
    {
        _trashLoader.DestroyTrash(trash);
    }

    public void InstantiateTrash(string[] trashAddresses)
    {
        foreach (var address in trashAddresses)
        {
            InstantiateTrash(address);
        }
    }

    public void InstantiateTrash(string address)
    {
        if (!_trashLoader.PrefabReferenceCache.TryGetValue(address, out GameObject prefab))
        {
            Debug.LogWarning($"Trying instantiate {address} without loading it first");
            return;
        }

        _trashLoader.ReferenceCountCache[address]++;
        var randomXPos = Random.Range(_trashSpawnBounds.min.x, _trashSpawnBounds.max.x);
        var randomYPos = Random.Range(_trashSpawnBounds.min.x, _trashSpawnBounds.max.y);
        var randomQuaternion = Quaternion.Euler(0, 0, Random.Range(-90f, 90f));
        var instantiatedTrash = Instantiate(prefab, _parentTransform);
        instantiatedTrash.transform.localPosition = new Vector3(randomXPos, randomYPos, 0);
        instantiatedTrash.transform.rotation = randomQuaternion;
        var trash = instantiatedTrash.GetComponent<Trash>();
        trash.TrashAddress = address;
        _instantiatedTrashList.Add(instantiatedTrash);
    }
}