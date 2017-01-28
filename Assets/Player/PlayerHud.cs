using UnityEngine;

public class PlayerHud : MonoBehaviour {
	
	void Update ()
	{
	    transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
	}
}
