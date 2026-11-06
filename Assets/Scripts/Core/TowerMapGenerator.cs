using System;
using System.Collections.Generic;

namespace PinballRoguelite
{
    public static class TowerMapGenerator
    {
        public const int NormalWeight = 55;
        public const int EliteWeight = 20;
        public const int EventWeight = 15;
        public const int ShopWeight = 5;
        public const int RestWeight = 5;
        public const int MinimumBattleNodes = 3;
        public const int MinimumNormalNodes = 2;
        public const int MaximumAttempts = 32;

        // A complete route must reserve space for combat plus the three strategic
        // node types that make route selection meaningful. The topology remains
        // random; only the layer roles are selected from these legal templates.
        private static readonly MapNodeType[][] RouteTypeTemplates =
        {
            new[] { MapNodeType.Normal, MapNodeType.Normal, MapNodeType.Event, MapNodeType.Rest, MapNodeType.Elite, MapNodeType.Shop, MapNodeType.Normal, MapNodeType.Boss },
            new[] { MapNodeType.Normal, MapNodeType.Normal, MapNodeType.Shop, MapNodeType.Elite, MapNodeType.Event, MapNodeType.Rest, MapNodeType.Normal, MapNodeType.Boss },
            new[] { MapNodeType.Normal, MapNodeType.Normal, MapNodeType.Event, MapNodeType.Normal, MapNodeType.Elite, MapNodeType.Rest, MapNodeType.Normal, MapNodeType.Boss }
        };

        public static TowerMapData Generate(int? requestedSeed = null)
        {
            var seed = requestedSeed ?? Environment.TickCount ^ Guid.NewGuid().GetHashCode();
            for (var attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                var map = GenerateCandidate(seed + attempt * 7919);
                if (ValidateRoutes(map)) return map;
            }
            return CreateFallback(seed);
        }

        public static bool ValidateRoutes(TowerMapData map)
        {
            return ValidateNode(map, map.Layers[0][0], 0, 0, 0, 0, 0);
        }

        private static TowerMapData GenerateCandidate(int seed)
        {
            var rng = new Random(seed);
            var result = new TowerMapData { Seed = seed };
            var template = RouteTypeTemplates[rng.Next(RouteTypeTemplates.Length)];
            var layerCount = template.Length;

            for (var layerIndex = 0; layerIndex < layerCount; layerIndex++)
            {
                var endpoint = layerIndex == 0 || layerIndex == layerCount - 1;
                var count = endpoint ? 1 : rng.Next(2, 5);
                var layer = new List<MapNodeData>(count);
                var type = template[layerIndex];
                for (var nodeIndex = 0; nodeIndex < count; nodeIndex++)
                {
                    layer.Add(new MapNodeData
                    {
                        Id = $"L{layerIndex}N{nodeIndex}",
                        Layer = layerIndex,
                        Index = nodeIndex,
                        Type = type
                    });
                }
                result.Layers.Add(layer);
            }

            for (var layerIndex = 0; layerIndex < result.Layers.Count - 1; layerIndex++)
                ConnectLayers(result.Layers[layerIndex], result.Layers[layerIndex + 1], rng);

            return result;
        }

        private static MapNodeType PickType(Random rng, int layer, int layerCount)
        {
            if (layer == 0)
                return MapNodeType.Normal;
            if (layer == layerCount - 2 && rng.NextDouble() < 0.5) return MapNodeType.Elite;
            var roll = rng.Next(100);
            if (roll < NormalWeight) return MapNodeType.Normal;
            if (roll < NormalWeight + EliteWeight) return MapNodeType.Elite;
            if (roll < NormalWeight + EliteWeight + EventWeight) return MapNodeType.Event;
            if (roll < NormalWeight + EliteWeight + EventWeight + ShopWeight) return MapNodeType.Shop;
            return MapNodeType.Rest;
        }

        private static bool ValidateNode(TowerMapData map, MapNodeData node, int battles, int normals, int shops, int rests, int nonBattles)
        {
            if (node.Type == MapNodeType.Boss) return battles >= MinimumBattleNodes && normals >= MinimumNormalNodes;
            var battle = node.Type == MapNodeType.Normal || node.Type == MapNodeType.Elite;
            battles += battle ? 1 : 0;
            normals += node.Type == MapNodeType.Normal ? 1 : 0;
            shops += node.Type == MapNodeType.Shop ? 1 : 0;
            rests += node.Type == MapNodeType.Rest ? 1 : 0;
            nonBattles = battle ? 0 : nonBattles + 1;
            if (shops > 1 || rests > 1 || nonBattles >= 3 || (node.Layer == map.Layers.Count - 2 && !battle)) return false;
            foreach (var id in node.Connections)
            {
                var next = map.Find(id);
                if (next == null || next.Layer != node.Layer + 1 || !ValidateNode(map, next, battles, normals, shops, rests, nonBattles)) return false;
            }
            return node.Connections.Count > 0;
        }

        private static TowerMapData CreateFallback(int seed)
        {
            var map = new TowerMapData { Seed = seed };
            var types = RouteTypeTemplates[0];
            for (var layer = 0; layer < types.Length; layer++) map.Layers.Add(new List<MapNodeData> { new MapNodeData { Id = $"L{layer}N0", Layer = layer, Index = 0, Type = types[layer] } });
            for (var layer = 0; layer < types.Length - 1; layer++) map.Layers[layer][0].Connections.Add(map.Layers[layer + 1][0].Id);
            return map;
        }

        private static void ConnectLayers(List<MapNodeData> current, List<MapNodeData> next, Random rng)
        {
            if (next.Count == 1)
            {
                foreach (var node in current)
                    node.Connections.Add(next[0].Id);
                return;
            }

            for (var i = 0; i < current.Count; i++)
            {
                var denominator = Math.Max(1, current.Count - 1);
                var mapped = (int)Math.Round((double)i * (next.Count - 1) / denominator);
                AddUnique(current[i].Connections, next[mapped].Id);

                if (rng.NextDouble() < 0.55)
                {
                    var neighbor = mapped + (rng.NextDouble() < 0.5 ? -1 : 1);
                    if (neighbor >= 0 && neighbor < next.Count)
                        AddUnique(current[i].Connections, next[neighbor].Id);
                }
            }

            foreach (var nextNode in next)
            {
                var hasIncoming = current.Exists(node => node.Connections.Contains(nextNode.Id));
                if (hasIncoming)
                    continue;
                var closest = Math.Clamp(nextNode.Index * current.Count / next.Count, 0, current.Count - 1);
                AddUnique(current[closest].Connections, nextNode.Id);
            }
        }

        private static void AddUnique(List<string> values, string value)
        {
            if (!values.Contains(value))
                values.Add(value);
        }
    }
}
