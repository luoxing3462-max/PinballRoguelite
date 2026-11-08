using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinballRoguelite
{
    public sealed class MapScreen : MonoBehaviour
    {
        public event Action<MapNodeData> NodeSelected;
        public event Action PlaceholderCompleted;

        private LevelMapUIController uiView;
        private TowerMapData mapData;
        private HashSet<string> available;
        private HashSet<string> completed;

        private void OnDestroy()
        {
            if (uiView != null) uiView.Release();
        }

        public void Setup(TowerMapData data, HashSet<string> availableIds, HashSet<string> completedIds, LevelMapUIController levelUiPrefab)
        {
            if (levelUiPrefab == null) throw new MissingReferenceException("LevelMapUI prefab is required.");
            uiView = Instantiate(levelUiPrefab, transform);
            uiView.name = "LevelMapUI";
            mapData = data;
            available = new HashSet<string>(availableIds);
            completed = new HashSet<string>(completedIds);
            uiView.Initialize(mapData, available, completed, node => NodeSelected?.Invoke(node), OnPlaceholderContinue);
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

        public static string TypeSymbol(MapNodeType type)
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
