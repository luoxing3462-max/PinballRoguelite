using UnityEngine;

namespace PinballRoguelite
{
    [RequireComponent(typeof(Camera))]
    public sealed class PortraitBoardCameraFitter : MonoBehaviour
    {
        // Covers the left board wall and the outer launch rail without changing any board coordinates.
        public const float BoardHalfWidth = 5.0f;
        public const float MinimumOrthographicSize = 9.6f;

        private Camera targetCamera;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            Apply();
        }

        private void Update()
        {
            if (lastWidth != Screen.width || lastHeight != Screen.height) Apply();
        }

        public static float CalculateOrthographicSize(int width, int height)
        {
            if (width <= 0 || height <= 0) return MinimumOrthographicSize;
            var aspect = (float)width / height;
            return Mathf.Max(MinimumOrthographicSize, BoardHalfWidth / aspect);
        }

        public void Apply()
        {
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = CalculateOrthographicSize(Screen.width, Screen.height);
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }
}
