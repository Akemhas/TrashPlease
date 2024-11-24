using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrashController : MonoBehaviour
{
    [SerializeField] private TrashTypeData _trashTypeData;
    [SerializeField] private TrashLoader _trashLoader;
    [SerializeField, Range(0, 1)] private float _spawnSpacingAmount = .6f;


    private List<string> _loadedTrashList = new();
    private List<Trash> _instantiatedTrashList = new();

    private Bounds _trashSpawnBounds;
    private float _closestZPosition = 1;
    private readonly Array _trashSortTypes = Enum.GetValues(typeof(TrashSortType));
    private readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(.02f);


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

    public void LoadTrash(TrashSortType sortType, int count = 3)
    {
        _loadedTrashList.Clear();

        var wrongTypes = GetWrongSortTypes(sortType);

        for (int i = 0; i < count; i++)
        {
            int randomValue = Random.Range(0, 2);
            string address;

            if (randomValue % 2 == 0)
            {
                address = _trashTypeData.GetRandomTrashAddress(sortType);
            }
            else
            {
                var wrongSortType = wrongTypes[Random.Range(0, wrongTypes.Count)];
                address = _trashTypeData.GetRandomTrashAddress(wrongSortType);
            }

            _trashLoader.LoadTrash(address);
            _loadedTrashList.Add(address);
        }
    }

    public bool CheckTrashSorting(TrashSortType binsSortType)
    {
        foreach (var trash in _instantiatedTrashList)
        {
            if (trash.TrashSortType != binsSortType) return false;
        }

        return true;
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

    private List<TrashSortType> GetWrongSortTypes(TrashSortType sortType)
    {
        List<TrashSortType> wrongSortTypes = new List<TrashSortType>();
        foreach (var value in _trashSortTypes)
        {
            var enumValue = (TrashSortType)value;
            if (enumValue != sortType)
            {
                wrongSortTypes.Add(enumValue);
            }
        }

        return wrongSortTypes;
    }

    public void InstantiateTrashWhenReady(Transform parent)
    {
        if (parent == null) return;
        if (_trashLoader.LoadingCount > 0)
        {
            StartCoroutine(WaitUntilLoadingFinish(parent));
        }
        else
        {
            InstantiateTrash(parent);
        }
    }

    private IEnumerator WaitUntilLoadingFinish(Transform parent)
    {
        while (_trashLoader.LoadingCount > 0)
        {
            yield return _waitForSeconds;
        }

        InstantiateTrash(parent);
    }

    private void InstantiateTrash(Transform parent)
    {
        foreach (var address in _loadedTrashList)
        {
            var prefab = _trashLoader.PrefabReferenceCache[address];
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
            _instantiatedTrashList.Add(trash);
        }
    }
}