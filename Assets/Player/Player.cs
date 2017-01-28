using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public static readonly string[] ControllerNames = { "PC", "J1" };

    public GameObject FireballPrefab;
    public GameObject DirectionArrowPrefab;

    private const float MoveForce = 40f;
    private const float FireballSpeed = 10f;

    private string _controllerName;
    private Vector3 _lookDir;
    private Vector3 _moveDir;
    private bool _move;
    private bool _shoot;

    private GameObject _indicatorContainer;
    private GameObject _directionIndicator;

    private Action _chosenAction;

    void Start ()
    {
        if (isClient)
        {
            StartCoroutine(UnscaledUpdate());
            RegisterPrefabs();
            InitializeIndicators();
            SetupControllers();

            TimeController.Instance.OnToggle += OnTimeControllerToggleHandler;
        }
    }

    void Update()
    {
        if (isClient)
        {
            ProcessInput();
        }

        UpdateRotation();
    }

    private IEnumerator UnscaledUpdate()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (isLocalPlayer)
            {
                if (_move && _moveDir != Vector3.zero)
                {
                    CmdMove(_moveDir);
                    _move = false;
                }

                if (_shoot)
                {
                    CmdShoot(_lookDir);
                    _shoot = false;
                }
            }
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

    private void RotateIndicators(Vector3 direction)
    {
        _indicatorContainer.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void ProcessInput()
    {
        if (isLocalPlayer && TimeController.Instance.IsFreeze)
        {
            var h = Input.GetAxisRaw("Horizontal_" + _controllerName);
            var v = Input.GetAxisRaw("Vertical_" + _controllerName);
            var rawDir = new Vector3(h, 0, v);
            _lookDir = rawDir.normalized;
            _moveDir = rawDir.magnitude < 0.05f ? Vector3.zero : rawDir.normalized;
            
            _move = Input.GetButtonDown("Move_" + _controllerName);
            _shoot = Input.GetButtonDown("Fire_" + _controllerName);
            
            if (_moveDir != Vector3.zero) {
                RotateIndicators(_moveDir);
                _directionIndicator.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        else
        {
            _directionIndicator.GetComponent<MeshRenderer>().enabled = false;
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
            if (_chosenAction != null) _chosenAction.Invoke();
            _chosenAction = null;
        }
    }

    [Command]
    private void CmdMove(Vector3 dir)
    {
        _lookDir = dir.normalized;

        _chosenAction = () =>
        {
            var thisRigidbody = GetComponent<Rigidbody>();
            thisRigidbody.velocity = Vector3.zero;
            thisRigidbody.AddForce(dir * MoveForce, ForceMode.Impulse);
            transform.rotation = Quaternion.LookRotation(_lookDir);
        };
    }

    [Command]
    private void CmdShoot(Vector3 dir)
    {
        _chosenAction = () =>
        {
            var fireball = Instantiate(FireballPrefab, transform.position, transform.rotation);
            fireball.GetComponent<Rigidbody>().velocity = dir.normalized * FireballSpeed;
            NetworkServer.Spawn(fireball);
        };
    }
}
