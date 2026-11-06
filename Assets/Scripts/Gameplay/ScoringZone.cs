using UnityEngine;

namespace PinballRoguelite
{
    public enum ScoringZoneMode
    {
        Fixed,
        Random
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ScoringZone : MonoBehaviour
    {
        public ScoringZoneMode Mode { get; private set; } = ScoringZoneMode.Fixed;
        public float Multiplier { get; private set; } = 1f;
        public float RandomMinimum { get; private set; } = 1.25f;
        public float RandomMaximum { get; private set; } = 2f;
        public float LastResolvedMultiplier { get; private set; } = -1f;
        public bool IsStarExit { get; private set; }
        public event System.Action<Marble> StarEntered;
        public string DisplayLabel => Mode == ScoringZoneMode.Random ? "×?" : $"×{Multiplier}";
        private SpriteRenderer visual;

        private void Awake()
        {
            var collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            visual = GetComponentInChildren<SpriteRenderer>();
        }

        public void Configure(float multiplier, Vector2 size, Color color)
        {
            ConfigureFixed(multiplier, size, color);
        }

        public void ConfigureFixed(float multiplier, Vector2 size, Color color)
        {
            IsStarExit = false;
            Mode = ScoringZoneMode.Fixed;
            Multiplier = Mathf.Max(0f, multiplier);
            LastResolvedMultiplier = -1;
            ConfigureVisual(size, color);
            gameObject.name = $"ScoreZone_x{Multiplier}";
        }

        public void ConfigureRandom(float minimum, float maximum, Vector2 size, Color color)
        {
            IsStarExit = false;
            Mode = ScoringZoneMode.Random;
            RandomMinimum = Mathf.Max(0f, minimum);
            RandomMaximum = Mathf.Max(RandomMinimum, maximum);
            Multiplier = -1f;
            LastResolvedMultiplier = -1;
            ConfigureVisual(size, color);
            gameObject.name = $"ScoreZone_Random_{RandomMinimum}_{RandomMaximum}";
        }

        public void ConfigureStar(Vector2 size, Color color)
        {
            IsStarExit = true;
            Mode = ScoringZoneMode.Fixed;
            Multiplier = 0f;
            LastResolvedMultiplier = 0f;
            ConfigureVisual(size, color);
            gameObject.name = "ScoreZone_Star";
        }

        public float ResolveMultiplier()
        {
            LastResolvedMultiplier = Mode == ScoringZoneMode.Random
                ? Random.Range(RandomMinimum, RandomMaximum)
                : Multiplier;
            return LastResolvedMultiplier;
        }

        private void ConfigureVisual(Vector2 size, Color color)
        {
            GetComponent<BoxCollider2D>().size = size;
            if (visual == null)
                visual = SpriteFactory.AddVisual(transform, SpriteFactory.Square, color, size, -2);
            else
            {
                visual.color = color;
                visual.transform.localScale = new Vector3(size.x, size.y, 1f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var marble = other.GetComponent<Marble>();
            if (marble == null) return;
            if (IsStarExit)
            {
                StarEntered?.Invoke(marble);
                marble.Settle(0, true);
                return;
            }
            marble.Settle(ResolveMultiplier());
        }
    }
}
