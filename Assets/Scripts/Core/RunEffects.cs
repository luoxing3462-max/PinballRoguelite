using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinballRoguelite
{
    public enum RunEffectId { AttackUp, CollisionUp, SplitUp, MultiplierUp, ShotUp, Heal, BlessAttack, BlessCollision, BlessRepair, CurseOverload, CurseSplit, CurseUnstable, StarCoins, StarNextAttack, StarCollision }

    public readonly struct EffectOffer
    {
        public readonly RunEffectId Id;
        public readonly string Name;
        public readonly string Description;
        public EffectOffer(RunEffectId id, string name, string description) { Id = id; Name = name; Description = description; }
    }

    public sealed class RunEffects
    {
        public const int StartingHealth = 100;
        public const int MaximumExtraSplitChildren = 2;
        public const float MaximumCollisionBonus = 4f;
        public int CurrentHealth { get; private set; } = StartingHealth;
        public int MaximumHealth { get; private set; } = StartingHealth;
        public int Coins { get; private set; }
        public float CollisionBonus { get; private set; }
        public int ExtraSplitChildren { get; private set; }
        public float ExitMultiplierBonus { get; private set; }
        public int ShotBonus { get; private set; }
        public float PermanentDamageBonus { get; private set; }
        public float NextAttackBonus { get; private set; }
        public float PermanentDamageMultiplier => 1f + PermanentDamageBonus;
        public float NextAttackMultiplier => 1f + NextAttackBonus;
        public float EnemyDamageMultiplier { get; private set; } = 1f;
        public bool DangerousSplit { get; private set; }
        public string LastApplied { get; private set; } = "无";

        public void Reset()
        {
            CurrentHealth = StartingHealth; MaximumHealth = StartingHealth; Coins = 0; CollisionBonus = 0;
            ExtraSplitChildren = 0; ExitMultiplierBonus = 0f; ShotBonus = 0; PermanentDamageBonus = 0f;
            NextAttackBonus = 0f; EnemyDamageMultiplier = 1f; DangerousSplit = false; LastApplied = "无";
        }

        public void Heal(int amount) => CurrentHealth = Mathf.Min(MaximumHealth, CurrentHealth + Mathf.Max(0, amount));
        public void TakeDamage(int amount) => CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Max(0, amount));
        public void SyncHealth(int health) => CurrentHealth = Mathf.Clamp(health, 0, MaximumHealth);
        public float ModifyExitMultiplier(float multiplier) => multiplier <= 0f ? 0f : multiplier + ExitMultiplierBonus;
        public int ModifyEnemyDamage(int damage) => Mathf.RoundToInt(Mathf.Max(0, damage) * EnemyDamageMultiplier);
        public int ResolvePlayerDamage(int rawDamage)
        {
            var result = Mathf.RoundToInt(Mathf.Max(0, rawDamage) * (1f + PermanentDamageBonus + NextAttackBonus));
            NextAttackBonus = 0f;
            return result;
        }

        public void Apply(RunEffectId id)
        {
            switch (id)
            {
                case RunEffectId.AttackUp: PermanentDamageBonus = Mathf.Min(CombatBalanceConfig.MaximumPermanentDamageBonus, PermanentDamageBonus + .10f); break;
                case RunEffectId.CollisionUp: CollisionBonus = Mathf.Min(MaximumCollisionBonus, CollisionBonus + .5f); break;
                case RunEffectId.SplitUp: ExtraSplitChildren = Mathf.Min(MaximumExtraSplitChildren, ExtraSplitChildren + 1); break;
                case RunEffectId.MultiplierUp: ExitMultiplierBonus = Mathf.Min(CombatBalanceConfig.MaximumExitMultiplierBonus, ExitMultiplierBonus + .15f); break;
                case RunEffectId.ShotUp: ShotBonus++; break;
                case RunEffectId.Heal: Heal(20); break;
                case RunEffectId.BlessAttack: PermanentDamageBonus = Mathf.Min(CombatBalanceConfig.MaximumPermanentDamageBonus, PermanentDamageBonus + .10f); break;
                case RunEffectId.BlessCollision: CollisionBonus = Mathf.Min(MaximumCollisionBonus, CollisionBonus + .5f); break;
                case RunEffectId.BlessRepair: Heal(25); break;
                case RunEffectId.CurseOverload: PermanentDamageBonus = Mathf.Min(CombatBalanceConfig.MaximumPermanentDamageBonus, PermanentDamageBonus + .25f); MaximumHealth = Mathf.Max(1, MaximumHealth - 15); CurrentHealth = Mathf.Min(CurrentHealth, MaximumHealth); break;
                case RunEffectId.CurseSplit: ExtraSplitChildren = Mathf.Min(MaximumExtraSplitChildren, ExtraSplitChildren + 1); DangerousSplit = true; break;
                case RunEffectId.CurseUnstable: ExitMultiplierBonus = Mathf.Min(CombatBalanceConfig.MaximumExitMultiplierBonus, ExitMultiplierBonus + .25f); EnemyDamageMultiplier += .20f; break;
                case RunEffectId.StarCoins: Coins += 12; break;
                case RunEffectId.StarNextAttack: NextAttackBonus = Mathf.Max(NextAttackBonus, CombatBalanceConfig.NextAttackBonus); break;
                case RunEffectId.StarCollision: CollisionBonus = Mathf.Min(MaximumCollisionBonus, CollisionBonus + .5f); break;
            }
            LastApplied = GetOffer(id).Name;
        }

        public string StatusText => $"HP {CurrentHealth}/{MaximumHealth}  金币 {Coins}\n攻+{PermanentDamageBonus:P0}  碰撞+{CollisionBonus:0.0}  倍率+{ExitMultiplierBonus:0.00}  分裂+{ExtraSplitChildren}  发射+{ShotBonus}" + (NextAttackBonus > 0f ? "\n下次攻击+20%" : "") + (DangerousSplit ? "  危险分裂" : "") + (EnemyDamageMultiplier > 1f ? $"  怪伤×{EnemyDamageMultiplier:0.00}" : "");

        public static EffectOffer GetOffer(RunEffectId id)
        {
            switch (id)
            {
                case RunEffectId.AttackUp: return new EffectOffer(id, "攻击强化", "最终攻击伤害 +10%");
                case RunEffectId.CollisionUp: return new EffectOffer(id, "碰撞强化", "每次有效碰撞基础攻击 +0.5");
                case RunEffectId.SplitUp: return new EffectOffer(id, "分裂强化", "分裂弹珠额外产生 1 颗子珠（上限 2）");
                case RunEffectId.MultiplierUp: return new EffectOffer(id, "倍率强化", "普通出口倍率 +0.15");
                case RunEffectId.ShotUp: return new EffectOffer(id, "发射强化", "每关最大发射次数 +1");
                case RunEffectId.Heal: return new EffectOffer(id, "生命恢复", "立即恢复 20 HP");
                case RunEffectId.BlessAttack: return new EffectOffer(id, "强力弹射", "最终攻击伤害 +10%");
                case RunEffectId.BlessCollision: return new EffectOffer(id, "连锁强化", "每次有效碰撞攻击 +0.5");
                case RunEffectId.BlessRepair: return new EffectOffer(id, "机械修复", "恢复 25 HP");
                case RunEffectId.CurseOverload: return new EffectOffer(id, "过载弹射", "攻击 +25%，最大 HP -15");
                case RunEffectId.CurseSplit: return new EffectOffer(id, "危险分裂", "额外 1 子珠；每次攻击后失去 5 HP");
                case RunEffectId.CurseUnstable: return new EffectOffer(id, "不稳定倍率", "出口倍率 +0.25；怪物伤害 +20%");
                case RunEffectId.StarCoins: return new EffectOffer(id, "星星金币", "获得 12 本轮金币");
                case RunEffectId.StarNextAttack: return new EffectOffer(id, "星星增幅", "下一次攻击最终伤害 +20%");
                default: return new EffectOffer(id, "星星碰撞", "每次有效碰撞永久 +0.5（有上限）");
            }
        }

        public static List<EffectOffer> ChooseUpgrades(bool elite, int seed)
        {
            var pool = new List<RunEffectId> { RunEffectId.AttackUp, RunEffectId.CollisionUp, RunEffectId.SplitUp, RunEffectId.MultiplierUp, RunEffectId.ShotUp, RunEffectId.Heal };
            if (elite) { pool.Add(RunEffectId.AttackUp); pool.Add(RunEffectId.SplitUp); }
            return Pick(pool, 3, seed);
        }

        public static List<EffectOffer> ChooseEvent(int seed)
        {
            return Pick(new List<RunEffectId> { RunEffectId.BlessAttack, RunEffectId.BlessCollision, RunEffectId.BlessRepair, RunEffectId.CurseOverload, RunEffectId.CurseSplit, RunEffectId.CurseUnstable }, 2 + Mathf.Abs(seed % 2), seed);
        }

        public static EffectOffer ChooseStarReward(int seed)
        {
            return Pick(new List<RunEffectId> { RunEffectId.StarCoins, RunEffectId.StarNextAttack, RunEffectId.StarCollision }, 1, seed)[0];
        }

        private static List<EffectOffer> Pick(List<RunEffectId> pool, int count, int seed)
        {
            var random = new System.Random(seed); var result = new List<EffectOffer>();
            while (result.Count < count && pool.Count > 0) { var index = random.Next(pool.Count); var id = pool[index]; pool.RemoveAll(value => value == id); result.Add(GetOffer(id)); }
            return result;
        }
    }
}
