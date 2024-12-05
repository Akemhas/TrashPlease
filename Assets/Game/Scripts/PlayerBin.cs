using System;
using UnityEngine;
using PrimeTween;

public class PlayerBin : MonoBehaviour
{
    public TrashSortType BinTrashSortType;
    [SerializeField] private Transform _lid;
    [SerializeField] private Vector3 _openedRotation;

    private readonly Vector3 _scaleUpTarget = new Vector3(1.3f, 1.3f, 1.3f);

    private bool _isLidOpen;
    private bool _hasLid;
    private Tween _scaleTween;

    private void Awake()
    {
        if (_lid) _hasLid = true;
    }

    public void OpenLid()
    {
        if (!_hasLid) return;

        if (_isLidOpen) return;

        Tween.StopAll(_lid);
        Tween.LocalRotation(_lid, _openedRotation, .2f, Ease.OutBack);
        _isLidOpen = true;
    }

    public void CloseLid()
    {
        if (!_hasLid) return;

        if (!_isLidOpen) return;

        _isLidOpen = false;
        Tween.StopAll(_lid);
        Tween.LocalRotation(_lid, Vector3.zero, .2f, Ease.InCirc);
    }

    public void ScaleUpDown()
    {
        if (_scaleTween.isAlive) return;
        _scaleTween = Tween.Scale(transform, _scaleUpTarget, .2f, Ease.OutBack, 2, CycleMode.Yoyo);
    }
}