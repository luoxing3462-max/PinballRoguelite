using UnityEngine;

namespace PinballRoguelite
{
    /// <summary>
    /// Keeps the runtime Rigidbody2D hierarchy below GameplayUI without inheriting
    /// Canvas Scaler position or scale. Its children remain world-space objects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayfieldPhysicsHost : MonoBehaviour
    {
        private void Awake() => AlignToWorldSpace();
        private void LateUpdate() => AlignToWorldSpace();

        public void PrepareForWorldPhysics() => AlignToWorldSpace();

        private void AlignToWorldSpace()
        {
            var parent = transform.parent;
            if (parent == null) return;

            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var scale = parent.lossyScale;
            transform.localScale = new Vector3(Inverse(scale.x), Inverse(scale.y), Inverse(scale.z));
        }

        private static float Inverse(float value) => Mathf.Abs(value) > 0.0001f ? 1f / value : 1f;
    }
}
