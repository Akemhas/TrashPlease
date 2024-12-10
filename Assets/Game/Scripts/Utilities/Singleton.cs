using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static readonly Lazy<T> _lazyInstance = new Lazy<T>(GetInstance, true);

    public static T Instance => _lazyInstance.Value;

    private static bool _shuttingDown;

    private static T GetInstance()
    {
        if (_shuttingDown)
        {
            Debug.LogWarning($"{typeof(T).Name} has already destroyed, returning null.");
            return null;
        }

        var instance = FindFirstObjectByType<T>();

        return instance ? instance : CreateInstance();
    }

    private static T CreateInstance()
    {
        var go = new GameObject($"[{typeof(T).Name}]");
        var instance = go.AddComponent<T>();
        DontDestroyOnLoad(go);
        return instance;
    }

    protected virtual void OnApplicationQuit()
    {
        _shuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        _shuttingDown = true;
    }
}