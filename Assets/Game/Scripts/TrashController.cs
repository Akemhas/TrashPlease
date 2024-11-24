using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrashController : Singleton<TrashController>
{
    [ReadOnly] public bool IsCurrentTrashLoaded;

    [SerializeField] private TrashLoader _trashLoader;
    [SerializeField, Range(0, 1)] private float _spawnSpacingAmount = .6f;

    private List<Trash> _instantiatedTrashList = new();

    private Bounds _trashSpawnBounds;
    private float _closestZPosition = 1;

    private void Awake()
    {
        var inputManager = InputManager.Instance;
        inputManager.TrashDroppedOnPlayerBin += OnTrashDroppedOnPlayerBin;
        inputManager.TrashDroppedOnEmptySpace += OnTrashDroppedOnEmptySpace;
        inputManager.TrashDroppedOnTrashArea += OnTrashDroppedOnTrashArea;
        inputManager.TrashPicked += OnTrashPicked;
    }

    private void OnTrashPicked(Trash trash)
    {
        if (trash.transform.position.z <= _closestZPosition) return;

        _closestZPosition -= .001f;

        trash.SetZPosition(_closestZPosition);

        if (_closestZPosition < -9)
        {
            ReorderTrash();
        }
    }

    private void ReorderTrash()
    {
        _instantiatedTrashList.Sort((y, x) => x.transform.position.z.CompareTo(y.transform.position.z));

        float maxZPos = 1f;
        foreach (var trash in _instantiatedTrashList)
        {
            var pos = trash.transform.position;
            pos.z = maxZPos;
            maxZPos -= .001f;
            trash.transform.position = pos;
        }

        _closestZPosition = maxZPos;
    }

    private void OnTrashDroppedOnTrashArea(Trash trash)
    {
        trash.SavePosition();
    }

    private void OnTrashDroppedOnEmptySpace(Trash trash)
    {
        trash.ReturnToStartPosition();
    }

    private void OnTrashDroppedOnPlayerBin(Trash trash, PlayerBin playerBin)
    {
        if (trash.TrashSortType == playerBin.BinTrashSortType)
        {
            DestroyTrash(trash);
        }
        else
        {
            trash.ReturnToStartPosition();
        }
    }

    public void LoadTrash(string[] trashAddresses)
    {
        _trashLoader.LoadMultipleTrash(trashAddresses);
    }

    public void ToggleTrashColliders(bool isEnabled)
    {
        foreach (var trash in _instantiatedTrashList) trash.ToggleCollider2D(isEnabled);
    }

    public void DestroyAllTrash()
    {
        for (var i = _instantiatedTrashList.Count - 1; i >= 0; i--)
        {
            DestroyTrash(_instantiatedTrashList[i]);
        }
    }

    private void DestroyTrash(Trash trash)
    {
        _instantiatedTrashList.Remove(trash);
        _trashLoader.DestroyTrash(trash);
    }

    public void InstantiateTrash(string[] trashAddresses, Transform parent)
    {
        foreach (var address in trashAddresses)
        {
            InstantiateTrash(address, parent);
        }
    }

    public void InstantiateTrash(string address, Transform parent)
    {
        if (!_trashLoader.PrefabReferenceCache.TryGetValue(address, out GameObject prefab))
        {
            Debug.LogWarning($"Trying instantiate {address} without loading it first");
            return;
        }

        if (parent == null) return;

        _trashSpawnBounds = parent.GetComponent<Collider2D>().bounds;
        _trashSpawnBounds.extents *= 1 - _spawnSpacingAmount;

        _trashLoader.ReferenceCountCache[address]++;
        var maxX = _trashSpawnBounds.extents.x / 2;
        var minX = -maxX;
        var maxY = _trashSpawnBounds.extents.y / 2;
        var minY = -maxY;
        var randomXPos = Random.Range(minX, maxX);
        var randomYPos = Random.Range(minY, maxY);

        var randomQuaternion = Quaternion.Euler(0, 0, Random.Range(-70f, 70f));

        var instantiatedTrash = Instantiate(prefab, parent);

        _closestZPosition -= .001f;
        instantiatedTrash.transform.localPosition = new Vector3(randomXPos, randomYPos, _closestZPosition);
        instantiatedTrash.transform.rotation = randomQuaternion;
        var trash = instantiatedTrash.GetComponent<Trash>();
        trash.TrashAddress = address;
        _instantiatedTrashList.Add(trash);
    }
}