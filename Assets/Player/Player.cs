using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public GameObject FireballPrefab;

    public Vector3 MoveDirection;
    public Vector3 ActionDirection;

    private const float MoveForce = 40f;
    private const float FireballSpeed = 10f;
    private GameObject _moveDirectionIndicator;
    private GameObject _actionDirectionIndicator;
    private PlayerActionHud _playerActionHud;
    private PlayerAction[] _actions = new PlayerAction[2];

    void Start ()
    {
        TimeController.Instance.OnToggle += OnTimeControllerToggleHandler;

        if (isClient)
        {
            InitializeHuds();

            if (!isLocalPlayer)
            {
                HideHuds();
            }
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            UpdateIndicators();
            CmdSendInputs(MoveDirection, ActionDirection, _actions.Select(i => i.ToString()).ToArray());
        }
    }

    public void AddAction(PlayerAction action)
    {
        for (var i = 0; i < _actions.Length; i++)
        {
            if (_actions[i] == PlayerAction.Empty)
            {
                SetAction(action, i);
                break;
            }
        }
    }

    private void InitializeHuds()
    {
        _moveDirectionIndicator = transform.FindChild("MoveDirectionIndicator").gameObject;
        _actionDirectionIndicator = transform.FindChild("ActionDirectionIndicator").gameObject;
        _playerActionHud = GetComponentInChildren<PlayerActionHud>();
    }

    private void HideHuds()
    {
        _moveDirectionIndicator.SetActive(false);
        _actionDirectionIndicator.SetActive(false);
        GetComponentInChildren<PlayerHud>().gameObject.SetActive(false);
    }

    private void UpdateIndicators()
    {
        if (TimeController.Instance.IsFreeze)
        {
            if (MoveDirection != Vector3.zero)
            {
                _moveDirectionIndicator.transform.rotation = Quaternion.LookRotation(MoveDirection);
                _moveDirectionIndicator.GetComponentInChildren<MeshRenderer>().enabled = true;
            }
            if (ActionDirection != Vector3.zero)
            {
                _actionDirectionIndicator.transform.rotation = Quaternion.LookRotation(ActionDirection);
                _actionDirectionIndicator.GetComponentInChildren<MeshRenderer>().enabled = true;
            }
        }
        else
        {
            _moveDirectionIndicator.GetComponentInChildren<MeshRenderer>().enabled = false;
            _actionDirectionIndicator.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
    }

    private bool IsSelectedAction(params PlayerAction[] actions)
    {
        return _actions.Length == actions.Length && !_actions.Where((t, i) => t != actions[i]).Any();
    }

    private void ClearActions()
    {
        for (var i = 0; i < _actions.Length; i++)
        {
            SetAction(PlayerAction.Empty, i);
        }
    }

    private void SetAction(PlayerAction action, int index)
    {
        _actions[index] = action;
        if (_playerActionHud.isActiveAndEnabled)
        {
            _playerActionHud.AddAction(action, index);
        }
    }
    
    private void OnTimeControllerToggleHandler()
    {
        if (TimeController.Instance.IsMove)
        {
            if (isServer)
            {
                if (ActionDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(ActionDirection);
                    
                    if (IsSelectedAction(PlayerAction.Fire, PlayerAction.Empty))
                    {
                        var fireball = Instantiate(FireballPrefab, transform.position, transform.rotation);
                        fireball.GetComponent<Rigidbody>().velocity = ActionDirection * FireballSpeed;
                        NetworkServer.Spawn(fireball);
                    }
                    else if (IsSelectedAction(PlayerAction.Fire, PlayerAction.Fire))
                    {
                        var fireball = Instantiate(FireballPrefab, transform.position, transform.rotation);
                        fireball.GetComponent<Rigidbody>().velocity = ActionDirection * FireballSpeed;
                        fireball.transform.localScale *= 2;
                        NetworkServer.Spawn(fireball);
                    }
                }

                if (MoveDirection != Vector3.zero)
                {
                    var thisRigidbody = GetComponent<Rigidbody>();
                    thisRigidbody.velocity = Vector3.zero;
                    thisRigidbody.AddForce(MoveDirection * MoveForce, ForceMode.Impulse);
                }
            }

            MoveDirection = Vector3.zero;
            ActionDirection = Vector3.zero;
            ClearActions();
        }
    }

    [Command]
    private void CmdSendInputs(Vector3 moveDirection, Vector3 actionDirection, string[] actionsString)
    {
        MoveDirection = moveDirection;
        ActionDirection = actionDirection;
        _actions = actionsString.Select(i => (PlayerAction) Enum.Parse(typeof(PlayerAction), i)).ToArray();
    }
}
