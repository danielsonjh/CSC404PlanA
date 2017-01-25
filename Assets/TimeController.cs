using System.Collections;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController Instance;

    public float DeltaTime;
    public float MoveTime;
    public float FreezeTime;

    public GameObject Bar;
    public Material MoveBarMaterial;
    public Material FreezeBarMaterial;

    public TimeState State = TimeState.Move;
    public float Counter = 0;

    private float _prevRealtime;

    void Awake()
    {
        Instance = this;
    }

	void Start ()
	{
	    _prevRealtime = Time.realtimeSinceStartup;

        StartCoroutine(UnscaledUpdate());
	}
	
	IEnumerator UnscaledUpdate ()
	{
	    while (true)
	    {
            yield return new WaitForEndOfFrame();
	        var currRealtime = Time.realtimeSinceStartup;
	        DeltaTime = currRealtime - _prevRealtime;
	        _prevRealtime = currRealtime;
	        Counter += DeltaTime;

	        var maxCounter = State == TimeState.Move ? MoveTime : FreezeTime;
    	    var p = Counter / maxCounter;
            Bar.transform.localScale = new Vector3(1 - p, Bar.transform.localScale.y, Bar.transform.localScale.z);
            Bar.transform.localPosition = new Vector3(-p/2, Bar.transform.localPosition.y, Bar.transform.localPosition.z);
            
            if (State == TimeState.Freeze && Counter >= FreezeTime)
    	    {
	            State = TimeState.Move;
	            Time.timeScale = 1;
	            Bar.GetComponent<MeshRenderer>().material = MoveBarMaterial;
    	        Counter = 0;
                ScreenShaker.Instance.Shake();
            }

            else if (State == TimeState.Move && Counter >= MoveTime)
    	    {
	            State = TimeState.Freeze;
	            Time.timeScale = 0.1f;
	            Bar.GetComponent<MeshRenderer>().material = FreezeBarMaterial;
                Counter = 0;
                ScreenShaker.Instance.Shake();
            }
        }
	}
}

public enum TimeState
{
    Move, Freeze
}