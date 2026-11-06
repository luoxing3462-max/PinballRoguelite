# Structure Rules

- GameRoot owns scene bootstrap and transitions among title, map, gameplay, and result flows.
- RunState, TowerMapGenerator, and map data types own run progression and route legality.
- MapScreen owns tower presentation and node input; it does not own combat rules.
- GameplayScreen coordinates one combat level, while PinballBoard creates physical board objects and tracks active marbles.
- Marble, Bumper2D, and ScoringZone own local Rigidbody2D and Collider2D behavior. ScoreManager owns per-launch scoring and settlement.
- UI.prefab owns the runtime UI root, Canvas configuration, raycaster, and EventSystem. GameRoot instantiates it before adding a screen.
- UiFactory and SpriteFactory are shared construction owners; screen-specific behavior stays with its screen.
- Add runtime code to the existing owning module. Keep Editor tooling in Assets/Editor and maintained tests in Assets/Tests.