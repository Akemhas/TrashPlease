using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;

public class TopBinPool : MonoBehaviour
{
    [SerializeField] private List<BinPoolObject> _binPoolObject;

    private readonly Dictionary<string, ObjectPool<TopBin>> _topBinPools = new();

    private void OnValidate()
    {
        foreach (var binPoolObject in _binPoolObject)
        {
            if (binPoolObject.Prefab == null)
            {
                Debug.LogWarning("Prefab is null in BinPoolObject");
                continue;
            }

            // Get the field "_sortType" with proper BindingFlags
            var field = binPoolObject.GetType().GetField("_sortType", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"Field '_sortType' not found in {nameof(BinPoolObject)}");
                continue;
            }

            try
            {
                // Set the value of the private field "_sortType"
                field.SetValue(binPoolObject, binPoolObject.Prefab.SortType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set '_sortType': {ex.Message}", binPoolObject.Prefab);
            }
        }
    }

    public void Initialize(Vector3 startPosition)
    {
        foreach (var poolObject in _binPoolObject)
        {
            _topBinPools.TryAdd(poolObject.ID(), new ObjectPool<TopBin>(() => InstantiateTopBin(poolObject.Prefab, startPosition), topBin =>
                {
                    topBin.transform.position = startPosition;
                    topBin.gameObject.SetActive(true);
                },
                topBin => topBin.gameObject.SetActive(false),
                bin => Destroy(bin.gameObject),
                true,
                poolObject.DefaultCapacity,
                poolObject.MaxSize));
        }
    }

    private TopBin InstantiateTopBin(TopBin prefab, Vector3 startPosition)
    {
        return Instantiate(prefab, startPosition, Quaternion.identity, transform);
    }

    public TopBin Get(TrashSortType sortType) => _topBinPools.TryGetValue(sortType.ToString(), out ObjectPool<TopBin> result) ? result.Get() : null;

    public void Release(TopBin topBin) => _topBinPools[topBin.SortType.ToString()].Release(topBin);

    [Serializable]
    private class BinPoolObject : PoolObject<TopBin>
    {
        [SerializeField] private TrashSortType _sortType;
        public override string ID() => _sortType.ToString();
    }
}