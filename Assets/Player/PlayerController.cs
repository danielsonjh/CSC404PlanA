using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public GameObject FireballPrefab;

    private const float FireballSpeed = 10f;

    private Vector3 _dir;
    private bool _shoot;

	void Start ()
	{
	    StartCoroutine(UnscaledUpdate());
	}

    void Update()
    {
        _dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        _shoot = Input.GetKeyUp(KeyCode.A);
    }

    IEnumerator UnscaledUpdate()
	{
	    while (true)
	    {
            yield return new WaitForEndOfFrame();
            
            if (_shoot)
	        {
	            var fireball = Instantiate(FireballPrefab);
	            fireball.transform.position = transform.position;
	            fireball.GetComponent<Rigidbody>().velocity = _dir * FireballSpeed;
	        }
	    }
	}
}
