using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    public bool InputPaused;
    private InputAction _dragAction;
    private Transform _draggable;
    private bool _enabled;
    private Camera _mainCam;

    private Vector3 _worldPos;

    public void Awake()
    {
        _mainCam = Camera.main;
        _dragAction = new InputAction("Dragging", InputActionType.Value, "<Mouse>/position");
        _dragAction.performed += OnDrag;
    }

    private void Start()
    {
        _worldPos.z = -_mainCam.transform.position.z;
        _dragAction.Disable();
    }

    public void AddDraggable(Transform t)
    {
        _draggable = t;
        _dragAction.Enable();
    }

    public void RemoveDraggable()
    {
        if (!_draggable) return;
        _dragAction.Disable();
        _draggable = null;
    }

    private void OnDrag(InputAction.CallbackContext context)
    {
        if (InputPaused) return;
        
        var mouseScreenPos = context.ReadValue<Vector2>();
        _worldPos.x = mouseScreenPos.x;
        _worldPos.y = mouseScreenPos.y;

        var newPosition = _mainCam.ScreenToWorldPoint(_worldPos);
        newPosition.z = _draggable.position.z;
        _draggable.position = newPosition;
    }
}