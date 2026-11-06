using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite
{
    public sealed class MapScreen : MonoBehaviour
    {
        public event Action<MapNodeData> NodeSelected;
        public event Action PlaceholderCompleted;

        private LevelUiView uiView;
        private TowerMapData mapData;
        private HashSet<string> available;
        private HashSet<string> completed;

        private void OnDestroy()
        {
            if (uiView != null) uiView.ContinuePressed -= OnPlaceholderContinue;
        }

        public void Setup(TowerMapData data, HashSet<string> availableIds, HashSet<string> completedIds, LevelUiView levelUiPrefab)
        {
            if (levelUiPrefab == null) throw new MissingReferenceException("LevelUI prefab is required.");
            uiView = Instantiate(levelUiPrefab, transform);
            uiView.name = "LevelUI";
            uiView.Initialize();
            uiView.ContinuePressed += OnPlaceholderContinue;
            mapData = data;
            available = new HashSet<string>(availableIds);
            completed = new HashSet<string>(completedIds);
            RenderMap();
        }

        private void RenderMap()
        {
            uiView.ClearMap();
            Canvas.ForceUpdateCanvases();

            var rect = uiView.MapArea.rect;
            var width = rect.width;
            var height = rect.height;
            var positions = new Dictionary<string, Vector2>();
            const float margin = 78f;

            for (var layerIndex = 0; layerIndex < mapData.Layers.Count; layerIndex++)
            {
                var layer = mapData.Layers[layerIndex];
                var y = height * 0.5f - margin - (height - margin * 2f) * layerIndex /
                    Mathf.Max(1, mapData.Layers.Count - 1);
                for (var nodeIndex = 0; nodeIndex < layer.Count; nodeIndex++)
                {
                    var x = -width * 0.5f + width * (nodeIndex + 1f) / (layer.Count + 1f);
                    positions[layer[nodeIndex].Id] = new Vector2(x, y);
                }
            }

            foreach (var layer in mapData.Layers)
            foreach (var node in layer)
            foreach (var targetId in node.Connections)
            {
                if (!positions.TryGetValue(targetId, out var target)) continue;
                var color = completed.Contains(node.Id) ? GamePalette.Cyan : GamePalette.Hex("334155");
                uiView.CreateConnection(positions[node.Id], target, color);
            }

            foreach (var layer in mapData.Layers)
            foreach (var node in layer)
                CreateNodeButton(node, positions[node.Id]);

            uiView.SetProgress($"{mapData.Layers.Count} 层路线 · 已通过 {completed.Count} 节点");
        }

        private void CreateNodeButton(MapNodeData node, Vector2 position)
        {
            var isAvailable = available.Contains(node.Id);
            var isCompleted = completed.Contains(node.Id);
            var color = isCompleted ? GamePalette.Green : TypeColor(node.Type);
            if (!isAvailable && !isCompleted) color = Color.Lerp(color, GamePalette.Panel, 0.68f);
            uiView.CreateNodeButton(node.Id, TypeSymbol(node.Type), TypeLabel(node.Type), color, isAvailable,
                isCompleted, node.Type == MapNodeType.Boss, position, () => NodeSelected?.Invoke(node));
        }

        public void ShowPlaceholder(MapNodeData node)
        {
            var body = string.Empty;
            switch (node.Type)
            {
                case MapNodeType.Event:
                    body = "你发现了一颗闪闪发亮的幸运弹珠。\n（随机事件使用基础占位逻辑）";
                    break;
                case MapNodeType.Shop:
                    body = "旅行商人正在整理货架。\n（商店使用基础占位逻辑）";
                    break;
                default:
                    body = "你在安全的营地休整片刻。\n（休息点使用基础占位逻辑）";
                    break;
            }
            uiView.ShowPlaceholder(TypeLabel(node.Type), TypeColor(node.Type), body);
        }

        private void OnPlaceholderContinue()
        {
            PlaceholderCompleted?.Invoke();
        }

        public static Color TypeColor(MapNodeType type)
        {
            switch (type)
            {
                case MapNodeType.Elite: return GamePalette.Orange;
                case MapNodeType.Event: return GamePalette.Purple;
                case MapNodeType.Shop: return GamePalette.Yellow;
                case MapNodeType.Rest: return GamePalette.Green;
                case MapNodeType.Boss: return GamePalette.Red;
                default: return GamePalette.Blue;
            }
        }

        public static string TypeLabel(MapNodeType type)
        {
            switch (type)
            {
                case MapNodeType.Elite: return "精英关";
                case MapNodeType.Event: return "随机事件";
                case MapNodeType.Shop: return "商店";
                case MapNodeType.Rest: return "休息点";
                case MapNodeType.Boss: return "Boss";
                default: return "普通关";
            }
        }

        private static string TypeSymbol(MapNodeType type)
        {
            switch (type)
            {
                case MapNodeType.Elite: return "精";
                case MapNodeType.Event: return "?";
                case MapNodeType.Shop: return "店";
                case MapNodeType.Rest: return "息";
                case MapNodeType.Boss: return "王";
                default: return "普";
            }
        }
    }
}
