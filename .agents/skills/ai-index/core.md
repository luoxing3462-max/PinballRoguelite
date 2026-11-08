# Core Module

Entry points: Assets/Scenes/Main.unity and Assets/Scripts/Core/GameRoot.cs.

GameRoot.Awake configures the portrait runtime, camera and physics, then GameRoot.CreateUi instantiates Resources/Prefabs/UI before showing the title screen. After a new run's two-marble selection is confirmed, GameRoot activates RunState.TryChooseOpeningNode and enters that fixed normal battle directly; the map is first presented only when the opening node later completes. ShowMap creates MapScreen and injects the LevelMapUIController prefab registered by LevelUiPrefabRegistry. PortraitBoardCameraFitter preserves the fixed world-space board proportions by expanding the orthographic viewport only when a narrow portrait aspect needs more width. GameplayScreen temporarily constrains that camera to the viewport derived from the editable GameplayUI PinballBoardArea and recenters it on PinballBoard.PlayfieldBounds; its teardown must call ResetViewport before another screen owns the camera. RunState owns the current map, available/completed node IDs, active node, and run progression. TowerMapGenerator creates each run's layered route graph; MapData defines the node and map records.

Keep combat physics in the gameplay module and map rendering in the map module. Core may coordinate screen transitions through their public setup and events.
