using System;
using UnityEngine;

namespace Try
{
    public class GameControllerButton : MonoBehaviour
    {
        public enum ButtonType : byte
        {
            RestartReady,
            WinReady
        }
        
        public static Action<ButtonType> OnButtonPress;

        [SerializeField] private ButtonType buttonType;

        public void OnPress()
        {
            OnButtonPress?.Invoke(buttonType);
        }
    }
}