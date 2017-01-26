using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public GameObject FireballPrefab;

    private const float MoveForce = 40f;
    private const float FireballSpeed = 10f;

    private Vector3 _lookDir;
    private Vector3 _moveDir;

    private bool _move;
    private bool _shoot;

    private Action _chosenAction;

	void Start ()
	{
	    if (isClient)
	    {
	        ClientScene.RegisterPrefab(FireballPrefab);
            TimeController.Instance.OnToggle += OnTimeControllerToggleHandler;
	        StartCoroutine(UnscaledUpdate());

            Debug.Log(string.Join(", ", Input.GetJoystickNames()));
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
            var h = Input.GetAxisRaw("Horizontal");
            var v = Input.GetAxisRaw("Vertical");
            var rawDir = new Vector3(h, 0, v);
            _lookDir = rawDir.normalized;
            _moveDir = rawDir.magnitude < 0.05f ? Vector3.zero : rawDir.normalized;

            _move = Input.GetButtonDown("Fire1");
            _shoot = Input.GetButtonDown("Fire2");
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
