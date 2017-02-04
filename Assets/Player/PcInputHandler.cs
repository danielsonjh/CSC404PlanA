using UnityEngine;
using UnityEngine.Networking;

public class PcInputHandler : NetworkBehaviour
{
    private Player _player;
    private MouseHandler _mouseHandler;

    void Start()
    {
        _player = GetComponent<Player>();
        _mouseHandler = GetComponent<MouseHandler>();
    }

    void Update()
    {
        if (isLocalPlayer && TimeController.Instance.IsFreeze)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _player.ActionDirection = _mouseHandler.GetMouseDirection();
            }

            if (Input.GetMouseButtonDown(1))
            {
                _player.MoveDirection = _mouseHandler.GetMouseDirection();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                _player.AddAction(PlayerAction.Fire);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                _player.AddAction(PlayerAction.Water);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _player.AddAction(PlayerAction.Wind);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _player.AddAction(PlayerAction.Earth);
            }
        }
    }
}
