using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Button _okButton;

    private void Awake()
    {
        _okButton.onClick.AddListener(OnOkButtonClicked);
    }

    private void OnOkButtonClicked()
    {
        GameManager.Instance.ProgressBin();
    }
}