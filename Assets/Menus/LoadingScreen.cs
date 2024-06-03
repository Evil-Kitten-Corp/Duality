using System;
using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public class LoadingScreen : MonoBehaviour
    {
        public float timeout;
        public static Action OnLoad;

        private bool isLoading;
        
        private IEnumerator Start()
        {
            if (isLoading)
            {
                yield break;
            }

            isLoading = true;
            Debug.Log("Loading screen started."); 
            yield return new WaitForSeconds(timeout);
            Debug.Log("Loading screen timeout completed."); 
            //OnLoad?.Invoke();
            //OnLoad = null;
        }
    }
}