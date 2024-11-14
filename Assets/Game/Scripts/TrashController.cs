using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrashController : Singleton<TrashController>
{
    [SerializeField] private TrashLoader _trashLoader;
    [SerializeField] private Transform _garbageDumpArea;
    private List<GameObject> _instantiatedTrashList = new();

    [SerializeField] private string[] _dummyAddresses;
    [SerializeField, Range(0, 1)] private float _spawnSpacingAmount = .6f;

    private Bounds _trashSpawnBounds;

    private void Awake()
    {
        InputManager.Instance.TrashDroppedOnPlayerBin += OnTrashDroppedOnPlayerBin;
        InputManager.Instance.TrashDroppedOnEmptySpace += OnTrashDroppedOnEmptySpace;
        InputManager.Instance.TrashDroppedOnTrashArea += OnTrashDroppedOnTrashArea;

        _trashSpawnBounds = _garbageDumpArea.GetComponent<Collider2D>().bounds;
        _trashSpawnBounds.extents *= 1 - _spawnSpacingAmount;
    }

    private void OnTrashDroppedOnTrashArea(Trash trash)
    {
        Debug.Log($"Trash area");
        trash.SavePosition();
    }

    private void OnTrashDroppedOnEmptySpace(Trash trash)
    {
        Debug.Log($"Empty Space");
        trash.ReturnToStartPosition();
    }

    private void OnTrashDroppedOnPlayerBin(Trash trash, PlayerBin playerBin)
    {
        Debug.Log($"Player bin = {playerBin.name}");
        if (trash.TrashSortType == playerBin.BinTrashSortType)
        {
            DestroyTrash(trash);
        }
        else
        {
            trash.ReturnToStartPosition();
        }
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
        var randomYPos = Random.Range(_trashSpawnBounds.min.y, _trashSpawnBounds.max.y);
        var randomQuaternion = Quaternion.Euler(0, 0, Random.Range(-70f, 70f));

        var instantiatedTrash = Instantiate(prefab);

        instantiatedTrash.transform.position = new Vector3(randomXPos, randomYPos, 0);
        instantiatedTrash.transform.rotation = randomQuaternion;
        var trash = instantiatedTrash.GetComponent<Trash>();
        trash.TrashAddress = address;
        _instantiatedTrashList.Add(instantiatedTrash);
    }
}