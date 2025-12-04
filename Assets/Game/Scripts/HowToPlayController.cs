using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayController : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private TextMeshProUGUI _howToPlayText;

    private void Awake()
    {
        _exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        gameObject.SetActive(false);
    }
}