using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class LoginAuth : MonoBehaviour
    {
        public TMP_Text error;
        public TMP_InputField username;
        public TMP_InputField password;
        
        private DatabaseManager _dbManager;

        void Start()
        {
            _dbManager = FindObjectOfType<DatabaseManager>();
        }

        public bool Authenticate()
        {
            string username = this.username.text;
            string password = this.password.text;

            if (_dbManager.Login(username, password))
            {
                error.text = "Login successful!";
                return true;
            }

            error.text = "Invalid username or password!";
            return false;
        }
    }
}