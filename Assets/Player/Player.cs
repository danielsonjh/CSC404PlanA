using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public static readonly string[] ControllerNames = { "PC", "J1" };

    public GameObject FireballPrefab;

    private const float MoveForce = 40f;
    private const float FireballSpeed = 10f;

    private string _controllerName;
    private Vector3 _lookDir;
    private Vector3 _moveDir;
    private bool _move;
    private bool _shoot;

    private Action _chosenAction;

	void Start ()
	{
	    if (isClient)
	    {
	        _controllerName = ControllerNames[playerControllerId];
	        Debug.Log("Adding Controller: " + _controllerName);
            StartCoroutine(UnscaledUpdate());
            TimeController.Instance.OnToggle += OnTimeControllerToggleHandler;
	        if (playerControllerId == 0)
	        {
                ClientScene.RegisterPrefab(FireballPrefab);

                if (Input.GetJoystickNames().Length > 0)
	            {
	                ClientScene.AddPlayer(1);
	            }
	        }
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

    IEnumerator UnscaledUpdate()
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
