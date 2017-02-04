using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


[NetworkSettings(channel = 1, sendInterval = 0)]
public class TimeController : NetworkBehaviour
{
    public static TimeController Instance;

    public event Action OnToggle;

    public const float MoveTime = 0.5f;
    public const float FreezeTime = 2.0f;

    public bool IsMove { get { return _state == TimeState.Move; } }
    public bool IsFreeze { get { return _state == TimeState.Freeze; } }
    [SyncVar] public float DeltaTime;

    public GameObject Bar;
    public Material MoveBarMaterial;
    public Material FreezeBarMaterial;
    
    private float MaxCounter { get {return IsMove ? MoveTime : FreezeTime; } }
    private float _prevRealtime;

    [SyncVar] private TimeState _state = TimeState.Move;
    [SyncVar] private float _counter;


    void Awake()
    {
        Instance = this;
    }

	void Start ()
	{
        LightController.Instance.SetOnToggleListener();
	    _prevRealtime = Time.realtimeSinceStartup;

        StartCoroutine(UnscaledUpdate());
	}

    private IEnumerator UnscaledUpdate ()
	{
	    while (true)
	    {
            yield return new WaitForEndOfFrame();
            
    	    var p = Mathf.Clamp01(_counter / MaxCounter);
            Bar.transform.localScale = new Vector3(1 - p, Bar.transform.localScale.y, Bar.transform.localScale.z);
            Bar.transform.localPosition = new Vector3(-p/2, Bar.transform.localPosition.y, Bar.transform.localPosition.z);

	        if (isServer)
	        {
	            var currRealtime = Time.realtimeSinceStartup;
	            DeltaTime = currRealtime - _prevRealtime;
	            _prevRealtime = currRealtime;
	            _counter += DeltaTime;

	            if (_counter >= MaxCounter)
	            {
                    if (!isClient)
                    {
                        ToggleTimeState();
                    }
                    RpcToggleTimeState();
	            }
	        }
	    }
	}

    private void ToggleTimeState()
    {
        _state = _state.Other();
        Time.timeScale = IsMove ? 1 : 0.05f;
        Bar.GetComponent<MeshRenderer>().material = IsMove ? MoveBarMaterial : FreezeBarMaterial;
        _counter = 0;

        if (OnToggle != null) OnToggle.Invoke();
    }

    [ClientRpc]
    private void RpcToggleTimeState()
    {
        ToggleTimeState();
    }
}

public enum TimeState
{
    Move, Freeze
}

public static class TimeStateExtensions
{
    public static TimeState Other(this TimeState timeState)
    {
        return timeState == TimeState.Move ? TimeState.Freeze : TimeState.Move;
    }
}