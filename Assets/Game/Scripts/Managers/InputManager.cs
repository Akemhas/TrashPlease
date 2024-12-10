using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster _graphicRaycaster;
    private readonly PointerEventData _clickData = new(EventSystem.current);
    private readonly List<RaycastResult> _raycastResults = new();

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

    public static event Action<Trash, PlayerBin> TrashDroppedOnPlayerBin;
    public static event Action<Trash> TrashDroppedOnEmptySpace;
    public static event Action<Trash> TrashDroppedOnTrashArea;
    public static event Action<Trash> TrashDroppedOnInspectTable;
    public static event Action<Trash> TrashPicked;
    public static event Action<Trash> TrashPickedFromInspectTable;

    [SerializeField] private DragHandler _dragHandler;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private LayerMask _trashLayerMask;
    [SerializeField] private LayerMask _binLayerMask;
    [SerializeField] private LayerMask _inspectionLayerMask;
    [SerializeField] private LayerMask _binSwipeLayerMask;

    private Trash _trash;
    private InputState _inputState;
    private Camera _mainCam;

    public Transform _swipe;

    public static bool HasTrash;

    private InputAction _clickAction;
    private InputAction _releaseAction;

    private void Awake()
    {
        HasTrash = false;
        _mainCam = Camera.main;
        _clickAction = InputSystem.actions.FindAction("Clicked");
        _releaseAction = InputSystem.actions.FindAction("Released");
    }

    private void OnEnable()
    {
        _clickAction.performed += OnClicked;
        _releaseAction.performed += OnReleased;
    }

    private void OnDisable()
    {
        _clickAction.performed -= OnClicked;
        _releaseAction.performed -= OnReleased;
    }

    private void OnDestroy()
    {
        HasTrash = false;
    }

    private void OnClicked(InputAction.CallbackContext callbackContext)
    {
        if (InputPaused) return;
        if (HasClickedOverUI()) return;

        var hitBin = RaycastToMousePosition(_binSwipeLayerMask);
        if (hitBin.collider != null)
        {
            _inputState = InputState.Swipe;
            _dragHandler.AddSwipe(_swipe);
            return;
        }

        var hit = RaycastToMousePosition(_trashLayerMask);
        if (hit.collider == null) return;

        _trash = hit.collider.GetComponent<Trash>();
        AudioManager.Instance.PlaySoundEffect(SoundEffectType.PickAndDrop, 1.5f);
        HasTrash = true;
        TrashPicked?.Invoke(_trash);

        var inspectRaycast = RaycastToMousePosition(_inspectionLayerMask);
        if (inspectRaycast.collider)
        {
            TrashPickedFromInspectTable?.Invoke(_trash);
        }

        Tween.Scale(_trash.transform, new Vector3(1.5f, 1.5f, 1), new TweenSettings(.2f, Ease.OutBack));
        _dragHandler.AddDraggable(_trash.transform);
        _inputState = InputState.CarryingObject;
    }

    private void OnReleased(InputAction.CallbackContext callbackContext)
    {
        if (InputPaused) return;

        if (_inputState == InputState.Swipe)
        {
            _inputState = InputState.Idle;
            _dragHandler.RemoveSwipe();
            return;
        }

        if (_inputState != InputState.CarryingObject) return;

        var hit = RaycastToMousePosition(_binLayerMask);

        if (!hit.collider)
        {
            if (!CheckInspectionTool())
            {
                TrashDroppedOnEmptySpace?.Invoke(_trash);
                ReleaseDrag();
                return;
            }

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

    private bool CheckInspectionTool()
    {
        bool result = false;
        var hit = RaycastToMousePosition(_inspectionLayerMask);
        if (hit.collider)
        {
            result = true;
            TrashDroppedOnInspectTable?.Invoke(_trash);
        }

        return result;
    }

    private void ReleaseDrag()
    {
        AudioManager.Instance.PlaySoundEffect(SoundEffectType.PickAndDrop, .5f);
        Tween.Scale(_trash.transform, Vector3.one, new TweenSettings(.125f, Ease.Linear));

        _trash = null;
        HasTrash = false;
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

    private bool HasClickedOverUI()
    {
        _clickData.position = Input.mousePosition;
        _raycastResults.Clear();
        _graphicRaycaster.Raycast(_clickData, _raycastResults);
        return _raycastResults.Count > 0;
    }

    private enum InputState
    {
        Idle,
        CarryingObject,
        Swipe,
    }
}