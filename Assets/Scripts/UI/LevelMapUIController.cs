using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinballRoguelite
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class LevelMapUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;
        [SerializeField] private VisualTreeAsset _nodeTemplate;

        private VisualElement _root;
        private VisualElement _mapCanvas;
        private VisualElement _placeholderOverlay;
        private Label _progressLabel;
        private Label _placeholderTitle;
        private Label _placeholderBody;
        private Button _placeholderContinueButton;
        private TowerMapData _mapData;
        private HashSet<string> _availableIds;
        private HashSet<string> _completedIds;
        private Action<MapNodeData> _nodeSelected;
        private Action _placeholderCompleted;
        private bool _handlersAttached;

        private void Awake()
        {
            if (_document == null) _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindVisualElements();
        }

        private void OnDisable()
        {
            UnbindVisualElements();
            _nodeSelected = null;
            _placeholderCompleted = null;
        }

        public void Initialize(TowerMapData mapData, HashSet<string> availableIds, HashSet<string> completedIds,
            Action<MapNodeData> nodeSelected, Action placeholderCompleted)
        {
            BindVisualElements();
            if (_mapCanvas == null || _nodeTemplate == null)
                throw new MissingReferenceException("LevelMapUI requires LevelMap.uxml bindings and Node.uxml.");

            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _availableIds = new HashSet<string>(availableIds ?? new HashSet<string>());
            _completedIds = new HashSet<string>(completedIds ?? new HashSet<string>());
            _nodeSelected = nodeSelected;
            _placeholderCompleted = placeholderCompleted;
            _placeholderOverlay.AddToClassList("is-hidden");
            _progressLabel.text = $"{_mapData.Layers.Count} 层路线 · 已通过 {_completedIds.Count} 节点";
            Show();
            RenderMap();
        }

        public void Show()
        {
            BindVisualElements();
            if (_root == null) return;
            _root.style.display = DisplayStyle.Flex;
            if (_mapData != null) RenderMap();
        }

        public void Hide()
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        public void ShowPlaceholder(string title, Color titleColor, string body)
        {
            BindVisualElements();
            _placeholderTitle.text = title;
            _placeholderTitle.style.color = titleColor;
            _placeholderBody.text = body;
            _placeholderOverlay.RemoveFromClassList("is-hidden");
        }

        public void Release()
        {
            _nodeSelected = null;
            _placeholderCompleted = null;
            _mapData = null;
            _availableIds = null;
            _completedIds = null;
            if (_mapCanvas != null) _mapCanvas.Clear();
        }

        private void BindVisualElements()
        {
            if (_handlersAttached || _document == null) return;

            _root = _document.rootVisualElement;
            _mapCanvas = _root.Q<VisualElement>("map-canvas");
            _placeholderOverlay = _root.Q<VisualElement>("placeholder-overlay");
            _progressLabel = _root.Q<Label>("progress-label");
            _placeholderTitle = _root.Q<Label>("placeholder-title");
            _placeholderBody = _root.Q<Label>("placeholder-body");
            _placeholderContinueButton = _root.Q<Button>("placeholder-continue-button");
            if (_mapCanvas == null || _placeholderOverlay == null || _progressLabel == null || _placeholderTitle == null ||
                _placeholderBody == null || _placeholderContinueButton == null) return;

            _mapCanvas.RegisterCallback<GeometryChangedEvent>(OnMapCanvasGeometryChanged);
            _placeholderContinueButton.clicked += OnPlaceholderContinueClicked;
            _handlersAttached = true;
        }

        private void UnbindVisualElements()
        {
            if (!_handlersAttached) return;
            _mapCanvas.UnregisterCallback<GeometryChangedEvent>(OnMapCanvasGeometryChanged);
            _placeholderContinueButton.clicked -= OnPlaceholderContinueClicked;
            _handlersAttached = false;
        }

        private void OnMapCanvasGeometryChanged(GeometryChangedEvent changeEvent)
        {
            if (_mapData != null) RenderMap();
        }

        private void RenderMap()
        {
            if (_mapData == null || _mapCanvas == null || _mapCanvas.contentRect.width <= 1f || _mapCanvas.contentRect.height <= 1f)
                return;

            _mapCanvas.Clear();
            Dictionary<string, Vector2> positions = BuildNodePositions();
            CreateConnections(positions);
            CreateNodes(positions);
        }

        private Dictionary<string, Vector2> BuildNodePositions()
        {
            float width = _mapCanvas.contentRect.width;
            float height = _mapCanvas.contentRect.height;
            float horizontalMargin = Mathf.Min(66f, width * 0.12f);
            float verticalMargin = Mathf.Min(72f, height * 0.08f);
            Dictionary<string, Vector2> positions = new Dictionary<string, Vector2>();

            for (int layerIndex = 0; layerIndex < _mapData.Layers.Count; layerIndex++)
            {
                List<MapNodeData> layer = _mapData.Layers[layerIndex];
                float y = verticalMargin + (height - verticalMargin * 2f) * layerIndex / Mathf.Max(1, _mapData.Layers.Count - 1);
                for (int nodeIndex = 0; nodeIndex < layer.Count; nodeIndex++)
                {
                    float x = horizontalMargin + (width - horizontalMargin * 2f) * (nodeIndex + 1f) / (layer.Count + 1f);
                    positions[layer[nodeIndex].Id] = new Vector2(x, y);
                }
            }

            return positions;
        }

        private void CreateConnections(IReadOnlyDictionary<string, Vector2> positions)
        {
            foreach (List<MapNodeData> layer in _mapData.Layers)
            foreach (MapNodeData node in layer)
            foreach (string targetId in node.Connections)
            {
                if (!positions.TryGetValue(targetId, out Vector2 target)) continue;
                MapConnectionVisual connection = new MapConnectionVisual(positions[node.Id], target,
                    _completedIds.Contains(node.Id) ? GamePalette.Cyan : GamePalette.Hex("334155"));
                _mapCanvas.Add(connection);
            }
        }

        private void CreateNodes(IReadOnlyDictionary<string, Vector2> positions)
        {
            foreach (List<MapNodeData> layer in _mapData.Layers)
            foreach (MapNodeData node in layer)
                CreateNode(node, positions[node.Id]);
        }

        private void CreateNode(MapNodeData node, Vector2 position)
        {
            TemplateContainer instance = _nodeTemplate.Instantiate();
            VisualElement root = instance.Q<VisualElement>("node-root");
            Button button = instance.Q<Button>("node-button");
            Label label = instance.Q<Label>("node-type-label");
            bool available = _availableIds.Contains(node.Id);
            bool completed = _completedIds.Contains(node.Id);
            bool boss = node.Type == MapNodeType.Boss;
            float size = boss ? 108f : 88f;
            Color color = completed ? GamePalette.Green : MapScreen.TypeColor(node.Type);
            if (!available && !completed) color = Color.Lerp(color, GamePalette.Hex("0C1D33"), 0.68f);

            root.style.left = position.x - 50f;
            root.style.top = position.y - size * 0.5f;
            if (boss) root.AddToClassList("boss");
            button.text = completed ? "✓" : MapScreen.TypeSymbol(node.Type);
            button.style.backgroundColor = color;
            button.SetEnabled(available);
            label.text = MapScreen.TypeLabel(node.Type);
            label.style.color = available ? Color.white : GamePalette.Muted;
            button.clicked += () => _nodeSelected?.Invoke(node);
            _mapCanvas.Add(instance);
        }

        private void OnPlaceholderContinueClicked()
        {
            _placeholderOverlay.AddToClassList("is-hidden");
            _placeholderCompleted?.Invoke();
        }

        private sealed class MapConnectionVisual : VisualElement
        {
            private readonly Vector2 _from;
            private readonly Vector2 _to;
            private readonly Color _color;

            public MapConnectionVisual(Vector2 from, Vector2 to, Color color)
            {
                _from = from;
                _to = to;
                _color = color;
                pickingMode = PickingMode.Ignore;
                style.position = Position.Absolute;
                style.left = 0f;
                style.right = 0f;
                style.top = 0f;
                style.bottom = 0f;
                generateVisualContent += Draw;
            }

            private void Draw(MeshGenerationContext context)
            {
                context.painter2D.strokeColor = _color;
                context.painter2D.lineWidth = 7f;
                context.painter2D.BeginPath();
                context.painter2D.MoveTo(_from);
                context.painter2D.LineTo(_to);
                context.painter2D.Stroke();
            }
        }
    }
}
