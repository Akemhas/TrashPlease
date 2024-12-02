using PrimeTween;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    public bool InputPaused;
    private InputAction _dragAction;
    private InputAction _swipeAction;
    private Transform _draggable;
    private bool _enabled;
    private Camera _mainCam;
    private float _swipeThreshold = 1;

    private float swipeStartX;
    private float mouseStartX;

    private Vector3 _worldPos;

    public BinScoll _binScroller;



    public void Awake()
    {
        _mainCam = Camera.main;
        _dragAction = new InputAction("Dragging", InputActionType.Value, "<Mouse>/position");
        _dragAction.performed += OnDrag;
        _swipeAction = new InputAction("Dragging", InputActionType.Value, "<Mouse>/position");
        _swipeAction.performed += OnSwipe;
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

    public void AddSwipe(Transform t)
    {
        if (_binScroller.tween.isAlive)
        {
            _binScroller.tween.Stop();
        }
        _draggable = t;
        swipeStartX = t.position.x;
        mouseStartX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        _swipeAction.Enable();
    }

    public void RemoveDraggable()
    {
        if (!_draggable) return;
        _dragAction.Disable();
        _draggable = null;
    }

    public void RemoveSwipe()
    {
        if (!_draggable) return;
        _swipeAction.Disable();
        float swipeDistance = _draggable.position.x - swipeStartX;
        if(swipeDistance < -_swipeThreshold)
        {
            Debug.Log("SWIPE LEFT");
            _binScroller.swipeRight();
        }
        if (swipeDistance > _swipeThreshold)
        {
            _binScroller.swipeLeft();
            
        }
        if (Math.Abs(swipeDistance) <= _swipeThreshold && Math.Abs(swipeDistance) > 0)
        {
            _binScroller.moveBack();
        }
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

    private void OnSwipe(InputAction.CallbackContext context)
    {
        if (InputPaused) return;

        var mouseScreenPos = context.ReadValue<Vector2>();        
        _worldPos.x = mouseScreenPos.x;

        var newPosition = _mainCam.ScreenToWorldPoint(_worldPos);

        float finalPos = Mathf.Clamp(swipeStartX - (mouseStartX - newPosition.x), -6, 6);

        _draggable.position = new Vector3(finalPos, _draggable.position.y, _draggable.position.z);
    }
}