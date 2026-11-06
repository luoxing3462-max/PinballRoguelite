using System;

namespace PinballRoguelite
{
    [Serializable]
    public sealed class BumperLayoutSettings
    {
        public const int DefaultNormalBumperCount = 12;
        public const int DefaultSpecialBumperCount = 4;

        public int NormalBumperCount { get; set; } = DefaultNormalBumperCount;
        public int SpecialBumperCount { get; set; } = DefaultSpecialBumperCount;
        public float HorizontalSpacingScale { get; set; } = 1f;
        public float VerticalSpacingScale { get; set; } = 1f;
        public float MinimumSpacing { get; set; } = 0.82f;

        public int TotalBumperCount => NormalBumperCount + SpecialBumperCount;

        public void Validate(int availablePointCount)
        {
            if (NormalBumperCount < 0 || SpecialBumperCount < 0 || TotalBumperCount <= 0)
                throw new InvalidOperationException("Bumper counts must be positive.");
            if (TotalBumperCount % 2 != 0 || SpecialBumperCount % 2 != 0)
                throw new InvalidOperationException("Staggered bumper layouts require even total and special bumper counts.");
            if (TotalBumperCount > availablePointCount)
                throw new InvalidOperationException("The configured bumper count exceeds the selected layout template capacity.");
            if (HorizontalSpacingScale <= 0f || VerticalSpacingScale <= 0f || MinimumSpacing <= 0f)
                throw new InvalidOperationException("Bumper layout spacing values must be positive.");
        }
    }
}
