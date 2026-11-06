using System;
using System.Collections.Generic;

namespace PinballRoguelite
{
    public enum MapNodeType
    {
        Normal,
        Elite,
        Event,
        Shop,
        Rest,
        Boss
    }

    [Serializable]
    public sealed class MapNodeData
    {
        public string Id;
        public int Layer;
        public int Index;
        public MapNodeType Type;
        public readonly List<string> Connections = new List<string>();
    }

    [Serializable]
    public sealed class TowerMapData
    {
        public int Seed;
        public readonly List<List<MapNodeData>> Layers = new List<List<MapNodeData>>();

        public MapNodeData Find(string id)
        {
            foreach (var layer in Layers)
            foreach (var node in layer)
                if (node.Id == id)
                    return node;
            return null;
        }
    }
}
