using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;

public class TrashLoader : MonoBehaviour
{
    [ReadOnly] public int LoadingCount { get; private set; }
    public readonly Dictionary<string, GameObject> PrefabReferenceCache = new();
    public readonly Dictionary<string, int> ReferenceCountCache = new();

    public void LoadTrash(string address)
    {
        LoadTrashAsync(address);
    }

    public void DestroyTrash(Trash trash)
    {
        var address = trash.Data.Address;
        var go = trash.gameObject;
        Destroy(go);
        var refCount = --ReferenceCountCache[address];

        if (refCount > 0) return;

        Addressables.Release(PrefabReferenceCache[address]);
        PrefabReferenceCache.Remove(address);
        ReferenceCountCache.Remove(address);
    }

    private async void LoadTrashAsync(string address)
    {
        LoadingCount++;
        var handle = Addressables.LoadAssetAsync<GameObject>(address);

        handle.Completed += operationHandle =>
        {
            ReferenceCountCache.TryAdd(address, 0);
            PrefabReferenceCache.TryAdd(address, operationHandle.Result);
            LoadingCount--;
        };

        await handle.Task;
    }
}