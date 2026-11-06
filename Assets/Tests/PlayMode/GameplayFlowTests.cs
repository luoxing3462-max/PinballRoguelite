using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace PinballRoguelite.Tests
{
    public sealed class GameplayFlowTests
    {
        [UnityTest]
        public IEnumerator MarbleSelectionScreenRequiresTwoDistinctChoices()
        {
            var canvasObject = new GameObject("MarbleSelectionCanvas", typeof(RectTransform), typeof(Canvas));
            var screenObject = new GameObject("MarbleSelectionScreen", typeof(RectTransform));
            screenObject.transform.SetParent(canvasObject.transform, false);
            var screen = screenObject.AddComponent<MarbleSelectionScreen>();
            IReadOnlyList<string> confirmedSelection = null;
            screen.SelectionConfirmed += selection => confirmedSelection = selection;
            screen.Setup();

            var confirm = screen.transform.Find("ConfirmButton").GetComponent<Button>();
            Assert.That(confirm.interactable, Is.False);
            screen.transform.Find("Marble_gravity").GetComponent<Button>().onClick.Invoke();
            screen.transform.Find("Marble_lucky").GetComponent<Button>().onClick.Invoke();
            Assert.That(confirm.interactable, Is.True);
            confirm.onClick.Invoke();
            Assert.That(confirmedSelection, Is.EqualTo(new[] { MarbleCatalog.Gravity, MarbleCatalog.Lucky }));

            Object.Destroy(canvasObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChargedImpulseProducesDistinctPhysicalLaunchSpeeds()
        {
            var settings = new LaunchPhysicsSettings();
            var lowImpulse = settings.EvaluateImpulse(0f);
            var highImpulse = settings.EvaluateImpulse(100f);
            Assert.That(highImpulse, Is.GreaterThan(lowImpulse * 1.5f));

            var lowObject = new GameObject("LowChargeMarble");
            var lowMarble = lowObject.AddComponent<Marble>();
            lowObject.transform.position = Vector2.up * 20f;
            lowMarble.Configure("normal", Vector2.up * lowImpulse, false);
            lowMarble.Body.gravityScale = 0f;

            var highObject = new GameObject("HighChargeMarble");
            var highMarble = highObject.AddComponent<Marble>();
            highObject.transform.position = Vector2.right * 2f + Vector2.up * 20f;
            highMarble.Configure("normal", Vector2.up * highImpulse, false);
            highMarble.Body.gravityScale = 0f;

            yield return new WaitForFixedUpdate();

            Assert.That(highMarble.Body.linearVelocity.y, Is.GreaterThan(lowMarble.Body.linearVelocity.y * 1.5f));
            Object.Destroy(lowObject);
            Object.Destroy(highObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllMarbleTypesUseContinuousBoundedPhysics()
        {
            foreach (var definition in MarbleCatalog.All)
            {
                var marbleObject = new GameObject("PhysicsProbe_" + definition.Id);
                marbleObject.transform.position = Vector2.up * 20f;
                var marble = marbleObject.AddComponent<Marble>();
                marble.Configure(definition.Id, Vector2.up * 100f, false);
                yield return new WaitForFixedUpdate();

                Assert.That(marble.Body.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode2D.Continuous));
                Assert.That(marble.Body.linearVelocity.magnitude, Is.LessThanOrEqualTo(26f));
                Assert.That(marble.GetComponent<CircleCollider2D>().sharedMaterial.bounciness, Is.LessThanOrEqualTo(0.82f));
                Object.Destroy(marbleObject);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator SplitMarbleOnlyRequestsItsSingleExitSplit()
        {
            var marbleObject = new GameObject("SplitExitProbe");
            marbleObject.transform.position = Vector2.up * 20f;
            var marble = marbleObject.AddComponent<Marble>();
            marble.Configure(MarbleCatalog.Split, Vector2.zero, true);
            var splitRequests = 0;
            marble.SplitRequested += _ => splitRequests++;

            Assert.That(marble.RequestSplit(), Is.True);
            Assert.That(marble.RequestSplit(), Is.False);
            Assert.That(splitRequests, Is.EqualTo(1));

            Object.Destroy(marbleObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LauncherGateAdmitsLauncherTravelAndBlocksBoardReturn()
        {
            var board = new PinballBoard();
            board.Build(LevelConfig.For(MapNodeType.Normal), 9017);
            var bounds = board.PlayfieldBounds;
            var gateY = Mathf.Lerp(bounds.min.y, bounds.max.y, 0.94f);
            var inbound = board.SpawnMarble(MarbleCatalog.Normal, new Vector2(bounds.max.x + 0.45f, gateY), Vector2.left * 12f, false);
            inbound.Body.gravityScale = 0f;
            for (var step = 0; step < 16; step++) yield return new WaitForFixedUpdate();
            Assert.That(inbound.transform.position.x, Is.LessThan(bounds.max.x - 0.10f), "The launcher must be able to enter the playfield.");

            var outbound = board.SpawnMarble(MarbleCatalog.Normal, new Vector2(bounds.max.x - 0.65f, gateY), Vector2.right * 12f, false);
            outbound.Body.gravityScale = 0f;
            for (var step = 0; step < 16; step++) yield return new WaitForFixedUpdate();
            Assert.That(outbound.transform.position.x, Is.LessThanOrEqualTo(bounds.max.x + 0.20f), "A playfield marble must not return into the launcher lane.");

            board.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator BounceMarbleStaysInsideClosedPlayfieldAtMaximumLaunchImpulse()
        {
            var board = new PinballBoard();
            board.Build(LevelConfig.For(MapNodeType.Normal), 9018);
            var settings = new LaunchPhysicsSettings();
            var marble = board.SpawnMarble(MarbleCatalog.Bounce, new Vector2(0f, 2.2f),
                new Vector2(16f, settings.EvaluateImpulse(100f)), false);
            var bounds = board.PlayfieldBounds;
            var escaped = false;
            var settled = false;
            marble.Settled += (_, _) => settled = true;
            var timeout = 16f;
            while (marble != null && marble.IsActive && timeout > 0f)
            {
                var position = marble.transform.position;
                escaped |= position.x < bounds.min.x - 0.22f || position.x > bounds.max.x + 0.22f || position.y > bounds.max.y + 0.22f;
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.That(escaped, Is.False, "The high-bounce marble escaped through a top or side boundary.");
            Assert.That(settled, Is.True, "The high-bounce marble did not reach a settlement path before its safety timeout.");
            board.Dispose();
        }

        [UnityTest]
        public IEnumerator RandomizedBumperLayoutsAreSeededAndSafe()
        {
            var config = LevelConfig.For(MapNodeType.Normal);
            var first = new PinballBoard();
            first.Build(config, 1104);
            var firstPositions = new List<Vector2>(first.BumperPositions);

            Assert.That(first.LayoutSeed, Is.EqualTo(1104));
            Assert.That(firstPositions.Count, Is.EqualTo(BumperLayoutSettings.DefaultNormalBumperCount + BumperLayoutSettings.DefaultSpecialBumperCount));
            Assert.That(first.Root.GetComponentsInChildren<Bumper2D>().Length, Is.EqualTo(16));
            Assert.That(first.Root.GetComponentsInChildren<Bumper2D>().Count(bumper => bumper.SplitTrigger), Is.EqualTo(4));
            Assert.That(first.LayoutTemplateIndex, Is.InRange(0, 3));
            Assert.That(first.LayoutRowCount, Is.EqualTo(4));
            Assert.That(first.LayoutColumnCount, Is.EqualTo(4));
            var scoringZones = first.Root.GetComponentsInChildren<ScoringZone>();
            Assert.That(scoringZones.Length, Is.EqualTo(8));
            Assert.That(scoringZones.Count(zone => zone.IsStarExit), Is.EqualTo(1));
            Assert.That(scoringZones.Count(zone => !zone.IsStarExit && zone.Multiplier == 1f), Is.GreaterThanOrEqualTo(1));
            Assert.That(scoringZones.Where(zone => !zone.IsStarExit).All(zone => zone.Multiplier == 0f || zone.Multiplier == 1f || zone.Multiplier == 1.25f || zone.Multiplier == 1.5f || zone.Multiplier == 2f), Is.True);
            Assert.That(first.Root.Cast<Transform>().Count(child => child.name == "BinDivider"), Is.EqualTo(7));
            var multiplierLabels = first.Root.GetComponentsInChildren<TextMesh>();
            Assert.That(multiplierLabels.Count(label => label.text == "★"), Is.EqualTo(1));
            Assert.That(multiplierLabels.Count(label => label.text.StartsWith("×")), Is.EqualTo(7));
            Assert.That(multiplierLabels.All(label => label.GetComponent<MeshRenderer>().sharedMaterial != null), Is.True);
            for (var left = 0; left < firstPositions.Count; left++)
            for (var right = left + 1; right < firstPositions.Count; right++)
                Assert.That(Vector2.Distance(firstPositions[left], firstPositions[right]), Is.GreaterThanOrEqualTo(first.LayoutSettings.MinimumSpacing));
            var rows = firstPositions.GroupBy(position => position.y).OrderByDescending(group => group.Key)
                .Select(group => group.OrderBy(position => position.x).ToList()).ToList();
            Assert.That(rows.Count, Is.EqualTo(4));
            Assert.That(rows.All(row => row.Count == 4), Is.True);
            var verticalSpacing = rows[0][0].y - rows[1][0].y;
            Assert.That(verticalSpacing, Is.GreaterThanOrEqualTo(first.LayoutSettings.MinimumSpacing));
            for (var row = 0; row < rows.Count; row++)
            {
                var horizontalSpacing = rows[row][1].x - rows[row][0].x;
                Assert.That(horizontalSpacing, Is.GreaterThanOrEqualTo(first.LayoutSettings.MinimumSpacing));
                for (var column = 1; column < rows[row].Count - 1; column++)
                    Assert.That(rows[row][column + 1].x - rows[row][column].x, Is.EqualTo(horizontalSpacing).Within(0.001f));
                if (row > 0)
                    Assert.That(Mathf.Abs(rows[row][0].x - rows[row - 1][0].x),
                        Is.EqualTo(horizontalSpacing * 0.5f).Within(0.001f));
            }
            foreach (var position in firstPositions)
            {
                Assert.That(PinballBoard.IsBumperLayoutPositionAllowed(position), Is.True);
                var opposite = new Vector2(PinballBoard.BumperSymmetryAxisX * 2f - position.x,
                    PinballBoard.BumperLayoutTop + PinballBoard.BumperLayoutBottom - position.y);
                Assert.That(firstPositions.Any(candidate => Vector2.Distance(candidate, opposite) <= 0.001f), Is.True);
            }
            Assert.That(firstPositions.Min(position => position.x), Is.LessThanOrEqualTo(PinballBoard.BumperLayoutLeft + 0.12f));
            Assert.That(firstPositions.Max(position => position.x), Is.GreaterThanOrEqualTo(PinballBoard.BumperLayoutRight - 0.12f));
            Assert.That(firstPositions.Min(position => position.y), Is.LessThanOrEqualTo(PinballBoard.BumperLayoutBottom + Bumper2D.Radius * .8f));
            Assert.That(firstPositions.Max(position => position.y), Is.GreaterThanOrEqualTo(PinballBoard.BumperLayoutTop - Bumper2D.Radius * .8f));

            var firstSpecialPositions = first.Root.GetComponentsInChildren<Bumper2D>().Where(bumper => bumper.SplitTrigger)
                .Select(bumper => (Vector2)bumper.transform.position).ToList();
            first.Dispose();
            yield return null;

            var second = new PinballBoard();
            second.Build(config, 2208);
            var secondSpecialPositions = second.Root.GetComponentsInChildren<Bumper2D>().Where(bumper => bumper.SplitTrigger)
                .Select(bumper => (Vector2)bumper.transform.position).ToList();
            var specialLayoutChanged = first.LayoutTemplateIndex != second.LayoutTemplateIndex ||
                firstSpecialPositions.Any(position => !secondSpecialPositions.Any(candidate => Vector2.Distance(candidate, position) <= 0.001f));
            Assert.That(specialLayoutChanged, Is.True, "Different round seeds must vary the selected stagger template or special grid positions.");

            var selectedTemplates = new HashSet<int>();
            for (var seed = 1; seed <= 24; seed++)
            {
                var sample = new PinballBoard();
                sample.Build(config, seed);
                selectedTemplates.Add(sample.LayoutTemplateIndex);
                Assert.That(sample.BumperPositions.Count(position => position.x <= PinballBoard.BumperLayoutLeft + 0.45f), Is.GreaterThanOrEqualTo(2));
                Assert.That(sample.BumperPositions.Count(position => position.x >= PinballBoard.BumperLayoutRight - 0.45f), Is.GreaterThanOrEqualTo(2));
                sample.Dispose();
            }
            Assert.That(selectedTemplates.Count, Is.EqualTo(4), "Round seeds must select from all four symmetric templates.");

            foreach (var totalCount in new[] { 14, 16, 18, 20 })
            {
                var settings = new BumperLayoutSettings { NormalBumperCount = totalCount - 4, SpecialBumperCount = 4 };
                var board = new PinballBoard(settings);
                board.Build(config, 8000 + totalCount);
                Assert.That(board.BumperPositions.Count, Is.EqualTo(totalCount));
                Assert.That(board.Root.GetComponentsInChildren<Bumper2D>().Count(bumper => bumper.SplitTrigger), Is.EqualTo(4));
                Assert.That(board.BumperPositions.Min(position => position.x), Is.LessThanOrEqualTo(PinballBoard.BumperLayoutLeft + 0.12f));
                Assert.That(board.BumperPositions.Max(position => position.x), Is.GreaterThanOrEqualTo(PinballBoard.BumperLayoutRight - 0.12f));
                board.Dispose();
            }

            var customSettings = new BumperLayoutSettings { NormalBumperCount = 8, SpecialBumperCount = 2 };
            var customBoard = new PinballBoard(customSettings);
            customBoard.Build(config, 7719);
            Assert.That(customBoard.Root.GetComponentsInChildren<Bumper2D>().Length, Is.EqualTo(10));
            Assert.That(customBoard.Root.GetComponentsInChildren<Bumper2D>().Count(bumper => bumper.SplitTrigger), Is.EqualTo(2));
            customBoard.Dispose();

            var zeroProbe = new GameObject("ZeroMultiplierProbe").AddComponent<Marble>();
            var resolvedMultiplier = -1f;
            zeroProbe.Settled += (_, multiplier) => resolvedMultiplier = multiplier;
            zeroProbe.Configure("normal", Vector2.zero, false);
            zeroProbe.Settle(0);
            Assert.That(resolvedMultiplier, Is.Zero, "A x0 slot must settle as zero instead of being clamped to x1.");

            second.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator MarbleRaisesOneDamageEventPerBumper()
        {
            var firstBumperPosition = new Vector2(0f, 20f);
            var secondBumperPosition = firstBumperPosition + Vector2.up;
            var bumperObject = new GameObject("DamageGateBumper");
            bumperObject.transform.position = firstBumperPosition;
            var bumper = bumperObject.AddComponent<Bumper2D>();
            bumper.Configure(10, false);
            var secondBumperObject = new GameObject("SecondDamageGateBumper");
            secondBumperObject.transform.position = secondBumperPosition;
            var secondBumper = secondBumperObject.AddComponent<Bumper2D>();
            secondBumper.Configure(10, false);

            var marbleObject = new GameObject("DamageGateMarble");
            marbleObject.transform.position = firstBumperPosition + Vector2.left * 2f;
            var marble = marbleObject.AddComponent<Marble>();
            marble.Configure("normal", Vector2.right * 12f, false);
            marble.Body.gravityScale = 0f;
            var damageEvents = 0;
            marble.BumperHit += (hitMarble, points) => damageEvents++;

            for (var step = 0; step < 25; step++) yield return new WaitForFixedUpdate();
            Assert.That(damageEvents, Is.EqualTo(1));

            marble.Body.position = firstBumperPosition + Vector2.left * 2f;
            marble.Body.linearVelocity = Vector2.right * 12f;
            for (var step = 0; step < 25; step++) yield return new WaitForFixedUpdate();
            Assert.That(damageEvents, Is.EqualTo(1));

            marble.Body.position = secondBumperPosition + Vector2.left * 2f;
            marble.Body.linearVelocity = Vector2.right * 12f;
            for (var step = 0; step < 25; step++) yield return new WaitForFixedUpdate();
            Assert.That(damageEvents, Is.EqualTo(2));

            Object.Destroy(marbleObject);
            Object.Destroy(bumperObject);
            Object.Destroy(secondBumperObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainSceneBuildsUiAndPhysicalShotCompletes()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            var root = Object.FindFirstObjectByType<GameRoot>();
            Assert.That(root, Is.Not.Null);
            Assert.That(root.CurrentScreen, Is.TypeOf<TitleScreen>());
            GameObject uiRoot = GameObject.Find("UI");
            Assert.That(uiRoot, Is.Not.Null);
            Assert.That(uiRoot.GetComponentInChildren<Canvas>(), Is.Not.Null);
            Assert.That(uiRoot.GetComponentInChildren<EventSystem>(), Is.Not.Null);

            root.RunState.StartNewRun(20261104);
            root.ShowMap();
            yield return null;
            Assert.That(root.CurrentScreen, Is.TypeOf<MapScreen>());
            Assert.That(root.CurrentScreen.GetComponentsInChildren<LevelUiView>(true).Length, Is.EqualTo(1));
            Assert.That(root.CurrentScreen.transform.Find("LevelUI/SafeArea/MapPanel/MapArea"), Is.Not.Null);

            var first = root.RunState.Map.Layers[0][0];
            Assert.That(root.RunState.ChooseNode(first), Is.True);
            root.ShowGameplay(first);
            yield return null;

            var gameplay = root.CurrentScreen as GameplayScreen;
            Assert.That(gameplay, Is.Not.Null);
            var gameplayUi = gameplay.GetComponentInChildren<GameplayUiView>(true);
            Assert.That(gameplayUi, Is.Not.Null);
            Assert.That(gameplay.GetComponentsInChildren<GameplayUiView>(true).Length, Is.EqualTo(1));
            Assert.That(gameplayUi.transform.Find("SafeArea/TopHUD/BattleInfo/PlayerAvatarPlaceholder"), Is.Not.Null);
            Assert.That(gameplayUi.transform.Find("SafeArea/TopHUD/BattleInfo/EnemyAvatarPlaceholder"), Is.Not.Null);
            Assert.That(gameplayUi.transform.Find("SafeArea/BottomHUD/MarbleSelectionPanel/NormalMarbleButton"), Is.Not.Null);
            Assert.That(gameplayUi.transform.Find("SafeArea/BottomHUD/LaunchPanel/LaunchButton"), Is.Not.Null);
            Assert.That(gameplayUi.transform.Find("SafeArea/ResultOverlay/VictoryPanel"), Is.Not.Null);
            Assert.That(gameplayUi.transform.Find("SafeArea/ResultOverlay/DefeatPanel"), Is.Not.Null);
            var resultOverlay = gameplayUi.transform.Find("SafeArea/ResultOverlay").gameObject;
            var victoryPanel = resultOverlay.transform.Find("VictoryPanel").gameObject;
            var defeatPanel = resultOverlay.transform.Find("DefeatPanel").gameObject;
            var victoryClicks = 0;
            gameplayUi.ShowResult(true, false, 140, 140, () => victoryClicks++, null, null);
            Assert.That(resultOverlay.activeSelf, Is.True);
            Assert.That(victoryPanel.activeSelf, Is.True);
            Assert.That(defeatPanel.activeSelf, Is.False);
            victoryPanel.GetComponentInChildren<Button>().onClick.Invoke();
            Assert.That(victoryClicks, Is.EqualTo(1));
            gameplayUi.ShowResult(false, false, 0, 140, null, () => { }, () => { });
            Assert.That(victoryPanel.activeSelf, Is.False);
            Assert.That(defeatPanel.activeSelf, Is.True);
            resultOverlay.SetActive(false);
            var expectedTargetScore = CombatBalanceConfig.CreateLevel(first.Type, first.Layer, root.RunState.Map.Layers.Count).TargetScore;
            Assert.That(gameplay.Config.TargetScore, Is.EqualTo(expectedTargetScore));
            Assert.That(gameplay.Combat, Is.Not.Null);
            Assert.That(gameplay.Combat.MaxHealth, Is.EqualTo(gameplay.Config.TargetScore));
            Assert.That(gameplay.Combat.CurrentHealth, Is.EqualTo(gameplay.Combat.MaxHealth));
            Assert.That(gameplay.Combat.PlayerCurrentHealth, Is.EqualTo(CombatDisplay.DefaultPlayerMaxHealth));
            Assert.That(gameplay.CurrentTurn, Is.EqualTo(GameplayScreen.CombatTurn.PlayerInput));
            Assert.That(gameplay.Combat.PlayerPositionX, Is.LessThan(gameplay.Combat.EnemyPositionX));
            Assert.That(gameplay.Combat.StepMarkerCount, Is.EqualTo(gameplay.Config.ShotLimit + 1));
            var initialEnemyX = gameplay.Combat.EnemyPositionX;
            Assert.That(gameplay.Board.Root.GetComponentsInChildren<Bumper2D>().Length, Is.EqualTo(16));
            Assert.That(PinballBoard.LaunchOrigin.x, Is.GreaterThan(4f));
            Assert.That(PinballBoard.LaunchOrigin.y, Is.LessThan(-3f));
            Assert.That(gameplay.Board.Root.Find("TopInnerGuide"), Is.Null,
                "The highlighted horizontal color block must not be created.");
            Assert.That(gameplay.Board.Root.Find("InnerArc0"), Is.Null,
                "The highlighted inner arc color block must not be created.");

            var fire = typeof(GameplayScreen).GetMethod("Fire", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(fire, Is.Not.Null);
            fire.Invoke(gameplay, new object[] { 55f });
            Assert.That(gameplay.ShotsUsed, Is.EqualTo(1));
            Assert.That(gameplay.ActiveMarbleCount, Is.EqualTo(1));

            var timeout = 18f;
            var maxY = float.NegativeInfinity;
            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var topTrace = new List<string>();
            while (gameplay.ActiveMarbleCount > 0 && timeout > 0f)
            {
                foreach (var marble in gameplay.Board.ActiveMarbles)
                {
                    if (marble == null) continue;
                    var position = marble.transform.position;
                    maxY = Mathf.Max(maxY, position.y);
                    minX = Mathf.Min(minX, position.x);
                    maxX = Mathf.Max(maxX, position.x);
                    if (position.y > 3.7f && topTrace.Count < 40)
                        topTrace.Add($"({position.x:F2},{position.y:F2}) v=({marble.Body.linearVelocity.x:F2},{marble.Body.linearVelocity.y:F2})");
                }
                timeout -= Time.deltaTime;
                yield return null;
            }

            Debug.Log($"PHYSICS TRACE: maxY={maxY:F2}, minX={minX:F2}, maxX={maxX:F2}, " +
                      $"shots={gameplay.ShotsUsed}, score={gameplay.Score.TotalScore}\n" + string.Join(" | ", topTrace));

            Assert.That(timeout, Is.GreaterThan(0f), "The physical marble did not settle before the safety timeout.");
            Assert.That(gameplay.ActiveMarbleCount, Is.Zero);
            yield return new WaitForSeconds(1.6f);
            Assert.That(gameplay.Score.TotalScore, Is.GreaterThanOrEqualTo(0),
                "A physical shot may validly score zero when it lands in the x0 slot.");
            Assert.That(gameplay.Combat.LastDamage, Is.EqualTo(gameplay.Score.TotalScore));
            Assert.That(gameplay.Combat.CurrentHealth,
                Is.EqualTo(Mathf.Max(0, gameplay.Config.TargetScore - gameplay.Score.TotalScore)));
            if (gameplay.Combat.CurrentHealth > 0)
            {
                Assert.That(gameplay.Combat.EnemyProgress,
                    Is.EqualTo((float)gameplay.ShotsUsed / gameplay.Config.ShotLimit).Within(0.001f));
                Assert.That(gameplay.Combat.EnemyPositionX, Is.LessThan(initialEnemyX),
                    "A surviving enemy should advance one shot step toward the fixed player.");
            }
        }
    }
}
