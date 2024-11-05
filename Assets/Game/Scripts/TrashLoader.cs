using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TrashLoader : MonoBehaviour
{
    public readonly Dictionary<string, GameObject> ReferenceCache = new();
    private readonly Dictionary<string, int> _referenceCountLookup = new();

    public void LoadTrash(string address)
    {
        LoadTrashAsync(address);
    }

    public void LoadMultipleTrash(string[] addresses)
    {
        LoadMultipleTrashAsync(addresses);
    }

    public void DestroyTrash(string address, GameObject go)
    {
        Destroy(go);
        var refCount = --_referenceCountLookup[address];

        if (refCount > 0) return;

        ReferenceCache.Remove(address);
        _referenceCountLookup.Remove(address);
        Addressables.Release(go);
        Debug.Log($"Trash {address} is not used anymore releasing it's reference");
    }

    private async void LoadTrashAsync(string address)
    {
        if (_referenceCountLookup.TryGetValue(address, out int refCount))
        {
            if (refCount > 0) return;
        }
        else _referenceCountLookup.TryAdd(address, 0);

        var handle = Addressables.LoadAssetAsync<GameObject>(address);

        handle.Completed += operationHandle =>
        {
            if (operationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                ReferenceCache.TryAdd(address, operationHandle.Result);
                _referenceCountLookup[address] = refCount + 1;
            }
        };

        await handle.Task;
    }

    private async void LoadMultipleTrashAsync(string[] addresses)
    {
        var handles = new List<Task>();

        foreach (var address in addresses)
        {
            if (_referenceCountLookup.TryGetValue(address, out int refCount))
            {
                if (refCount > 0) continue;
            }
            else _referenceCountLookup.TryAdd(address, 0);

            var handle = Addressables.LoadAssetAsync<GameObject>(address);

            handle.Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    ReferenceCache.TryAdd(address, operationHandle.Result);
                    _referenceCountLookup[address] = refCount + 1;
                }
            };

            handles.Add(handle.Task);
        }

        await Task.WhenAll(handles);
    }
}