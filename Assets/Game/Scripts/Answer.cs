using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Answer : MonoBehaviour
{
    internal bool IsCorrect;
    public event Action<Answer> Clicked;
    [SerializeField] private Button _button;
    public Transform AnswerPanel;
    public GameObject CorrectBackground;
    public GameObject FalseBackground;
    public GameObject NeutralBackground;
    public TextMeshProUGUI AnswerTMP;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    public void ToggleButtonActiveness(bool active) => _button.interactable = active;

    public void SetNeutral()
    {
        CorrectBackground.SetActive(false);
        FalseBackground.SetActive(false);
        NeutralBackground.SetActive(true);
    }

    public void SetCorrect()
    {
        CorrectBackground.SetActive(true);
        FalseBackground.SetActive(false);
        NeutralBackground.SetActive(false);
    }

    public void SetWrong()
    {
        CorrectBackground.SetActive(false);
        FalseBackground.SetActive(true);
        NeutralBackground.SetActive(false);
    }

    private void OnButtonClicked()
    {
        AudioManager.Instance.PlaySoundEffect(SoundEffectType.Click);
        Clicked?.Invoke(this);
    }
}