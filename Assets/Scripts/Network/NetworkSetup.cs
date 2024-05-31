using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

public class NetworkSetup : MonoBehaviour
{
    public bool Server
    {
        get => _isServer;
    }
    
    bool _isServer;
    
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    // Delegate to filter windows
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private static IntPtr FindWindowByProcessId(uint processId)
    {
        IntPtr windowHandle = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out var windowProcessId);
            if (windowProcessId == processId)
            {
                windowHandle = hWnd;
                return false; // Found the window, stop enumerating
            }

            return true; // Continue enumerating
        }, IntPtr.Zero);
        return windowHandle;
    }

    static void SetWindowTitle(string title)
    {
#if !UNITY_EDITOR
        uint processId = (uint)Process.GetCurrentProcess().Id;
        IntPtr hWnd = FindWindowByProcessId(processId);
        if (hWnd != IntPtr.Zero)
        {
            SetWindowText(hWnd, title);
        }
#endif
    }

#else
      static void SetWindowTitle(string title)
      {
      }
#endif
    
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
            SetWindowTitle("Duality (Server)");
        }
        else
        {
            StartCoroutine(StartAsClient());
            SetWindowTitle("Duality (Client)");
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
