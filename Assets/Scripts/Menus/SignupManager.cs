using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SignupManager : MonoBehaviour
{
    public TMP_Text errorText;
    public TMP_InputField username;
    public TMP_InputField password;
    public TMP_InputField confirmPassword;
    
    private DatabaseManager _dbManager;
    
    public void ReturnToBoot()
    {
        SceneManagement.Instance.LoadSceneAs("Bootstrap", null);
    }

    void Start()
    {
        _dbManager = FindObjectOfType<DatabaseManager>();
    }

    public void ValidateRegister()
    {
        string username = this.username.text;
        string password = this.password.text;
        string confirmPassword = this.confirmPassword.text;

        if (password != confirmPassword)
        {
            errorText.text = "Passwords do not match!";
            return;
        }

        string hashedPassword = Sha1Hash(password);

        if (_dbManager.Register(username, hashedPassword))
        {
            errorText.text = "Registration successful!";
            SceneManagement.Instance.LoginSuccessful(username);
        }
        else
        {
            errorText.text = "Username already exists!";
        }
    }

    private string Sha1Hash(string input)
    {
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = sha1.ComputeHash(bytes);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
