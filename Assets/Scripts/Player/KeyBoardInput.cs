using System;
using UnityEngine;

public class KeyBoardInput : MonoBehaviour, IPlayerInput, IPauseHandler {
    public event Action<Vector2> Direction = (Vector2) => { };
    public event Action<Vector2> Rotation = (Vector2) => { };
    private bool _isPause;
    public void OnPause(bool IsPause)
    {
        _isPause = IsPause;
    }

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {

        if (_isPause)
            return;

        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Direction.Invoke(moveDirection);

        Vector2 rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Rotation.Invoke(rotation);

    }
}
