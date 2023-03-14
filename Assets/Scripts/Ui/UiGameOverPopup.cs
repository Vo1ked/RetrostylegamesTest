using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RetroStyleGamesTest.UI
{
	public class UiGameOverPopup : MonoBehaviour
	{

		[SerializeField] private Button _restartButton;

		private void OnEnable()
		{
			_restartButton.onClick.AddListener(OnRestart);
		}

		private void OnDisable()
		{
			_restartButton.onClick.RemoveListener(OnRestart);
		}

		private void OnRestart()
		{
			SceneManager.LoadScene("SampleScene");
		}
	}
}