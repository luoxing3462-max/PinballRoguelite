using System.Collections.Generic;

namespace PinballRoguelite
{
    public sealed class ScoreManager
    {
        public int TotalScore { get; private set; }
        private readonly Dictionary<int, int> activeMarbles = new Dictionary<int, int>();
        public int EffectiveCollisionCount { get; private set; }
        public float ExitMultiplier { get; private set; } = 1f;
        private int baseAttack = CombatBalanceConfig.PlayerBaseAttack;
        private float collisionDamage = CombatBalanceConfig.BaseCollisionDamage;
        private float accumulatedCollisionAttack;
        private bool hasSettledExit;

        public void Reset()
        {
            TotalScore = 0;
            BeginLaunch();
        }

        public void BeginLaunch(int launchBaseAttack = CombatBalanceConfig.PlayerBaseAttack, float launchCollisionDamage = CombatBalanceConfig.BaseCollisionDamage)
        {
            activeMarbles.Clear();
            EffectiveCollisionCount = 0;
            ExitMultiplier = 1f;
            baseAttack = launchBaseAttack;
            collisionDamage = launchCollisionDamage;
            accumulatedCollisionAttack = 0f;
            hasSettledExit = false;
        }

        public void RegisterMarble(int id, int startingScore = 0) => activeMarbles[id] = 0;

        public int DetachMarble(int id)
        {
            activeMarbles.Remove(id);
            return 0;
        }

        public void AddBumperScore(int id, float collisionMultiplier)
        {
            if (!activeMarbles.ContainsKey(id) || EffectiveCollisionCount >= CombatBalanceConfig.MaximumEffectiveCollisions) return;
            EffectiveCollisionCount++;
            accumulatedCollisionAttack += collisionDamage * UnityEngine.Mathf.Max(0f, collisionMultiplier);
        }

        public void AddBumperScore(int id, int points)
        {
            AddBumperScore(id, 1f);
        }

        public int SettleMarble(int id, float multiplier)
        {
            if (activeMarbles.ContainsKey(id))
            {
                ExitMultiplier = hasSettledExit ? System.Math.Max(ExitMultiplier, multiplier) : multiplier;
                hasSettledExit = true;
            }
            activeMarbles.Remove(id);
            return PendingPreview();
        }

        public int PendingPreview()
        {
            var baseDamage = System.Math.Min(CombatBalanceConfig.MaximumBaseDamage, baseAttack + accumulatedCollisionAttack);
            return UnityEngine.Mathf.RoundToInt(baseDamage * ExitMultiplier);
        }

        public int FinishLaunch()
        {
            var awarded = PendingPreview();
            TotalScore += awarded;
            BeginLaunch();
            return awarded;
        }
    }
}
