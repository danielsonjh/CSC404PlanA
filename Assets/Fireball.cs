using UnityEngine;

public class Fireball : MonoBehaviour
{
    private const float Duration = 5f;

    private float _counter = 0;
    
	void Update ()
	{
	    _counter += Time.deltaTime;
	    if (_counter > Duration)
	    {
	        Destroy(gameObject);
	    }
	}
}
