# UI and Shared Module

Assets/Resources/Prefabs/UI.prefab is the runtime UI root. It owns GameCanvas, CanvasScaler, GraphicRaycaster, EventSystem, StandaloneInputModule, GameplayUiPrefabRegistry, and LevelUiPrefabRegistry. GameRoot loads this prefab before it creates a screen.

Assets/Prefabs/UI/GameplayUI.prefab is the isolated portrait gameplay HUD. Its stable SafeArea hierarchy owns TopHUD/BattleInfo, BoardHintText, BottomHUD, FloatingFeedbackLayer, and ResultOverlay with VictoryPanel and DefeatPanel. SafeAreaFitter maps this hierarchy to Screen.safeArea so notches, cutouts, and gesture insets do not cover interactive UI. It contains Image, TextMeshProUGUI, and Button nodes only; GameplayUiView binds named serialized references and forwards input to GameplayScreen without computing gameplay state. Assets/UI/Gameplay/Fonts/GameplayUIFont.asset supplies the temporary Chinese-capable TMP font. GameplayUiPrefabBuilder regenerates the prefab, applies the 780×1688 width-priority Canvas Scaler profile, and reconnects the runtime UI root registry.

Assets/Prefabs/UI/LevelUI.prefab is the isolated portrait tower-level interface. Its stable SafeArea hierarchy owns Header, MapPanel/MapArea, legends, and PlaceholderModal. LevelUiView binds explicit serialized references, creates only visual map lines and node controls from MapScreen-provided data, and forwards the placeholder continuation event without deciding route legality. LevelUiPrefabBuilder regenerates this prefab and reconnects LevelUiPrefabRegistry on the runtime UI root.

TitleScreen, ResultOverlay, and LaunchHoldButton own their respective runtime UI behavior. GameplayUiView owns gameplay-HUD bindings and result-panel presentation, while CombatDisplay only animates its bound combat placeholders. UiFactory creates legacy anchored controls beneath the prefab Canvas for non-gameplay screens. SpriteFactory creates temporary geometric visuals used by gameplay and map screens.

Keep the 780 by 1688 portrait reference layout responsive, with width-priority scaling. Mouse and touch input share the prefab EventSystem path.
