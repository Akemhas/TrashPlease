using UnityEngine;
using PrimeTween;

public class Sway : MonoBehaviour
{
    void Start()
    {
        Tween.LocalPositionY(transform, startValue: 390, endValue: 370, duration: 0.8f, cycles: -1, cycleMode: CycleMode.Rewind);
    }
}
