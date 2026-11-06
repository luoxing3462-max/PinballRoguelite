using System.Collections.Generic;

namespace PinballRoguelite
{
    public sealed class RunState
    {
        public TowerMapData Map { get; private set; }
        public readonly HashSet<string> AvailableIds = new HashSet<string>();
        public readonly HashSet<string> CompletedIds = new HashSet<string>();
        public RunEffects Effects { get; } = new RunEffects();
        public MapNodeData ActiveNode { get; private set; }
        private readonly List<string> equippedMarbleIds = new List<string>();
        public IReadOnlyList<string> EquippedMarbleIds => equippedMarbleIds.Count == 2 ? equippedMarbleIds : DefaultMarbles;
        private static readonly IReadOnlyList<string> DefaultMarbles = new[] { MarbleCatalog.Normal, MarbleCatalog.Split };

        public void StartNewRun(int? seed = null)
        {
            Map = TowerMapGenerator.Generate(seed);
            AvailableIds.Clear();
            CompletedIds.Clear();
            ActiveNode = null;
            Effects.Reset();
            equippedMarbleIds.Clear();
            foreach (var node in Map.Layers[0])
                AvailableIds.Add(node.Id);
        }

        public bool SetEquippedMarbles(IReadOnlyList<string> marbleIds)
        {
            if (marbleIds == null || marbleIds.Count != 2 || marbleIds[0] == marbleIds[1]) return false;
            equippedMarbleIds.Clear();
            equippedMarbleIds.Add(MarbleCatalog.Get(marbleIds[0]).Id);
            equippedMarbleIds.Add(MarbleCatalog.Get(marbleIds[1]).Id);
            return true;
        }

        public bool ChooseNode(MapNodeData node)
        {
            if (node == null || !AvailableIds.Contains(node.Id))
                return false;
            ActiveNode = node;
            return true;
        }

        public void CompleteActiveNode()
        {
            if (ActiveNode == null)
                return;
            CompletedIds.Add(ActiveNode.Id);
            AvailableIds.Clear();
            foreach (var nextId in ActiveNode.Connections)
                AvailableIds.Add(nextId);
            ActiveNode = null;
        }
    }
}
