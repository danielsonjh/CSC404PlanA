using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public static readonly string[] ControllerNames = { "PC", "J1" };

    public GameObject FireballPrefab;
    public GameObject DirectionArrowPrefab;

    private const float MoveForce = 40f;
    private const float FireballSpeed = 10f;

    private Vector3 LookDir { get { return _rawAxes.normalized; } }
    private Vector3 MoveDir { get { return _rawAxes.magnitude < 0.05f ? Vector3.zero : _rawAxes.normalized; } }

    private GameObject _indicatorContainer;
    private GameObject _directionIndicator;
    private string _controllerName;
    private Vector3 _rawAxes;
    private PlayerAction[] _actions = new PlayerAction[1];

    void Start ()
    {
        if (isClient)
        {
            RegisterPrefabs();
            InitializeIndicators();
            SetupControllers();

            TimeController.Instance.OnToggle += OnTimeControllerToggleHandler;
        }
    }

    void Update()
    {
        UpdateRotation();

        if (isLocalPlayer)
        {
            ProcessInput();
            UpdateIndicators();
        }
    }

    private void RegisterPrefabs()
    {
        ClientScene.RegisterPrefab(FireballPrefab);
    }

    private void InitializeIndicators()
    {
        _indicatorContainer = transform.FindChild("IndicatorContainer").gameObject;
        _directionIndicator = _indicatorContainer.transform.FindChild("DirectionIndicator").gameObject;
    }

    private void SetupControllers()
    {
        _controllerName = ControllerNames[playerControllerId];
        Debug.Log("Adding Controller: " + _controllerName);

        var isFirstPlayer = playerControllerId == 0;
        if (isFirstPlayer)
        {
            if (Input.GetJoystickNames().Length > 0)
            {
                ClientScene.AddPlayer(1);
            }
        }
    }

    private void UpdateIndicators()
    {
        if (TimeController.Instance.IsFreeze && MoveDir != Vector3.zero)
        {
            _indicatorContainer.transform.rotation = Quaternion.LookRotation(LookDir);
            _directionIndicator.GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            _directionIndicator.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void ProcessInput()
    {
        if (TimeController.Instance.IsFreeze)
        {
            var h = Input.GetAxisRaw("Horizontal_" + _controllerName);
            var v = Input.GetAxisRaw("Vertical_" + _controllerName);
            _rawAxes = new Vector3(h, 0, v);
            
            if (Input.GetButtonDown("Move_" + _controllerName))
            {
                AddAction(PlayerAction.Move);
            }
            if (Input.GetButtonDown("Fire_" + _controllerName))
            {
                AddAction(PlayerAction.Fire);
            }
            
            CmdSendInputs(_rawAxes, _actions.Select(i => i.ToString()).ToArray());
        }
    }

    private void AddAction(PlayerAction action)
    {
        for (var i = 0; i < _actions.Length; i++)
        {
            if (_actions[i] == PlayerAction.Empty)
            {
                _actions[i] = action;
            }
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
            _actions[i] = PlayerAction.Empty;
        }
    }

    private void UpdateRotation()
    {
        var lookDir = GetComponent<Rigidbody>().velocity;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void OnTimeControllerToggleHandler()
    {
        if (TimeController.Instance.IsMove)
        {
            if (IsSelectedAction(PlayerAction.Move))
            {
                var thisRigidbody = GetComponent<Rigidbody>();
                thisRigidbody.velocity = Vector3.zero;
                thisRigidbody.AddForce(MoveDir * MoveForce, ForceMode.Impulse);
                transform.rotation = Quaternion.LookRotation(LookDir);
            }
            else if (IsSelectedAction(PlayerAction.Fire))
            {
                var fireball = Instantiate(FireballPrefab, transform.position, transform.rotation);
                fireball.GetComponent<Rigidbody>().velocity = LookDir * FireballSpeed;
                NetworkServer.Spawn(fireball);
            }

            ClearActions();
        }
    }

    [Command]
    private void CmdSendInputs(Vector3 rawAxes, string[] actionsString)
    {
        _rawAxes = rawAxes;
        _actions = actionsString.Select(i => (PlayerAction) Enum.Parse(typeof(PlayerAction), i)).ToArray();
    }
}
