using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSamples.Demo.Rolling.UI
{

    public class VirtualControlsDisabler : MonoBehaviour
    {
        private void OnEnable()
        {
#if (!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR
            gameObject.SetActive(false);
            return;
#endif
            InputSystem.onDeviceChange += OnDeviceChange;
            CheckCurrentDevices();
        }

        private void OnDisable()
        {
#if (UNITY_ANDROID || UNITY_IOS)
            InputSystem.onDeviceChange -= OnDeviceChange;
#endif
        }


        private void CheckCurrentDevices()
        {
            bool hasGamepad = Gamepad.current != null;
            bool hasKeyboardMouse = Keyboard.current != null || Mouse.current != null;

            gameObject.SetActive(!(hasGamepad || hasKeyboardMouse));
        }


        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Removed:
                    CheckCurrentDevices();
                    break;
            }
        }
    }
}
