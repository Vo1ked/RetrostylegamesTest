using UnityEngine;
using UnityEngine.UI;
using Zenject;
using RetroStyleGamesTest.Data;

namespace RetroStyleGamesTest.UI
{
	public class UiScore : MonoBehaviour
	{

		[SerializeField] private Text _counter;

		private Score _score;
		[Inject]
		private void Construct(Score score)
		{
			_score = score;
		}

		private void OnEnable()
		{
			_score.ScoreChanged += OnScoreChanged;
			OnScoreChanged(_score.CurrentScore);
		}

		private void OnScoreChanged(int currentHeals)
		{
			_counter.text = currentHeals.ToString();
		}

		private void OnDisable()
		{
			_score.ScoreChanged -= OnScoreChanged;
		}
	}
}