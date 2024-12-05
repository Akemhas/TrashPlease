using System;
using System.Collections;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    private ParticleSystem _ps;
    private ParticleSystemRenderer _pr;
    [SerializeField] private Material _plusParticle;
    [SerializeField] private Material _minusParticle;

    private static readonly WaitForSeconds _waitForSeconds = new(2.5f);

    void Awake()
    {
        _ps = gameObject.GetComponent<ParticleSystem>();
        _pr = gameObject.GetComponent<ParticleSystemRenderer>();
    }

    public void PlayParticle(bool plus, Action onComplete)
    {
        _pr.material = plus ? _plusParticle : _minusParticle;
        _ps.Play();
        StartCoroutine(ParticleTimer(onComplete));
    }

    private IEnumerator ParticleTimer(Action onComplete)
    {
        yield return _waitForSeconds;
        onComplete?.Invoke();
    }
}