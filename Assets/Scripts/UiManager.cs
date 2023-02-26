using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UiManager : MonoBehaviour {

	[SerializeField] private Button _options;
    private PauseManager _pauseManager;


    [Inject]
	private void Construct(PauseManager pauseManager)
	{
		_pauseManager = pauseManager;
	}
	// Use this for initialization
	void Start () {
		_options.onClick.AddListener(() => { _pauseManager.SetPause(!_pauseManager.IsPaused); });
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
