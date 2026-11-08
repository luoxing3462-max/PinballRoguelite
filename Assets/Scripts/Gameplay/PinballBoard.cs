using System.Collections.Generic;
using UnityEngine;

namespace PinballRoguelite
{
    public sealed class PinballBoard
    {
        public static readonly Vector2 LaunchOrigin = new Vector2(4.27f, -3.75f);
        public const float LaunchExitX = 3.45f;
        public const float LaunchExitY = 3.60f;
        public const float LeftWallCenterX = -4.82f;
        public const float LeftWallWidth = 0.14f;
        public const float ChannelInnerWallX = 3.99f;
        public const float PlayfieldRightEdgeX = 3.65f;
        public const float BumperSideMargin = Bumper2D.Radius * 2.7f;
        public const float BumperLayoutLeft = LeftWallCenterX + LeftWallWidth * 0.5f + BumperSideMargin;
        public const float BumperLayoutRight = PlayfieldRightEdgeX - BumperSideMargin;
        public const float BumperSymmetryAxisX = (BumperLayoutLeft + BumperLayoutRight) * 0.5f;
        public const float BumperLayoutTop = 2.70f;
        public const float BumperLayoutBottom = -2.40f;
        private const float BoundaryThickness = 0.12f;
        // Matches the rounded inner lip of the 670 x 660 BoardBackground artwork.
        // This is physics-only; BoardFrame remains the sole renderer for the rim.
        private const float BoardCornerRadius = 0.42f;
        private const float BottomExitHeightRatio = 0.125f;
        private const float LauncherEntranceLowerRatio = 0.80f;
        private const float LauncherEntranceUpperRatio = 0.975f;
        private const float FigmaBoardBackgroundAspect = 670f / 660f;
        private static readonly Vector2 playfieldCenter = new Vector2(-0.55f, 0.08f);
        private static readonly Vector2 playfieldSize = new Vector2(8.55f, 10.6f);
        public Transform Root { get; private set; }
        public Bounds PlayfieldBounds { get; private set; }
        public int LayoutSeed { get; private set; }
        public int LayoutTemplateIndex { get; private set; }
        public string LayoutTemplateName { get; private set; }
        public int LayoutRowCount { get; private set; }
        public int LayoutColumnCount { get; private set; }
        public IReadOnlyList<Vector2> BumperPositions => bumperPositions;
        public BumperLayoutSettings LayoutSettings => layoutSettings;

        private static readonly BumperLayoutTemplate[] bumperLayoutTemplates =
        {
            new BumperLayoutTemplate("ClassicStaggerLeft", -1, 0),
            new BumperLayoutTemplate("ClassicStaggerRight", 1, 3),
            new BumperLayoutTemplate("ClassicStaggerLeftCross", -1, 6),
            new BumperLayoutTemplate("ClassicStaggerRightCross", 1, 9)
        };

        private readonly BumperLayoutSettings layoutSettings;
        private GameObject marblePrefab;
        private GameObject bumperPrefab;
        private GameObject scoringZonePrefab;
        private readonly BattleArtCatalog battleArt;
        private SpriteRenderer loadedMarbleVisual;
        private readonly List<Marble> activeMarbles = new List<Marble>();
        private readonly List<Vector2> bumperPositions = new List<Vector2>();
        public ScoringZone StarZone { get; private set; }

        public PinballBoard(BumperLayoutSettings bumperLayoutSettings = null, BattleArtCatalog visualArt = null)
        {
            layoutSettings = bumperLayoutSettings ?? new BumperLayoutSettings();
            battleArt = visualArt ?? Resources.Load<BattleArtCatalog>("UI/Battle/BattleArtCatalog");
        }

        public void Build(LevelConfig config, int? layoutSeed = null)
        {
            Build(config, null, layoutSeed);
        }

        public void Build(LevelConfig config, RunEffects effects, int? layoutSeed = null, Transform physicsParent = null)
        {
            LayoutSeed = layoutSeed ?? System.Guid.NewGuid().GetHashCode();
            bumperPositions.Clear();
            var rootObject = new GameObject("PhysicsWorld");
            Root = rootObject.transform;
            if (physicsParent != null)
            {
                physicsParent.GetComponent<PlayfieldPhysicsHost>()?.PrepareForWorldPhysics();
                Root.SetParent(physicsParent, true);
            }
            marblePrefab = Resources.Load<GameObject>("Prefabs/Marble");
            bumperPrefab = Resources.Load<GameObject>("Prefabs/Bumper");
            scoringZonePrefab = Resources.Load<GameObject>("Prefabs/ScoringZone");

            PlayfieldBounds = new Bounds(playfieldCenter, playfieldSize);
            var boardBackgroundSize = new Vector2(PlayfieldBounds.size.y * FigmaBoardBackgroundAspect, PlayfieldBounds.size.y);
            CreateDecoration("BoardBackground", PlayfieldBounds.center, boardBackgroundSize, GamePalette.Panel, -20,
                battleArt != null ? battleArt.BoardTexture : null);

            var guideMaterial = new PhysicsMaterial2D("GuideMaterial") { bounciness = 0.10f, friction = 0.01f };
            CreatePlayfieldBoundaries(PlayfieldBounds, guideMaterial);
            CreateWall("ChannelInnerWall", new Vector2(ChannelInnerWallX, -0.35f), new Vector2(0.09f, 8.9f), 0f, guideMaterial);
            CreateWall("ChannelOuterWall", new Vector2(4.55f, -0.35f), new Vector2(0.09f, 8.9f), 0f, guideMaterial);

            var turnCenter = new Vector2(3.55f, 4.10f);
            CreateArc("OuterArc", turnCenter, 1.00f, 0.09f, 24, guideMaterial);
            // A short physical guide inside the visible double-rail curve converts the upward
            // momentum into leftward motion. The marble still travels and collides normally;
            // no position or velocity teleport is used for the launch turn.
            CreateWall("TurnDeflector", new Vector2(4.25f, 4.75f), new Vector2(0.78f, 0.09f), -45f,
                guideMaterial);
            CreateWall("TopOuterGuide", new Vector2(3.12f, 5.10f), new Vector2(0.86f, 0.09f), 0f, guideMaterial);

            var random = new System.Random(LayoutSeed);
            CreateLauncherVisual();
            CreateBumpers(config, random);
            CreateScoringBins(random);
        }

        public Marble SpawnMarble(string kind, Vector2 position, Vector2 initialImpulse, bool canSplit, float childCollisionMultiplier = 1f)
        {
            GameObject go;
            if (marblePrefab != null)
                go = Object.Instantiate(marblePrefab, position, Quaternion.identity, Root);
            else
            {
                go = new GameObject("Marble");
                go.transform.SetParent(Root);
                go.transform.position = position;
                go.AddComponent<Marble>();
            }
            var marble = go.GetComponent<Marble>();
            marble.Configure(kind, initialImpulse, canSplit, childCollisionMultiplier);
            activeMarbles.Add(marble);
            return marble;
        }

        public IReadOnlyList<Marble> ActiveMarbles => activeMarbles;

        public void SetLauncherLoaded(bool visible, string kind)
        {
            if (loadedMarbleVisual == null) return;
            loadedMarbleVisual.enabled = visible;
            loadedMarbleVisual.color = MarbleCatalog.Get(kind).Color;
        }

        public void Forget(Marble marble)
        {
            activeMarbles.Remove(marble);
        }

        public void DestroyAllMarbles()
        {
            foreach (var marble in new List<Marble>(activeMarbles))
                if (marble != null) Object.Destroy(marble.gameObject);
            activeMarbles.Clear();
        }

        public void Dispose()
        {
            if (Root != null) Object.Destroy(Root.gameObject);
            activeMarbles.Clear();
        }

        private void CreateBumpers(LevelConfig config, System.Random random)
        {
            LayoutTemplateIndex = random.Next(bumperLayoutTemplates.Length);
            BumperLayoutTemplate template = bumperLayoutTemplates[LayoutTemplateIndex];
            LayoutTemplateName = template.Name;
            List<GridPoint> points = CreateStaggeredGrid(template);
            layoutSettings.Validate(points.Count);
            HashSet<int> specialPointIndexes = SelectSpecialPointIndexes(points, template, random);
            var generated = new List<Vector2>();
            for (var index = 0; index < points.Count; index++)
            {
                GridPoint point = points[index];
                AddSafeBumper(point.Position, generated, config.BumperPoints, specialPointIndexes.Contains(index));
            }
        }

        private List<GridPoint> CreateStaggeredGrid(BumperLayoutTemplate template)
        {
            int[] rowCounts = CreateRowCounts(layoutSettings.TotalBumperCount);
            LayoutRowCount = rowCounts.Length;
            LayoutColumnCount = 0;
            foreach (int count in rowCounts) LayoutColumnCount = Mathf.Max(LayoutColumnCount, count);

            const float edgeInsetMultiplier = 0.20f;
            var horizontalInset = Bumper2D.Radius * edgeInsetMultiplier;
            var verticalInset = Bumper2D.Radius * 0.75f;
            var verticalSpacing = (BumperLayoutTop - BumperLayoutBottom - verticalInset * 2f) / (LayoutRowCount - 1);
            var points = new List<GridPoint>(layoutSettings.TotalBumperCount);
            for (var row = 0; row < LayoutRowCount; row++)
            {
                int columns = rowCounts[row];
                float phase = template.GetPhase(row, LayoutRowCount);
                float span = columns - 1 + Mathf.Abs(phase) * 2f;
                float horizontalSpacing = (BumperLayoutRight - BumperLayoutLeft - horizontalInset * 2f) / span;
                float y = BumperLayoutTop - verticalInset - verticalSpacing * row;
                for (var column = 0; column < columns; column++)
                {
                    float centeredColumn = column - (columns - 1) * 0.5f + phase;
                    float x = BumperSymmetryAxisX + centeredColumn * horizontalSpacing;
                    points.Add(new GridPoint(new Vector2(x, y)));
                }
            }
            return points;
        }

        private static int[] CreateRowCounts(int totalCount)
        {
            int rowCount = totalCount <= 16 ? 4 : 5;
            var counts = new int[rowCount];
            int baseCount = totalCount / rowCount;
            for (var row = 0; row < rowCount; row++) counts[row] = baseCount;
            int remaining = totalCount - baseCount * rowCount;
            for (var pairOffset = 0; remaining >= 2 && pairOffset < rowCount / 2; pairOffset++)
            {
                counts[pairOffset]++;
                counts[rowCount - 1 - pairOffset]++;
                remaining -= 2;
            }
            if (remaining == 1) counts[rowCount / 2]++;
            return counts;
        }

        private HashSet<int> SelectSpecialPointIndexes(IReadOnlyList<GridPoint> points, BumperLayoutTemplate template, System.Random random)
        {
            var pairs = new List<GridIndexPair>();
            var visited = new HashSet<int>();
            for (var index = 0; index < points.Count; index++)
            {
                if (visited.Contains(index)) continue;
                GridPoint point = points[index];
                int opposite = FindOppositePoint(points, point);
                visited.Add(index);
                visited.Add(opposite);
                pairs.Add(new GridIndexPair(index, opposite));
            }
            layoutSettings.Validate(points.Count);
            var candidates = new List<GridIndexPair>(pairs);
            Shuffle(candidates, random);
            var selected = new HashSet<int>();
            for (var index = 0; index < layoutSettings.SpecialBumperCount / 2; index++)
            {
                GridIndexPair pair = candidates[(index + template.SpecialPairOffset) % candidates.Count];
                selected.Add(pair.First);
                selected.Add(pair.Second);
            }
            return selected;
        }

        private static int FindOppositePoint(IReadOnlyList<GridPoint> points, GridPoint point)
        {
            var mirroredPosition = new Vector2(BumperSymmetryAxisX * 2f - point.Position.x,
                BumperLayoutTop + BumperLayoutBottom - point.Position.y);
            for (var index = 0; index < points.Count; index++)
            {
                GridPoint candidate = points[index];
                if (Vector2.Distance(candidate.Position, mirroredPosition) <= 0.001f) return index;
            }
            throw new System.InvalidOperationException("The staggered bumper grid has no rotationally symmetric counterpart.");
        }

        private void AddSafeBumper(Vector2 position, List<Vector2> existing, int points, bool split)
        {
            if (!IsBumperLayoutPositionAllowed(position) || !IsSafeBumperPosition(position, existing, layoutSettings.MinimumSpacing))
                throw new System.InvalidOperationException($"Bumper layout template {LayoutTemplateIndex} violates clearance at {position}.");
            existing.Add(position);
            CreateBumper(position, points, split);
        }

        private static bool IsSafeBumperPosition(Vector2 candidate, List<Vector2> existing, float minimumSpacing)
        {
            foreach (var position in existing)
                if (Vector2.Distance(candidate, position) < minimumSpacing)
                    return false;
            return true;
        }

        public static bool IsBumperLayoutPositionAllowed(Vector2 position)
        {
            return position.x >= BumperLayoutLeft && position.x <= BumperLayoutRight && position.y >= BumperLayoutBottom && position.y <= BumperLayoutTop;
        }

        private static void Shuffle<T>(IList<T> values, System.Random random)
        {
            for (var index = values.Count - 1; index > 0; index--)
            {
                var swapIndex = random.Next(index + 1);
                var value = values[index];
                values[index] = values[swapIndex];
                values[swapIndex] = value;
            }
        }

        private sealed class BumperLayoutTemplate
        {
            private readonly string name;
            private readonly int firstRowDirection;
            private readonly int specialPairOffset;

            public string Name => name;
            public int SpecialPairOffset => specialPairOffset;

            public BumperLayoutTemplate(string templateName, int firstDirection, int specialOffset)
            {
                name = templateName;
                firstRowDirection = firstDirection;
                specialPairOffset = specialOffset;
            }

            public float GetPhase(int row, int rowCount)
            {
                if (rowCount % 2 == 1)
                {
                    int middleRow = rowCount / 2;
                    if (row == middleRow) return 0f;
                    if (row > middleRow) return -GetPhase(rowCount - 1 - row, rowCount);
                }
                return ((row % 2 == 0 ? firstRowDirection : -firstRowDirection) * 0.25f);
            }
        }

        private readonly struct GridPoint
        {
            public readonly Vector2 Position;

            public GridPoint(Vector2 position)
            {
                Position = position;
            }
        }

        private readonly struct GridIndexPair
        {
            public readonly int First;
            public readonly int Second;

            public GridIndexPair(int first, int second)
            {
                First = first;
                Second = second;
            }
        }

        private void CreateBumper(Vector2 position, int points, bool split)
        {
            bumperPositions.Add(position);
            GameObject go;
            if (bumperPrefab != null)
                go = Object.Instantiate(bumperPrefab, position, Quaternion.identity, Root);
            else
            {
                go = new GameObject("Bumper");
                go.transform.SetParent(Root);
                go.transform.position = position;
                go.AddComponent<Bumper2D>();
            }
            var bumper = go.GetComponent<Bumper2D>();
            bumper.Configure(points, split);
            if (battleArt != null) bumper.SetVisualSprite(battleArt.NormalPeg);
        }

        private void CreatePlayfieldBoundaries(Bounds bounds, PhysicsMaterial2D material)
        {
            var left = bounds.min.x + BoundaryThickness * 0.5f;
            var right = bounds.max.x - BoundaryThickness * 0.5f;
            var top = bounds.max.y - BoundaryThickness * 0.5f;
            var bottomOpeningTop = Mathf.Lerp(bounds.min.y, bounds.max.y, BottomExitHeightRatio);
            var entranceLower = Mathf.Lerp(bounds.min.y, bounds.max.y, LauncherEntranceLowerRatio);
            var entranceUpper = Mathf.Lerp(bounds.min.y, bounds.max.y, LauncherEntranceUpperRatio);
            var cornerRadius = Mathf.Min(BoardCornerRadius, (top - bottomOpeningTop) * 0.25f);
            var roundedLeftTop = top - cornerRadius;

            CreateWall("LeftWall", new Vector2(left, (bottomOpeningTop + roundedLeftTop) * 0.5f),
                new Vector2(BoundaryThickness, roundedLeftTop - bottomOpeningTop), 0f, material);
            CreateWall("TopWall", new Vector2((left + cornerRadius + right) * 0.5f, top),
                new Vector2(right - left - cornerRadius, BoundaryThickness), 0f, material);
            CreateWall("RightWall", new Vector2(right, (bottomOpeningTop + entranceLower) * 0.5f),
                new Vector2(BoundaryThickness, entranceLower - bottomOpeningTop), 0f, material);
            CreateWall("RightWallTop", new Vector2(right, (entranceUpper + top) * 0.5f),
                new Vector2(BoundaryThickness, top - entranceUpper), 0f, material);
            CreateArcRange("TopLeftCorner", new Vector2(left + cornerRadius, top - cornerRadius), cornerRadius,
                BoundaryThickness, 5, 90f, 180f, material);
            CreateOneWayEntranceGate(new Vector2(right, (entranceLower + entranceUpper) * 0.5f),
                new Vector2(BoundaryThickness, entranceUpper - entranceLower), material);
        }

        private void CreateOneWayEntranceGate(Vector2 position, Vector2 size, PhysicsMaterial2D material)
        {
            var go = new GameObject("LauncherEntryGate");
            go.transform.SetParent(Root);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            var localSize = new Vector2(size.y, size.x);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = localSize;
            collider.usedByEffector = true;
            collider.sharedMaterial = material;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 180f;
        }

        private void CreateScoringBins(System.Random random)
        {
            const int slotCount = 8;
            var left = PlayfieldBounds.min.x + BoundaryThickness * 0.5f;
            var right = PlayfieldBounds.max.x - BoundaryThickness * 0.5f;
            var binCenterY = Mathf.Lerp(PlayfieldBounds.min.y, PlayfieldBounds.max.y, 0.057f);
            var dividerHeight = PlayfieldBounds.size.y * 0.127f;
            var zoneHeight = PlayfieldBounds.size.y * 0.099f;
            var binWidth = (right - left) / slotCount;
            for (var i = 1; i < slotCount; i++)
                CreateWall("BinDivider", new Vector2(left + binWidth * i, binCenterY + 0.07f), new Vector2(0.07f, dividerHeight),
                    0f, null);

            var rules = new List<float> { 0f, 1f, 1f, 1.25f, 1.25f, 1.5f, 1.5f, 2f };
            Shuffle(rules, random);
            var starIndex = random.Next(slotCount);
            for (var i = 0; i < slotCount; i++)
            {
                var rule = rules[i];
                var color = rule == 0f
                    ? new Color(0.50f, 0.08f, 0.12f, 0.88f)
                    : rule == 1
                        ? new Color(0.10f, 0.23f, 0.55f, 0.82f)
                        : rule >= 1.5f ? new Color(0.38f, 0.18f, 0.58f, 0.86f) : new Color(0.10f, 0.45f, 0.55f, 0.86f);
                var center = new Vector2(left + binWidth * (i + 0.5f), binCenterY);
                var zoneSize = new Vector2(binWidth - 0.08f, zoneHeight);
                var zone = CreateScoringZone(center, zoneSize, rule, color);
                if (i == starIndex)
                {
                    zone.ConfigureStar(zoneSize, GamePalette.Yellow);
                    StarZone = zone;
                    if (battleArt != null && battleArt.StarSlot != null) zone.SetVisualSprite(battleArt.StarSlot);
                    else CreateWorldLabel("★", center + new Vector2(0f, -0.15f), 44, Color.white);
                }
                else CreateWorldLabel(zone.DisplayLabel, center + new Vector2(0f, -0.15f), 38, Color.white);
            }
        }

        private ScoringZone CreateScoringZone(Vector2 position, Vector2 size, float rule, Color color)
        {
            GameObject go;
            if (scoringZonePrefab != null)
                go = Object.Instantiate(scoringZonePrefab, position, Quaternion.identity, Root);
            else
            {
                go = new GameObject("ScoringZone");
                go.transform.SetParent(Root);
                go.transform.position = position;
                go.AddComponent<ScoringZone>();
            }
            var zone = go.GetComponent<ScoringZone>();
            zone.ConfigureFixed(rule, size, color);
            return zone;
        }

        private void CreateArc(string name, Vector2 center, float radius, float thickness, int segments,
            PhysicsMaterial2D material)
        {
            CreateArcRange(name, center, radius, thickness, segments, 0f, 90f, material);
        }

        private void CreateArcRange(string name, Vector2 center, float radius, float thickness, int segments,
            float startAngle, float endAngle, PhysicsMaterial2D material)
        {
            for (var i = 0; i < segments; i++)
            {
                var a0 = Mathf.Lerp(startAngle, endAngle, (float)i / segments);
                var a1 = Mathf.Lerp(startAngle, endAngle, (float)(i + 1) / segments);
                var mid = (a0 + a1) * 0.5f;
                var radians = mid * Mathf.Deg2Rad;
                var position = center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
                var length = Mathf.Max(0.09f, radius * (a1 - a0) * Mathf.Deg2Rad * 1.25f);
                CreateWall(name + i, position, new Vector2(length, thickness), mid + 90f, material);
            }
        }

        private void CreateWall(string name, Vector2 position, Vector2 size, float rotation,
            PhysicsMaterial2D material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = size;
            if (material != null) collider.sharedMaterial = material;
            var sprite = name == "BinDivider" && battleArt != null ? battleArt.SlotDivider : null;
            if (sprite != null) AddWorldVisual(go.transform, sprite, Color.white, size, 1);
        }

        private void CreateDecoration(string name, Vector2 position, Vector2 size, Color color, int order, Sprite sprite = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root);
            go.transform.position = position;
            AddWorldVisual(go.transform, sprite, color, size, order);
        }

        private void CreateLauncherVisual()
        {
            CreateDecoration("LauncherSpring", LaunchOrigin + new Vector2(0f, -0.37f), new Vector2(0.12f, 0.60f),
                GamePalette.Green, 3, battleArt != null ? battleArt.LauncherSpring : null);
            if (battleArt != null && battleArt.LauncherSpringCap != null)
                CreateDecoration("LauncherSpringCap", LaunchOrigin + new Vector2(0f, -0.65f), new Vector2(0.22f, 0.10f),
                    Color.white, 3, battleArt.LauncherSpringCap);
            var loaded = new GameObject("LoadedMarble");
            loaded.transform.SetParent(Root);
            loaded.transform.position = LaunchOrigin;
            loadedMarbleVisual = AddWorldVisual(loaded.transform,
                battleArt != null ? battleArt.LauncherLoadedMarble : null, GamePalette.Cyan, Vector2.one * 0.38f, 4);
        }

        private static SpriteRenderer AddWorldVisual(Transform parent, Sprite sprite, Color color, Vector2 size, int order)
        {
            var visual = SpriteFactory.AddVisual(parent, sprite ?? SpriteFactory.Square, sprite != null ? Color.white : color, size, order);
            if (sprite != null && sprite.bounds.size.x > 0f && sprite.bounds.size.y > 0f)
                visual.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
            return visual;
        }

        private void CreateWorldLabel(string value, Vector2 position, int fontSize, Color color)
        {
            var go = new GameObject("MultiplierLabel");
            go.transform.SetParent(Root);
            go.transform.position = position;
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = value;
            mesh.font = UiFactory.RuntimeFont;
            mesh.fontSize = fontSize;
            mesh.characterSize = 0.045f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            var renderer = mesh.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mesh.font.material;
            renderer.sortingOrder = 3;
        }
    }
}
