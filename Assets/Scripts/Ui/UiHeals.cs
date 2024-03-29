﻿using UnityEngine;
using UnityEngine.UI;
using Zenject;
using RetroStyleGamesTest.Data;

namespace RetroStyleGamesTest.UI
{
	public class UiHeals : MonoBehaviour
	{
		[SerializeField] private Text _counter;

		private PlayerStats _playerStats;
		[Inject]
		private void Construct(PlayerStats playerStats)
		{
			_playerStats = playerStats;
		}

		private void OnEnable()
		{
			_playerStats.Heals.HealsChanged += OnHealsChanged;
			OnHealsChanged(_playerStats.Heals.CurrentHeals);
		}

		private void OnHealsChanged(int currentHeals)
		{
			_counter.text = currentHeals.ToString();
		}

		private void OnDisable()
		{
			_playerStats.Heals.HealsChanged -= OnHealsChanged;
		}
	}
}
