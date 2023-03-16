using UnityEngine;
using UnityEngine.UI;
using Zenject;
using RetroStyleGamesTest.Pause;
using RetroStyleGamesTest.Data;

namespace RetroStyleGamesTest.UI
{
	public class UiManager : MonoBehaviour
	{
		[SerializeField] private Button _options;
		[SerializeField] private UIOptionsPopup _optionsPopup;
		[SerializeField] private UiGameOverPopup _gameOverPopup;

		private PauseManager _pauseManager;
		private PlayerStats _playerStats;
        private IPlayerInput _input;

        [Inject]
		private void Construct(PauseManager pauseManager, PlayerStats playerStats, IPlayerInput input)
		{
			_pauseManager = pauseManager;
			_playerStats = playerStats;

			_playerStats.Heals.HealsChanged += OnDie;

			_input = input;
			_input.Pause += OnOptionsClick;
		}

		public void OnOptionsClick()
		{
			if (_pauseManager.IsPaused)
				return;
			_pauseManager.SetPause(true);
			_optionsPopup.gameObject.SetActive(true);
			_optionsPopup.Closed += OnOptionsClose;
		}

		private void OnDie(int heals)
		{
			if (heals < 1)
			{
				_pauseManager.SetPause(true);
				_gameOverPopup.gameObject.SetActive(true);
			}
		}

		private void Start()
		{
			_options.onClick.AddListener(OnOptionsClick);
		}

		private void OnDestroy()
		{
			_playerStats.Heals.HealsChanged -= OnDie;
			_input.Pause -= OnOptionsClick;
			OnOptionsClose();
		}

		private void OnOptionsClose()
		{
			_optionsPopup.Closed -= OnOptionsClose;
			_pauseManager.SetPause(false);
		}
	}
}