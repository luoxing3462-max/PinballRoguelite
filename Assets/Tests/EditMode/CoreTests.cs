using NUnit.Framework;
using UnityEngine;

namespace PinballRoguelite.Tests
{
    public sealed class CoreTests
    {
        [Test]
        public void GeneratedMapHasReachableSingleBoss()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var map = TowerMapGenerator.Generate(seed);
                Assert.That(TowerMapGenerator.ValidateRoutes(map), Is.True);
                Assert.That(map.Layers.Count, Is.InRange(6, 8));
                Assert.That(map.Layers[0].Count, Is.EqualTo(1));
                Assert.That(map.Layers[0][0].Type, Is.EqualTo(MapNodeType.Normal));
                Assert.That(map.Layers[^1].Count, Is.EqualTo(1));
                Assert.That(map.Layers[^1][0].Type, Is.EqualTo(MapNodeType.Boss));
                var hasElite = false;
                var hasEvent = false;
                var hasRest = false;
                foreach (var layer in map.Layers) Assert.That(layer.Count, Is.LessThanOrEqualTo(4));
                foreach (var layer in map.Layers)
                foreach (var node in layer)
                {
                    hasElite |= node.Type == MapNodeType.Elite;
                    hasEvent |= node.Type == MapNodeType.Event;
                    hasRest |= node.Type == MapNodeType.Rest;
                }
                Assert.That(hasElite, Is.True);
                Assert.That(hasEvent, Is.True);
                Assert.That(hasRest, Is.True);
                for (var index = 1; index < map.Layers.Count; index++)
                foreach (var node in map.Layers[index])
                    Assert.That(map.Layers[index - 1].Exists(parent => parent.Connections.Contains(node.Id)), Is.True);
            }
        }

        [Test]
        public void SplitMarblesShareOneLaunchDamageBudget()
        {
            var score = new ScoreManager();
            score.Reset();
            score.BeginLaunch(8, 3f);
            score.RegisterMarble(1);
            score.AddBumperScore(1, 12);
            score.DetachMarble(1);
            score.RegisterMarble(2);
            score.RegisterMarble(3);
            score.AddBumperScore(2, 12);
            score.AddBumperScore(3, 12);
            score.SettleMarble(2, 1f);
            score.SettleMarble(3, 2f);
            Assert.That(score.FinishLaunch(), Is.EqualTo(34));
        }

        [Test]
        public void EachEffectiveBumperCollisionAddsDamage()
        {
            var score = new ScoreManager();
            score.BeginLaunch(8, 3f);
            score.RegisterMarble(1);
            score.RegisterMarble(2);

            score.AddBumperScore(1, 10);
            score.AddBumperScore(1, 30);
            score.AddBumperScore(2, 20);

            score.SettleMarble(1, 1f);
            score.SettleMarble(2, 1f);
            Assert.That(score.FinishLaunch(), Is.EqualTo(17));
        }

        [Test]
        public void MarbleBuildCatalogProvidesSixBalancedVariants()
        {
            Assert.That(MarbleCatalog.All.Count, Is.EqualTo(6));
            Assert.That(MarbleCatalog.Get(MarbleCatalog.Normal).EnhancedCollisionCount, Is.EqualTo(6));
            Assert.That(MarbleCatalog.Get(MarbleCatalog.Split).CanSplit, Is.True);
            Assert.That(MarbleCatalog.Get(MarbleCatalog.Gravity).GravityScaleMultiplier, Is.GreaterThan(1f));
            Assert.That(MarbleCatalog.Get(MarbleCatalog.Pierce).PierceNormalBumperCount, Is.EqualTo(2));
            Assert.That(MarbleCatalog.Get(MarbleCatalog.Bounce).BaseCollisionMultiplier, Is.LessThan(1f));
            Assert.That(MarbleCatalog.Get(MarbleCatalog.Lucky).ExtraStarRewards, Is.EqualTo(1));

            var score = new ScoreManager();
            score.BeginLaunch(8, 3f);
            score.RegisterMarble(1);
            for (var index = 0; index < 10; index++) score.AddBumperScore(1, 1.2f);
            score.SettleMarble(1, 1f);
            Assert.That(score.EffectiveCollisionCount, Is.EqualTo(CombatBalanceConfig.MaximumEffectiveCollisions));
            Assert.That(score.FinishLaunch(), Is.EqualTo(37));
        }

        [Test]
        public void RunStateKeepsTwoChosenMarblesUntilANewRun()
        {
            var runState = new RunState();
            runState.StartNewRun(2204);
            Assert.That(runState.SetEquippedMarbles(new[] { MarbleCatalog.Gravity, MarbleCatalog.Lucky }), Is.True);
            Assert.That(runState.EquippedMarbleIds, Is.EqualTo(new[] { MarbleCatalog.Gravity, MarbleCatalog.Lucky }));
            runState.StartNewRun(2205);
            Assert.That(runState.EquippedMarbleIds, Is.EqualTo(new[] { MarbleCatalog.Normal, MarbleCatalog.Split }));
        }

        [Test]
        public void RunEffectsApplyOncePerAttackAndResetForNewRun()
        {
            var effects = new RunEffects();
            effects.Apply(RunEffectId.AttackUp);
            effects.Apply(RunEffectId.StarNextAttack);
            Assert.That(effects.ResolvePlayerDamage(100), Is.EqualTo(130));
            Assert.That(effects.ResolvePlayerDamage(100), Is.EqualTo(110));
            effects.Apply(RunEffectId.CurseOverload);
            Assert.That(effects.MaximumHealth, Is.EqualTo(85));
            effects.Reset();
            Assert.That(effects.CurrentHealth, Is.EqualTo(100));
            Assert.That(effects.PermanentDamageMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void CombatBalanceUsesSharedCollisionCapAndLayeredHealth()
        {
            var score = new ScoreManager();
            score.BeginLaunch(CombatBalanceConfig.PlayerBaseAttack, CombatBalanceConfig.BaseCollisionDamage);
            for (var index = 0; index < 10; index++)
            {
                score.RegisterMarble(index);
                score.AddBumperScore(index, 3);
            }
            score.SettleMarble(0, 1.5f);
            Assert.That(score.EffectiveCollisionCount, Is.EqualTo(8));
            Assert.That(score.FinishLaunch(), Is.EqualTo(48));
            Assert.That(CombatBalanceConfig.CreateLevel(MapNodeType.Normal, 0, 8).TargetScore, Is.EqualTo(100));
            Assert.That(CombatBalanceConfig.CreateLevel(MapNodeType.Elite, 2, 8).TargetScore, Is.EqualTo(190));
            Assert.That(CombatBalanceConfig.CreateLevel(MapNodeType.Boss, 7, 8).TargetScore, Is.EqualTo(400));
        }

        [TestCase(750, 1334)]
        [TestCase(780, 1688)]
        [TestCase(1080, 2400)]
        [TestCase(1440, 3200)]
        public void PortraitCameraFitsTheFullBoardAtSupportedResolutions(int width, int height)
        {
            var size = PortraitBoardCameraFitter.CalculateOrthographicSize(width, height);
            var visibleHalfWidth = size * width / height;

            Assert.That(size, Is.GreaterThanOrEqualTo(PortraitBoardCameraFitter.MinimumOrthographicSize));
            Assert.That(visibleHalfWidth, Is.GreaterThanOrEqualTo(PortraitBoardCameraFitter.BoardHalfWidth));
        }

        [Test]
        public void SafeAreaAnchorsKeepNotchAndGestureInsetsInsideTheCanvas()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(0f, 72f, 780f, 1516f), new Vector2(780f, 1688f),
                out var anchorMin, out var anchorMax);

            Assert.That(anchorMin, Is.EqualTo(new Vector2(0f, 72f / 1688f)));
            Assert.That(anchorMax, Is.EqualTo(new Vector2(1f, 1588f / 1688f)));
        }
    }
}
