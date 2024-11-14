using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TrashLoader : MonoBehaviour
{
    public readonly Dictionary<string, GameObject> PrefabReferenceCache = new();
    public readonly Dictionary<string, int> ReferenceCountCache = new();

    public void LoadTrash(string address)
    {
        LoadTrashAsync(address);
    }

    public void LoadMultipleTrash(string[] addresses)
    {
        LoadMultipleTrashAsync(addresses);
    }

    public void DestroyTrash(Trash trash)
    {
        var address = trash.TrashAddress;
        var go = trash.gameObject;
        Destroy(go);
        var refCount = --ReferenceCountCache[address];

        if (refCount > 0) return;

        Addressables.Release(PrefabReferenceCache[address]);
        PrefabReferenceCache.Remove(address);
        ReferenceCountCache.Remove(address);
#if UNITY_EDITOR
        Debug.Log($"{address} is not being used anymore releasing it's reference");
#endif
    }

    private async void LoadTrashAsync(string address)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(address);

        handle.Completed += operationHandle =>
        {
            ReferenceCountCache.TryAdd(address, 0);
            if (PrefabReferenceCache.TryAdd(address, operationHandle.Result))
            {
            }
        };

        await handle.Task;
    }

    private async void LoadMultipleTrashAsync(string[] addresses)
    {
        var handles = new List<Task>();

        foreach (var address in addresses)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);

            handle.Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    ReferenceCountCache.TryAdd(address, 0);
                    if (PrefabReferenceCache.TryAdd(address, operationHandle.Result))
                    {
                    }
                }
            };

            handles.Add(handle.Task);
        }

        await Task.WhenAll(handles);
    }
}