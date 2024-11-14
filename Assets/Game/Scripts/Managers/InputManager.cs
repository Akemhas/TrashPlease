using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private bool _inputPaused;

    public bool InputPaused
    {
        get => _inputPaused;
        set
        {
            _dragHandler.InputPaused = value;
            _inputPaused = value;
        }
    }

    public event Action<Trash, PlayerBin> TrashDroppedOnPlayerBin;
    public event Action<Trash> TrashDroppedOnEmptySpace;
    public event Action<Trash> TrashDroppedOnTrashArea;

    [SerializeField] private DragHandler _dragHandler;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private LayerMask _trashLayerMask;
    [SerializeField] private LayerMask _binLayerMask;

    private Trash _trash;
    private InputState _inputState;
    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void OnClicked(InputValue value)
    {
        if (InputPaused) return;
        var hit = RaycastToMousePosition(_trashLayerMask);
        if (hit.collider == null) return;

        _trash = hit.collider.GetComponent<Trash>();
        _dragHandler.AddDraggable(_trash.transform);
        _inputState = InputState.CarryingObject;
    }

    private void OnReleased()
    {
        if (InputPaused) return;
        if (_inputState != InputState.CarryingObject) return;

        var hit = RaycastToMousePosition(_binLayerMask);
        if (hit.collider == null)
        {
            TrashDroppedOnEmptySpace?.Invoke(_trash);

            ReleaseDrag();
            return;
        }

        if (hit.collider.TryGetComponent(out PlayerBin bin))
        {
            TrashDroppedOnPlayerBin?.Invoke(_trash, bin);
        }
        else
        {
            TrashDroppedOnTrashArea?.Invoke(_trash);
        }

        ReleaseDrag();
    }

    private void ReleaseDrag()
    {
        _trash = null;
        _dragHandler.RemoveDraggable();
        _inputState = InputState.Idle;
    }

    private RaycastHit2D RaycastToMousePosition(LayerMask layerMask)
    {
        var mousePos = Input.mousePosition;
        Vector3 mouseWorldPos = _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -_mainCam.transform.position.z));
        var hit = Physics2D.Raycast(new Vector2(mouseWorldPos.x, mouseWorldPos.y), Vector2.up, distance: 0.0001f, layerMask: layerMask);
        return hit;
    }

    private enum InputState
    {
        Idle,
        CarryingObject,
    }
}