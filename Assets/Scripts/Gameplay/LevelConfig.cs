using UnityEngine;

namespace PinballRoguelite
{
    public sealed class LevelConfig
    {
        public string Title;
        public int TargetScore;
        public int ShotLimit;
        public int BumperPoints;
        public Color Accent;
        public int DifficultyTier;
        public int EnemyDamageMinimum;
        public int EnemyDamageMaximum;

        public static LevelConfig For(MapNodeType type)
        {
            return CombatBalanceConfig.CreateLevel(type, 0, 2);
        }
    }
}
