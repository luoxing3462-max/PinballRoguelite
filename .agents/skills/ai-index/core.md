# Core Module

Entry points: Assets/Scenes/Main.unity and Assets/Scripts/Core/GameRoot.cs.

GameRoot.Awake configures the portrait runtime, camera and physics, then GameRoot.CreateUi instantiates Resources/Prefabs/UI before showing the title screen. PortraitBoardCameraFitter preserves the fixed world-space board proportions by expanding the orthographic viewport only when a narrow portrait aspect needs more width. RunState owns the current map, available/completed node IDs, active node, and run progression. TowerMapGenerator creates each run's layered route graph; MapData defines the node and map records.

Keep combat physics in the gameplay module and map rendering in the map module. Core may coordinate screen transitions through their public setup and events.
