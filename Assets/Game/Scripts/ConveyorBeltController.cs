using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConveyorBeltController : MonoBehaviour
{
    public List<ConveyorBelt> ConveyorBelts;
    [SerializeField] private float _spacing;

    public void ToggleBeltAnimations(bool isEnabled)
    {
        foreach (var belt in ConveyorBelts)
        {
            belt.SetAnimationEnabled(isEnabled);
        }
    }

    private void OnValidate()
    {
        ConveyorBelts = GetComponentsInChildren<ConveyorBelt>().ToList();

        var startPoint = (ConveyorBelts.Count - 1) / 2f * -_spacing;

        float offset = 0;
        foreach (var stopPoint in ConveyorBelts)
        {
            var pos = stopPoint.transform.position;
            pos.x = startPoint + offset;
            stopPoint.transform.position = pos;

            offset += _spacing;
        }
    }
}