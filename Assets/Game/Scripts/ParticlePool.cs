using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticlePool : MonoBehaviour
{
    [SerializeField] private List<ParticlePoolObject> _particlePoolObject;

    private readonly Dictionary<string, ObjectPool<ParticleController>> _particlePools = new();

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        foreach (var poolObject in _particlePoolObject)
        {
            _particlePools.TryAdd(poolObject.ID(),
                new ObjectPool<ParticleController>(() => InstantiateParticle(poolObject.Prefab, transform),
                    particle => { particle.gameObject.SetActive(true); },
                    particleController => particleController.gameObject.SetActive(false),
                    particleController => Destroy(particleController.gameObject),
                    true,
                    poolObject.DefaultCapacity,
                    poolObject.MaxSize));
        }
    }

    private ParticleController InstantiateParticle(ParticleController prefab, Transform parent) => Instantiate(prefab, parent);

    public ParticleController Get(string id, Vector3 position)
    {
        ParticleController particle = null;

        if (_particlePools.TryGetValue(id, out ObjectPool<ParticleController> result))
        {
            particle = result.Get();
            particle.transform.position = position;
        }
        else
        {
            Debug.LogError($"There is no object pool for {id}");
        }

        return particle;
    }

    public void Release(string id, ParticleController particle) => _particlePools[id].Release(particle);

    [Serializable]
    private class ParticlePoolObject : PoolObject<ParticleController>
    {
        [SerializeField] private string _id;
        public override string ID() => _id;
    }
}