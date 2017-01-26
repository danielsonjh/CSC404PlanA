using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ConnectionManager : NetworkBehaviour
{
    private NetworkManager _networkManager;
    private InputField _ipInputField;
    private InputField _portInputField;

    void Start ()
	{
	    _networkManager = FindObjectOfType<NetworkManager>();
	    _ipInputField = GameObject.Find("IpInput").GetComponent<InputField>();
	    _portInputField = GameObject.Find("PortInput").GetComponent<InputField>();
	    HandleHeadlessServerMode();
	}

    public void StartHost()
    {
        _networkManager.StartHost();
        Hide();
    }

    public void StartServer()
    {
        _networkManager.StartServer();
        Hide();
    }

    public void StartClient()
    {
        _networkManager.StartClient();
        Hide();
    }

    public void RemoteConnect()
    {
        Debug.Log(string.Format("Connecting to remote host at {0}:{1}", _ipInputField.text, _portInputField.text));
        _networkManager.networkAddress = _ipInputField.text;
        _networkManager.networkPort = Convert.ToInt32(_portInputField.text);
        _networkManager.StartClient();
        Hide();
    }

    private void HandleHeadlessServerMode()
    {
        var args = Environment.GetCommandLineArgs();
        Debug.Log("Command line args: " + string.Join(", ", args));
        if (args.Length == 4)
        {
            try
            {
                var port = Convert.ToInt32(args[3]);
                Debug.Log("Using port: " + port);
                _networkManager.networkPort = port;
                _networkManager.StartServer();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Application.Quit();
            }
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
