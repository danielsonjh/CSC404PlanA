using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LatencyHud : MonoBehaviour {

    private NetworkManager _networkManager;
    private Text _text;

    void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        _text = GetComponent<Text>();
    }
	
	void Update ()
	{
	    if (_networkManager.client != null)
	    {	        
	        _text.text = _networkManager.client.GetRTT() + "ms";
        }
	}
}
