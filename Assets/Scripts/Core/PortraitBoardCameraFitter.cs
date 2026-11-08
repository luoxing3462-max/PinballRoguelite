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
        private Rect viewport = new Rect(0f, 0f, 1f, 1f);
        private float worldHeightOverride = -1f;
        private Vector3 defaultPosition;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            defaultPosition = transform.position;
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
            targetCamera.rect = viewport;
            targetCamera.orthographicSize = worldHeightOverride > 0f
                ? worldHeightOverride * 0.5f
                : CalculateOrthographicSize(Screen.width, Screen.height);
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }

        public void SetPlayfieldViewport(Rect normalizedViewport, float worldHeight, Vector2 worldCenter)
        {
            viewport = normalizedViewport;
            worldHeightOverride = Mathf.Max(0.01f, worldHeight);
            transform.position = new Vector3(worldCenter.x, worldCenter.y, defaultPosition.z);
            Apply();
        }

        public void ResetViewport()
        {
            viewport = new Rect(0f, 0f, 1f, 1f);
            worldHeightOverride = -1f;
            transform.position = defaultPosition;
            Apply();
        }
    }
}
