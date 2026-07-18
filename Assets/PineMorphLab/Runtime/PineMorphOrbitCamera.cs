using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AdieLab.PineMorphLab
{
    public sealed class PineMorphOrbitCamera : MonoBehaviour
    {
        private Vector3 target = new Vector3(0f, 1.15f, 2.1f);
        private float yaw = -23f;
        private float pitch = 18f;
        private float distance = 7.8f;

        private void LateUpdate()
        {
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (mouse.leftButton.isPressed && !overUi)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    yaw += delta.x * 0.16f;
                    pitch = Mathf.Clamp(pitch - delta.y * 0.14f, -5f, 58f);
                }

                if (!overUi)
                {
                    distance = Mathf.Clamp(distance - mouse.scroll.ReadValue().y * 0.004f, 5.2f, 11.5f);
                }
            }

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                yaw = -23f;
                pitch = 18f;
                distance = 7.8f;
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target - rotation * Vector3.forward * distance;
            transform.rotation = rotation;
        }

        public void SetVerificationView()
        {
            yaw = 34f;
            pitch = 31f;
            distance = 6.5f;
        }
    }
}
