using UnityEngine;
using UnityEngine.Networking;

public class Fireball : NetworkBehaviour
{
    private const float Duration = 5f;

    private float _counter = 0;
    
	void Update ()
	{
	    if (isServer)
	    {
	        _counter += Time.deltaTime;
	        if (_counter > Duration)
	        {
	            NetworkServer.Destroy(gameObject);
	        }
	    }
	}
}
