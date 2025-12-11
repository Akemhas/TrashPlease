using System;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;
using Managers;

public class InspectController : MonoBehaviour
{
    [SerializeField] private GameObject _panelWindow;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Transform _inspectWindowTransform;
    [SerializeField] private float _duration = .1f;
    [Header("AI Help")]
    [SerializeField] private ChatGptService _chatGptService;
    [SerializeField] private GameObject _askAiPanel;
    [SerializeField] private Button _wantToAskAIButton;
    [SerializeField] private Button _closeAiPanelButton;
    [SerializeField] private TMP_InputField _questionInput;
    [SerializeField] private Button _askAiButton;
    [SerializeField] private TextMeshProUGUI _aiResponseTMP;
    [SerializeField] private GameObject _aiLoadingIndicator;
    [SerializeField] private string _defaultQuestion = "Where should this go and why?";
    [SerializeField] private List<TrashSortType> _availableBins = new();

    private readonly List<Trash> _trashOnTable = new();
    private GameObject _inspectGo;
    private readonly Vector3 _defaultScale = Vector3.one;
    private TrashData _currentData;
    private bool _isRequestRunning;

    public int TrashCount => _trashOnTable.Count;

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(DeactivateInspect);
        if (_askAiButton != null) _askAiButton.onClick.AddListener(OnAskAiClicked);
        if (_wantToAskAIButton != null) _wantToAskAIButton.onClick.AddListener(OnWantToAskAIClicked);
        if (_closeAiPanelButton != null) _closeAiPanelButton.onClick.AddListener(OnCloseAiPanelClicked);
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(DeactivateInspect);
        if (_askAiButton != null) _askAiButton.onClick.RemoveListener(OnAskAiClicked);
        if (_wantToAskAIButton != null) _wantToAskAIButton.onClick.RemoveListener(OnWantToAskAIClicked);
        if (_closeAiPanelButton != null) _closeAiPanelButton.onClick.RemoveListener(OnCloseAiPanelClicked);
    }

    private void Start()
    {
        _inspectWindowTransform.localScale = _defaultScale;
        _inspectGo = _inspectWindowTransform.gameObject;
        _inspectGo.SetActive(false);
        ResetAiUi();
    }

    public void ActivateInspect(TrashData data)
    {
        if (_inspectGo.activeSelf) return;

        _currentData = data;
        SetText(data);
        ResetAiUi();
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
            _currentData = null;
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

    private void ResetAiUi()
    {
        if (_availableBins == null || _availableBins.Count == 0)
        {
            _availableBins = Enum.GetValues(typeof(TrashSortType))
                .Cast<TrashSortType>()
                .Where(t => t != TrashSortType.Question)
                .ToList();
        }

        if (_questionInput != null) _questionInput.text = _defaultQuestion;
        if (_aiResponseTMP != null) _aiResponseTMP.SetText(string.Empty);
        ToggleAiLoading(false);
        UpdateAiAvailability();
    }

    private void UpdateAiAvailability()
    {
        var service = _chatGptService;
        bool canUseAi = service != null && service.HasApiKey && !_isRequestRunning;
        if (_askAiButton != null) _askAiButton.interactable = canUseAi;
        if (!canUseAi && _aiResponseTMP != null)
        {
            // string reason = service == null ? "AI service not in scene." : "Add OPENAI_API_KEY to enable AI help.";
            // _aiResponseTMP.SetText(reason);
        }
    }

    private void ToggleAiLoading(bool loading)
    {
        _isRequestRunning = loading;
        if (_aiLoadingIndicator != null) _aiLoadingIndicator.SetActive(loading);
        if (_askAiButton != null) _askAiButton.interactable = !loading;
    }

    private void OnWantToAskAIClicked()
    {
        _panelWindow.SetActive(false);
        _askAiPanel.SetActive(true);
    }

    private void OnCloseAiPanelClicked()
    {
        _panelWindow.SetActive(true);
        _askAiPanel.SetActive(false);
    }

    private void OnAskAiClicked()
    {
        if (_currentData == null)
        {
            if (_aiResponseTMP != null) _aiResponseTMP.SetText("Inspect an item first.");
            return;
        }

        var service = _chatGptService;
        if (service == null)
        {
            if (_aiResponseTMP != null) _aiResponseTMP.SetText("No ChatGptService found in scene.");
            return;
        }

        if (!service.HasApiKey)
        {
            if (_aiResponseTMP != null) _aiResponseTMP.SetText("Set OPENAI_API_KEY to enable AI help.");
            return;
        }

        string question = _questionInput != null ? _questionInput.text : string.Empty;
        _ = RequestAiAsync(service, question);
    }

    private async Task RequestAiAsync(ChatGptService service, string question)
    {
        ToggleAiLoading(true);
        UpdateAiAvailability();
        string reply = await service.AskAsync(question, _currentData, _availableBins);
        ToggleAiLoading(false);
        if (_aiResponseTMP != null) _aiResponseTMP.SetText(reply);
        UpdateAiAvailability();
    }
}