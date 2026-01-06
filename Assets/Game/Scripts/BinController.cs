using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class BinController : MonoBehaviour
{
    public event Action BinCreated;
    public event Action BinBeforeDestroy;
    public event Action BinReachedCenter;

    [ReadOnly] public Transform CurrentBin;

    [SerializeField] private SpriteRenderer _beltRenderer;
    [SerializeField] private Vector2 _targetScrollValue = new Vector2(-1f, 0);
    [SerializeField] private float _binEntranceDuration = .8f;

    private AsyncOperationHandle<GameObject> _binHandle;
    private InstantiationParameters _binSpawnParams;
    private Vector3 _binSpawnPos, _binDestroyPos;

    private readonly int _offsetId = Shader.PropertyToID("_SpriteOffset");

    private void Awake()
    {
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

    public void DestroyBin()
    {
        OnTrashBinDestroyed();
        // ScrollConveyorBelt();
        // var binTransform = _binHandle.Result.transform;
        // Tween.LocalPositionX(binTransform, _binDestroyPos.x, _binEntranceDuration)
        //     .OnComplete(OnTrashBinDestroyed);
        return;

        void OnTrashBinDestroyed()
        {
            BinBeforeDestroy?.Invoke();
            TopBinController.BinCounter++;
            CurrentBin = null;
            Addressables.ReleaseInstance(_binHandle);
        }
    }

    public void StartCreatingBin(TrashSortType sortType)
    {
        if (_binHandle.IsValid())
        {
            Debug.LogWarning($"Please destroy the current bin before instantiating another one");
            return;
        }

        _binHandle = Addressables.InstantiateAsync(BinAddress(sortType), _binSpawnParams);
        _binHandle.Completed += OnBinInstantiated;
    }

    private void OnBinInstantiated(AsyncOperationHandle<GameObject> handle)
    {
        var t = handle.Result.transform;
        var lp = t.localPosition;
        lp.y = 0;
        t.localPosition = lp;
        CurrentBin = t;
        BinCreated?.Invoke();
        Tween.LocalPositionX(t, 0, _binEntranceDuration, Ease.Linear).OnComplete(() => BinReachedCenter?.Invoke());
        ScrollConveyorBelt();
    }

    private void ScrollConveyorBelt()
    {
        Tween.MaterialProperty(_beltRenderer.material, _offsetId, _targetScrollValue, _binEntranceDuration, Ease.Linear)
            .OnComplete(() => _beltRenderer.material.SetVector(_offsetId, Vector2.zero));
    }

    private string BinAddress(TrashSortType sortType) => "Bin_" + sortType;
}