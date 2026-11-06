using System;
using UnityEngine;

namespace PinballRoguelite
{
    public static class CombatBalanceConfig
    {
        public const int PlayerBaseAttack = 8;
        public const float BaseCollisionDamage = 3f;
        public const int MaximumEffectiveCollisions = 8;
        public const int MaximumBaseDamage = 80;
        public const float MaximumPermanentDamageBonus = .60f;
        public const float MaximumExitMultiplierBonus = .75f;
        public const float NextAttackBonus = .20f;
        public static bool EnableDebugLogging = false;

        private static readonly int[] NormalHealthByTier = { 100, 120, 140, 160, 180, 210, 240 };
        private static readonly int[] EliteHealthByTier = { 190, 220, 250, 290, 330 };

        public static LevelConfig CreateLevel(MapNodeType type, int layerIndex, int totalLayers)
        {
            var tier = type == MapNodeType.Boss ? 8 : GetNormalizedTier(layerIndex, totalLayers);
            if (type == MapNodeType.Boss) return new LevelConfig { Title = "最终 Boss", TargetScore = 400, ShotLimit = 6, BumperPoints = 3, Accent = GamePalette.Red, DifficultyTier = tier, EnemyDamageMinimum = 20, EnemyDamageMaximum = 28 };
            if (type == MapNodeType.Elite)
            {
                var eliteTier = Mathf.Clamp(tier, 3, 7);
                return new LevelConfig { Title = "精英关", TargetScore = EliteHealthByTier[eliteTier - 3], ShotLimit = 5, BumperPoints = 3, Accent = GamePalette.Orange, DifficultyTier = tier, EnemyDamageMinimum = 16, EnemyDamageMaximum = 22 };
            }
            var normalTier = Mathf.Clamp(tier, 1, 7);
            var midGame = normalTier >= 3 && normalTier <= 5;
            return new LevelConfig { Title = "普通关", TargetScore = NormalHealthByTier[normalTier - 1], ShotLimit = 5, BumperPoints = 3, Accent = GamePalette.Blue, DifficultyTier = tier, EnemyDamageMinimum = normalTier <= 2 ? 6 : midGame ? 10 : 14, EnemyDamageMaximum = normalTier <= 2 ? 10 : midGame ? 14 : 18 };
        }

        public static int RollEnemyDamage(LevelConfig config, System.Random random) => random.Next(config.EnemyDamageMinimum, config.EnemyDamageMaximum + 1);
        public static int GetNormalizedTier(int layerIndex, int totalLayers) => Mathf.Clamp(Mathf.RoundToInt(1f + 6f * Mathf.Clamp01((float)layerIndex / Mathf.Max(1, totalLayers - 2))), 1, 7);
        public static void LogShot(LevelConfig config, string marbleKind, int launchCount, int collisions, float multiplier, float permanentBonus, float nextBonus, int finalDamage, int playerHealth)
        {
            if (!EnableDebugLogging) return;
            Debug.Log($"[Balance] 层{config.DifficultyTier} {config.Title} HP{config.TargetScore} | 第{launchCount}发 {marbleKind} | 碰撞{collisions}/{MaximumEffectiveCollisions} | 倍率×{multiplier:0.##} | 常驻+{permanentBonus:P0} | 下次+{nextBonus:P0} | 伤害{finalDamage} | 玩家HP{playerHealth}");
        }
    }
}
