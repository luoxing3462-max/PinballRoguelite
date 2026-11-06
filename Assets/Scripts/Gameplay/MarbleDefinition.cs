using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinballRoguelite
{
    [Serializable]
    public sealed class MarbleDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public Color Color { get; }
        public bool CanSplit { get; }
        public int EnhancedCollisionCount { get; }
        public float EnhancedCollisionMultiplier { get; }
        public float BaseCollisionMultiplier { get; }
        public float ChainMultiplierPerHit { get; }
        public float GravityScaleMultiplier { get; }
        public float Bounciness { get; }
        public int PierceNormalBumperCount { get; }
        public int ExtraStarRewards { get; }
        public float SplitChildCollisionMultiplier { get; }

        public MarbleDefinition(string id, string displayName, string description, Color color, bool canSplit = false,
            int enhancedCollisionCount = 0, float enhancedCollisionMultiplier = 1f, float baseCollisionMultiplier = 1f,
            float chainMultiplierPerHit = 0f, float gravityScaleMultiplier = 1f, float bounciness = 0.64f,
            int pierceNormalBumperCount = 0, int extraStarRewards = 0, float splitChildCollisionMultiplier = 1f)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Color = color;
            CanSplit = canSplit;
            EnhancedCollisionCount = enhancedCollisionCount;
            EnhancedCollisionMultiplier = enhancedCollisionMultiplier;
            BaseCollisionMultiplier = baseCollisionMultiplier;
            ChainMultiplierPerHit = chainMultiplierPerHit;
            GravityScaleMultiplier = gravityScaleMultiplier;
            Bounciness = bounciness;
            PierceNormalBumperCount = pierceNormalBumperCount;
            ExtraStarRewards = extraStarRewards;
            SplitChildCollisionMultiplier = splitChildCollisionMultiplier;
        }

        public float GetCollisionMultiplier(int hitIndex, float childMultiplier)
        {
            var multiplier = BaseCollisionMultiplier * childMultiplier;
            if (hitIndex <= EnhancedCollisionCount) multiplier *= EnhancedCollisionMultiplier;
            return multiplier + Mathf.Max(0, hitIndex - 1) * ChainMultiplierPerHit * childMultiplier;
        }
    }

    public static class MarbleCatalog
    {
        public const string Normal = "normal";
        public const string Split = "split";
        public const string Gravity = "gravity";
        public const string Pierce = "pierce";
        public const string Bounce = "bounce";
        public const string Lucky = "lucky";

        private static readonly List<MarbleDefinition> definitions = new List<MarbleDefinition>
        {
            new MarbleDefinition(Normal, "普通弹珠", "前 6 次有效碰撞基础攻击收益 +20%，稳定通用。", GamePalette.Blue, enhancedCollisionCount: 6, enhancedCollisionMultiplier: 1.2f),
            new MarbleDefinition(Split, "分裂弹珠", "离开发射轨道进入中央区时立即分裂为 2 颗子珠；子珠碰撞收益为 60%。", GamePalette.Pink, canSplit: true, splitChildCollisionMultiplier: .6f),
            new MarbleDefinition(Gravity, "重力弹珠", "重力更强、弹跳较低；前 4 次有效碰撞获得额外攻击加成。", GamePalette.Orange, enhancedCollisionCount: 4, enhancedCollisionMultiplier: 1.35f, gravityScaleMultiplier: 1.55f, bounciness: .50f),
            new MarbleDefinition(Pierce, "穿透弹珠", "可穿透前 2 个普通碰撞柱，穿透依然计分且不反弹。", GamePalette.Cyan, pierceNormalBumperCount: 2),
            new MarbleDefinition(Bounce, "弹跳弹珠", "弹性提高，连续碰撞越多收益越高；基础碰撞攻击 -20%。", GamePalette.Purple, baseCollisionMultiplier: .8f, chainMultiplierPerHit: .12f, bounciness: .82f),
            new MarbleDefinition(Lucky, "幸运弹珠", "普通碰撞攻击 -15%；进入星星出口时额外获得 1 次奖励。", GamePalette.Yellow, baseCollisionMultiplier: .85f, extraStarRewards: 1)
        };

        public static IReadOnlyList<MarbleDefinition> All => definitions;

        public static MarbleDefinition Get(string id)
        {
            foreach (var definition in definitions)
                if (definition.Id == id) return definition;
            return definitions[0];
        }
    }
}
