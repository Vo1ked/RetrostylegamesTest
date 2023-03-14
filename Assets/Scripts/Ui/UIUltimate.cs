using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UIUltimate : MonoBehaviour, IPointerClickHandler
{

	public event Action UltimateClicked = () => { };

	[SerializeField] private Slider _playerUltimate;
	[SerializeField] private Text _clickText;

	private PlayerStats _playerStats;
	[Inject]
	private void Construct(PlayerStats playerStats)
	{
		_playerStats = playerStats;
	}

	private void Start()
	{
		_playerStats.Mana.CurrentManaChanged += OnUltimateStateChanged;
		_playerUltimate.value = _playerStats.Mana.CurrentMana / (float)_playerStats.Mana.MaxMana;
		_clickText.enabled = _playerStats.Mana.CurrentMana >= _playerStats.Mana.MaxMana;
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		UltimateClicked.Invoke();
	}

	public void OnDisable()
	{
		_playerStats.Mana.CurrentManaChanged -= OnUltimateStateChanged;
	}

	private void OnUltimateStateChanged(int mana)
	{
		_playerUltimate.value = _playerStats.Mana.CurrentMana / (float)_playerStats.Mana.MaxMana;
		_clickText.enabled = _playerStats.Mana.CurrentMana >= _playerStats.Mana.MaxMana;
	}
}
