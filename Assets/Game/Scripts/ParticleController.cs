using System.Collections;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    private ParticleSystem _ps;
    private ParticleSystemRenderer _pr;
    [SerializeField] private Material _plusParticle;
    [SerializeField] private Material _minusParticle;
    void Awake()
    {
        _ps = gameObject.GetComponent<ParticleSystem>();
        _pr = gameObject.GetComponent<ParticleSystemRenderer>();
    }

    public void playParticle(bool plus)
    {
        _pr.material = plus ? _plusParticle : _minusParticle;
        _ps.Play();
        Destroy(gameObject, 3f);
    }

}
