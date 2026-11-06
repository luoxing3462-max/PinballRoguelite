# Map Module

Entry point: Assets/Scripts/Map/MapScreen.cs.

MapScreen renders the generated layered tree through the registered LevelUI prefab, enables only legally available nodes, reports node selection to GameRoot, and supplies basic placeholder completion for event, shop, and rest nodes. LevelUiView owns the anchored visual hierarchy and button presentation; MapScreen remains the owner of map data, route availability, and node actions.

Route generation and progression legality belong to Core. Combat nodes hand off to Gameplay; do not duplicate those rules in the map presentation.
