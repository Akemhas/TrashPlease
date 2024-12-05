using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class PoolObject<T> where T : MonoBehaviour
{
    public abstract string ID();
    [AssetsOnly] public T Prefab;
    public int DefaultCapacity;
    public int MaxSize;
}