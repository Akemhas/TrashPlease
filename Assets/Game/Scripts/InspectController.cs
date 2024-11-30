using System;
using UnityEngine;
using PrimeTween;
using TMPro;
using UnityEngine.UI;

public class InspectController : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _bin;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Transform _inspectWindowTransform;
    [SerializeField] private float _duration = .1f;

    private GameObject _inspectGo;
    private readonly Vector3 _defaultScale = Vector3.one;

    private void Awake()
    {
        _closeButton.onClick.AddListener(DeactivateInspect);
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

    public void DeactivateInspect()
    {
        if (!_inspectGo.activeSelf) return;

        Tween.ScaleY(_inspectWindowTransform, 0, _duration).OnComplete(() => _inspectGo.SetActive(false));
    }

    private void SetText(TrashData trashData)
    {
        _itemName.SetText(trashData.Name);
        _bin.SetText(trashData.SortType.ToString());
        _text.SetText(trashData.Information);
    }
}