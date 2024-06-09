using System;
using UnityEngine;

namespace Test
{
    public enum ButtonActions : byte
    {
        LobbyReady,
        LobbyNotReady,
        RestartReady,
        WinReady
    }
    
    public class ConfirmationButton : MonoBehaviour
    {
        public static Action<ButtonActions> OnButtonPress;

        [SerializeField] private ButtonActions buttonAction;

        public void OnPress()
        {
            OnButtonPress?.Invoke(buttonAction);
        }
    }
}