using UnityEngine;

namespace AdieLab.PineMorphLab
{
    public sealed class PineMorphOrbitCamera : MonoBehaviour
    {
        private Vector3 target = new Vector3(0f, 0.92f, 1.62f);
        private float yaw = -23f;
        private float pitch = 18f;
        private float distance = 6.35f;
        private PineMorphApp app;
        private Camera orbitCamera;

        private void Awake()
        {
            app = FindAnyObjectByType<PineMorphApp>();
            orbitCamera = GetComponent<Camera>();
        }

        public void Initialize(PineMorphApp owner)
        {
            app = owner;
        }

        private void LateUpdate()
        {
            bool insideViewport = orbitCamera != null
                && orbitCamera.pixelRect.Contains(Input.mousePosition)
                && (app == null || !app.TutorialVisible);
            if (Input.GetMouseButton(0) && insideViewport)
            {
                Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                yaw += delta.x * 2.4f;
                pitch = Mathf.Clamp(pitch - delta.y * 2.1f, -5f, 58f);
                if (delta.sqrMagnitude > 0.0001f)
                {
                    app?.NotifyCameraRotated();
                }
            }

            if (insideViewport)
            {
                float scroll = Input.mouseScrollDelta.y;
                distance = Mathf.Clamp(distance - scroll * 0.55f, 4.6f, 10.0f);
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    app?.NotifyCameraZoomed();
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                yaw = -23f;
                pitch = 18f;
                distance = 6.35f;
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
