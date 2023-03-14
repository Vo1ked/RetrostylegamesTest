using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIOptionsPopup : MonoBehaviour
{
    public System.Action Closed = () => { };

    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _restartButton;

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(OnClose);
        _restartButton.onClick.AddListener(OnRestart);
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(OnClose);
        _restartButton.onClick.RemoveListener(OnRestart);
    }

    private void OnRestart()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void OnClose()
    {
        Closed.Invoke();
        gameObject.SetActive(false);
    }
}
