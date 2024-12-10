using PrimeTween;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    [ReadOnly] public bool InputPaused;

    [SerializeField] private BinScroll _binScroller;

    private InputAction _dragAction;
    private InputAction _swipeAction;
    private Transform _draggable;
    private bool _enabled;
    private Camera _mainCam;
    private float _swipeThreshold = 1;

    private float swipeStartX;
    private float mouseStartX;

    private Vector3 _worldPos;

    public void Awake()
    {
        _mainCam = Camera.main;

        _dragAction = InputSystem.actions.FindAction("Drag");
        _swipeAction = InputSystem.actions.FindAction("Swipe");
    }

    private void OnEnable()
    {
        _dragAction.performed += OnDragPerformed;
        _swipeAction.performed += OnSwipePerformed;
        _dragAction.Disable();
        _swipeAction.Disable();
    }

    private void OnDisable()
    {
        _dragAction.performed -= OnDragPerformed;
        _swipeAction.performed -= OnSwipePerformed;
    }

    private void Start()
    {
        _worldPos.z = -_mainCam.transform.position.z;
    }

    public void AddDraggable(Transform t)
    {
        _draggable = t;
        _dragAction.Enable();
    }

    public void AddSwipe(Transform t)
    {
        if (_binScroller.Tween.isAlive)
        {
            _binScroller.Tween.Stop();
        }

        _draggable = t;
        swipeStartX = t.position.x;
        mouseStartX = _mainCam.ScreenToWorldPoint(Input.mousePosition).x;
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

        if (swipeDistance < -_swipeThreshold)
        {
            _binScroller.SwipeDir(1);
        }

        if (swipeDistance > _swipeThreshold)
        {
            _binScroller.SwipeDir(-1);
        }

        if (Math.Abs(swipeDistance) <= _swipeThreshold && Math.Abs(swipeDistance) > 0)
        {
            _binScroller.MoveBack();
        }

        _draggable = null;
    }

    private void OnDragPerformed(InputAction.CallbackContext callbackContext)
    {
        if (InputPaused) return;
        if (!_draggable) return;
        if (!InputManager.HasTrash) return;

        var mouseScreenPos = callbackContext.ReadValue<Vector2>();
        _worldPos.x = mouseScreenPos.x;
        _worldPos.y = mouseScreenPos.y;

        var newPosition = _mainCam.ScreenToWorldPoint(_worldPos);
        newPosition.z = _draggable.position.z;
        _draggable.position = newPosition;
    }

    private void OnSwipePerformed(InputAction.CallbackContext callbackContext)
    {
        if (InputPaused) return;

        if (InputManager.HasTrash) return;
        if (!_draggable) return;

        var mouseScreenPos = callbackContext.ReadValue<Vector2>();
        _worldPos.x = mouseScreenPos.x;

        var newPosition = _mainCam.ScreenToWorldPoint(_worldPos);

        float finalPos = Mathf.Clamp(swipeStartX - (mouseStartX - newPosition.x), -6, 6);

        _draggable.localPosition = new Vector3(finalPos, 0, 0);
    }
}