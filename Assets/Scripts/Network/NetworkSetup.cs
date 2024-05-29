using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkSetup : MonoBehaviour
{
    bool _isServer;
    
    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--server")
            {
                _isServer = true;
            }
        }

        if (_isServer)
        {
            StartCoroutine(StartAsServer());
        }
        else
        {
            StartCoroutine(StartAsClient());
        }
    }

    IEnumerator StartAsServer()
    {
        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        var transport = GetComponent<UnityTransport>();
        transport.enabled = true;

        yield return null;

        if (networkManager.StartServer())
        {
            Debug.Log($"Serving on port {transport.ConnectionData.Port}.");
        }
        else
        {
            Debug.LogError($"Failed to serve on port {transport.ConnectionData.Port}.");
        }
    }

    IEnumerator StartAsClient()
    {
        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        var transport = GetComponent<UnityTransport>();
        transport.enabled = true;

        yield return null;

        if (networkManager.StartClient())
        {
            Debug.Log($"Connecting on port {transport.ConnectionData.Port}.");
        }
        else
        {
            Debug.LogError($"Failed to connect on port {transport.ConnectionData.Port}.");
        }
    }
}
