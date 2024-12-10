using System;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using TMPro;
using UnityEngine.UI;

public class InspectController : MonoBehaviour
{
    public int TrashCount => _trashOnTable.Count;
    private readonly List<Trash> _trashOnTable = new();
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _bin;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Transform _inspectWindowTransform;
    [SerializeField] private float _duration = .1f;

    private GameObject _inspectGo;
    private readonly Vector3 _defaultScale = Vector3.one;

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(DeactivateInspect);
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(DeactivateInspect);
    }

    private void Start()
    {
        _inspectWindowTransform.localScale = _defaultScale;
        _inspectGo = _inspectWindowTransform.gameObject;
        _inspectGo.SetActive(false);
    }

    public void ActivateInspect(TrashData data)
    {
        if (_inspectGo.activeSelf) return;

        SetText(data);
        _inspectGo.SetActive(true);
        Tween.ScaleY(_inspectWindowTransform, 0, 1, _duration);
    }

    public void AddTrashToTable(Trash trash)
    {
        if (!_trashOnTable.Contains(trash))
        {
            _trashOnTable.Add(trash);
        }
    }

    public void RemoveTrashFromTable(Trash trash)
    {
        if (_trashOnTable.Contains(trash))
        {
            _trashOnTable.Remove(trash);
            DeactivateInspect();
        }
    }

    public void DeactivateInspect()
    {
        if (!_inspectGo.activeSelf) return;

        Tween.ScaleY(_inspectWindowTransform, 0, _duration).OnComplete(() => _inspectGo.SetActive(false));
    }

    private void SetText(TrashData trashData)
    {
        _itemName.SetText(trashData.Name);
        _text.SetText(trashData.Information);
    }
}