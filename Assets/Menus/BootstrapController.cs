using DefaultNamespace;
using UnityEngine;

public class BootstrapController : MonoBehaviour
{
    public void Signup()
    {
        SceneManagement.Instance.Register();
    }
    
    public void Login(LoginAuth auth)
    {
        SceneManagement.Instance.TryLogin(auth);
    }
    
    public void GuestMode()
    {
        SceneManagement.Instance.GoToMainMenu();
    }
}
