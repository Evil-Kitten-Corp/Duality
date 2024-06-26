﻿using Unity.Netcode;
using UnityEngine;

namespace Try
{
    public class NetworkHelper : MonoBehaviour
    {
        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
        }
        
        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }
        
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }
    }
}