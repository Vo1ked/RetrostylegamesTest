using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UiManager : MonoBehaviour {

	[SerializeField] private Button _options;
	[SerializeField] private Text playerHeals;



	private PauseManager _pauseManager;
	private Score _score;
	[Inject]
	private void Construct(PauseManager pauseManager,Score score)
	{
		_pauseManager = pauseManager;
		_score = score;
	}
	// Use this for initialization
	void Start () {
		_options.onClick.AddListener(() => { _pauseManager.SetPause(!_pauseManager.IsPaused); });
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
