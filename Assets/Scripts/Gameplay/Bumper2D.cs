using UnityEngine;

namespace PinballRoguelite
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class Bumper2D : MonoBehaviour
    {
        public int Points { get; private set; } = 10;
        public bool SplitTrigger { get; private set; }
        public const float Radius = 0.28f;
        private CircleCollider2D bumperCollider;
        private SpriteRenderer visual;

        private void Awake()
        {
            bumperCollider = GetComponent<CircleCollider2D>();
            bumperCollider.radius = Radius;
            ApplyPhysicsMaterial(false);
            visual = GetComponentInChildren<SpriteRenderer>();
            if (visual == null)
                visual = SpriteFactory.AddVisual(transform, SpriteFactory.Circle, Color.white,
                    Vector2.one * (Radius * 2f), 2);
        }

        public void Configure(int points, bool splitTrigger)
        {
            Points = points;
            SplitTrigger = splitTrigger;
            ApplyPhysicsMaterial(splitTrigger);
            if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();
            if (visual != null)
                visual.color = splitTrigger ? GamePalette.Pink : new Color(0.93f, 0.97f, 1f);
            gameObject.name = splitTrigger ? "SplitBumper" : "Bumper";
        }

        private void ApplyPhysicsMaterial(bool splitTrigger)
        {
            if (bumperCollider == null) bumperCollider = GetComponent<CircleCollider2D>();
            bumperCollider.sharedMaterial = new PhysicsMaterial2D(splitTrigger ? "SplitBumperMaterial" : "BumperMaterial")
            {
                bounciness = splitTrigger ? 0.91f : 0.82f,
                friction = splitTrigger ? 0.01f : 0.035f
            };
        }
    }
}
