using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    public void SetAnimationEnabled(bool isEnabled)
    {
        _animator.enabled = isEnabled;
    }
}