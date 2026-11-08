using UnityEngine;

namespace PinballRoguelite
{
    public sealed class GameRoot : MonoBehaviour
    {
        private const string UiPrefabResourcePath = "Prefabs/UI";
        public RunState RunState { get; private set; } = new RunState();
        public Component CurrentScreen { get; private set; }
        private Canvas canvas;
        private GameplayUiPrefabRegistry gameplayUiPrefabRegistry;
        private LevelUiPrefabRegistry levelUiPrefabRegistry;
        private MarbleSelectionUiPrefabRegistry marbleSelectionUiPrefabRegistry;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            Physics2D.gravity = new Vector2(0f, -10.8f);
            EnsureCamera();
            CreateUi();
            ShowTitle();
        }

        private void EnsureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                camera = go.AddComponent<Camera>();
            }
            camera.orthographic = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = GamePalette.Background;
            camera.clearFlags = CameraClearFlags.SolidColor;
            var fitter = camera.GetComponent<PortraitBoardCameraFitter>();
            if (fitter == null) fitter = camera.gameObject.AddComponent<PortraitBoardCameraFitter>();
            fitter.Apply();
        }

        private void CreateUi()
        {
            GameObject uiPrefab = Resources.Load<GameObject>(UiPrefabResourcePath);
            if (uiPrefab == null) throw new MissingReferenceException($"Missing Resources prefab: {UiPrefabResourcePath}");
            GameObject uiRoot = Instantiate(uiPrefab);
            uiRoot.name = "UI";
            canvas = uiRoot.GetComponentInChildren<Canvas>();
            if (canvas == null) throw new MissingComponentException("UI.prefab must contain a Canvas component.");
            gameplayUiPrefabRegistry = uiRoot.GetComponent<GameplayUiPrefabRegistry>();
            if (gameplayUiPrefabRegistry == null || gameplayUiPrefabRegistry.GameplayUiPrefab == null)
                throw new MissingReferenceException("UI.prefab must reference GameplayUI.prefab through GameplayUiPrefabRegistry.");
            levelUiPrefabRegistry = uiRoot.GetComponent<LevelUiPrefabRegistry>();
            if (levelUiPrefabRegistry == null || levelUiPrefabRegistry.LevelMapUiPrefab == null)
                throw new MissingReferenceException("UI.prefab must reference LevelMapUI.prefab through LevelUiPrefabRegistry.");
            marbleSelectionUiPrefabRegistry = uiRoot.GetComponent<MarbleSelectionUiPrefabRegistry>();
            if (marbleSelectionUiPrefabRegistry == null || marbleSelectionUiPrefabRegistry.MarbleSelectionUiPrefab == null)
                throw new MissingReferenceException("UI.prefab must reference MarbleSelectionUI.prefab through MarbleSelectionUiPrefabRegistry.");
        }

        public void ShowTitle()
        {
            var screen = ReplaceScreen<TitleScreen>("TitleScreen");
            screen.StartPressed += StartRun;
        }

        private void StartRun()
        {
            RunState.StartNewRun();
            ShowMarbleSelection();
        }

        private void ShowMarbleSelection()
        {
            var screen = ReplaceScreen<MarbleSelectionScreen>("MarbleSelectionScreen");
            screen.Setup(marbleSelectionUiPrefabRegistry.MarbleSelectionUiPrefab);
            screen.SelectionConfirmed += marbleIds =>
            {
                if (!RunState.SetEquippedMarbles(marbleIds)) return;
                if (RunState.TryChooseOpeningNode(out MapNodeData openingNode))
                    ShowGameplay(openingNode);
                else
                    ShowMap();
            };
        }

        public void ShowMap()
        {
            var screen = ReplaceScreen<MapScreen>("MapScreen");
            screen.Setup(RunState.Map, RunState.AvailableIds, RunState.CompletedIds, levelUiPrefabRegistry.LevelMapUiPrefab);
            screen.NodeSelected += OnNodeSelected;
            screen.PlaceholderCompleted += OnPlaceholderCompleted;
        }

        private void OnNodeSelected(MapNodeData node)
        {
            if (!RunState.ChooseNode(node)) return;
            if (node.Type == MapNodeType.Normal || node.Type == MapNodeType.Elite || node.Type == MapNodeType.Boss)
                ShowGameplay(node);
            else if (node.Type == MapNodeType.Event)
                ShowEventChoices();
            else
                ((MapScreen)CurrentScreen).ShowPlaceholder(node);
        }

        private void ShowEventChoices()
        {
            var screen = ReplaceScreen<EffectChoiceScreen>("EventChoices");
            screen.Setup("随机事件：祝福与诅咒", "选择一项立即生效，本轮爬塔持续", RunEffects.ChooseEvent(System.Guid.NewGuid().GetHashCode()));
            screen.ChoiceSelected += id => { RunState.Effects.Apply(id); RunState.CompleteActiveNode(); ShowMap(); };
        }

        private void OnPlaceholderCompleted()
        {
            RunState.CompleteActiveNode();
            ShowMap();
        }

        public void ShowGameplay(MapNodeData node)
        {
            var screen = ReplaceScreen<GameplayScreen>("GameplayScreen");
            screen.Setup(node, gameplayUiPrefabRegistry.GameplayUiPrefab, RunState.Effects, RunState.Map.Layers.Count, RunState.EquippedMarbleIds);
            screen.LevelWon += OnLevelWon;
            screen.RetryRequested += () => ShowGameplay(RunState.ActiveNode);
            screen.RestartRunRequested += StartRun;
        }

        private void OnLevelWon()
        {
            if (RunState.ActiveNode != null && (RunState.ActiveNode.Type == MapNodeType.Normal || RunState.ActiveNode.Type == MapNodeType.Elite))
            {
                var elite = RunState.ActiveNode.Type == MapNodeType.Elite;
                var screen = ReplaceScreen<EffectChoiceScreen>("UpgradeChoices");
                screen.Setup(elite ? "精英奖励：三选一" : "关卡奖励：三选一", elite ? "精英关提高了稀有强化出现权重" : "选择一项强化，本轮爬塔持续", RunEffects.ChooseUpgrades(elite, System.Guid.NewGuid().GetHashCode()));
                screen.ChoiceSelected += id => { RunState.Effects.Apply(id); RunState.CompleteActiveNode(); ShowMap(); };
                return;
            }
            RunState.CompleteActiveNode();
            ShowMap();
        }

        private T ReplaceScreen<T>(string screenName) where T : Component
        {
            if (CurrentScreen != null)
                Destroy(CurrentScreen.gameObject);
            var rect = UiFactory.CreateRect(screenName, canvas.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            var screen = rect.gameObject.AddComponent<T>();
            CurrentScreen = screen;
            return screen;
        }
    }
}
