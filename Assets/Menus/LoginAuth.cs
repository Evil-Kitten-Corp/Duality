using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class LoginAuth : MonoBehaviour
    {
        public GameObject error;
        public TMP_InputField username;
        public TMP_InputField password;

        public bool Authenticate()
        {
            return false;
        }
    }
}