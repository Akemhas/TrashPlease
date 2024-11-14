using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Random = UnityEngine.Random;

public class TrashController : Singleton<TrashController>
{
    [SerializeField] private TrashLoader _trashLoader;
    [SerializeField] private SpriteRenderer _beltRenderer;
    [SerializeField, Range(0, 1)] private float _spawnSpacingAmount = .6f;
    [SerializeField] private float _binEntranceDuration = .8f;
    [SerializeField] private string[] _dummyAddresses;

    private List<Trash> _instantiatedTrashList = new();

    private Bounds _trashSpawnBounds;
    private AsyncOperationHandle<GameObject> _binHandle;
    private InstantiationParameters _binSpawnParams;
    private float _closestZPosition = 1;
    private Vector3 _binSpawnPos, _binDestroyPos;

    private void Awake()
    {
        var inputManager = InputManager.Instance;
        inputManager.TrashDroppedOnPlayerBin += OnTrashDroppedOnPlayerBin;
        inputManager.TrashDroppedOnEmptySpace += OnTrashDroppedOnEmptySpace;
        inputManager.TrashDroppedOnTrashArea += OnTrashDroppedOnTrashArea;
        inputManager.TrashPicked += OnTrashPicked;

        CalculateBinInsParameters();
    }

    private void CalculateBinInsParameters()
    {
        var mainCam = Camera.main;

        float screenHeight = mainCam.orthographicSize * 2;
        float screenWidth = screenHeight * mainCam.aspect;

        _binSpawnPos = new Vector3(-screenWidth / 2 - 2, 0, 0);
        _binSpawnPos.x -= mainCam.transform.position.x;

        _binDestroyPos = new Vector3(screenWidth / 2 + 2, 0, 0);
        _binDestroyPos.x += mainCam.transform.position.x;

        _binSpawnPos.z = 0;
        _binDestroyPos.z = 0;

        _binSpawnParams = new InstantiationParameters(_binSpawnPos, Quaternion.identity, transform);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            DummyLoadTrash();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            DummyInsTrash();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DummyBringBin(TrashSortType.Yellow);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            DummyDestroyBin();
        }
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

    [ContextMenu("ro")]
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

    private void DummyDestroyBin()
    {
        foreach (var trash in _instantiatedTrashList)
        {
            trash.ToggleCollider2D(false);
        }

        PrimeTween.Tween.LocalPositionX(_binHandle.Result.transform, _binDestroyPos.x, _binEntranceDuration)
            .OnComplete(() =>
            {
                for (var i = _instantiatedTrashList.Count - 1; i >= 0; i--)
                {
                    var trash = _instantiatedTrashList[i];
                    DestroyTrash(trash);
                }

                Addressables.ReleaseInstance(_binHandle);
            });
    }

    private void DummyBringBin(TrashSortType sortType)
    {
        if (_binHandle.IsValid())
        {
            Debug.LogWarning($"Please destroy the current bin before instantiating another one");
            return;
        }

        _binHandle = Addressables.InstantiateAsync(BinAddress(sortType), _binSpawnParams);
        _binHandle.Completed += OnBinInstantiated;
    }

    private void OnBinInstantiated(AsyncOperationHandle<GameObject> obj)
    {
        var t = obj.Result.transform;
        var lp = t.localPosition;
        lp.y = 0;
        t.localPosition = lp;
        PrimeTween.Tween.LocalPositionX(t, 0, _binEntranceDuration);
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
        _instantiatedTrashList.Remove(trash);
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

        var parent = _binHandle.Result.transform;

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

    private string BinAddress(TrashSortType sortType) => "Bin_" + sortType;
}