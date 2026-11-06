# PinballRoguelite Project Guidance

## Purpose

- Unity 6 portrait 2D pinball Roguelite demo with a complete title, tower map, pinball level, settlement, and boss loop.
- Preserve the established gameplay rules and use temporary geometric art unless the user requests a redesign.
- The external Godot project is reference-only; never modify or delete it from this project.

## Structure

- Assets/Scripts/Core: run state, tower data, generation, palette, and the GameRoot screen flow.
- Assets/Scripts/Map: tower-map presentation and node selection.
- Assets/Scripts/Gameplay: board construction, marble physics, scoring, level configuration, and gameplay flow.
- Assets/Scripts/UI and Assets/Scripts/Shared: reusable runtime UI and visual helpers.
- Assets/Resources/Prefabs/UI.prefab: runtime UI root, Canvas configuration, and EventSystem.
- Assets/Editor and Assets/Tests: validation/build tooling and EditMode/PlayMode tests. `WebGlBuild` is the explicit local WebGL/GitHub Pages export entry point.
- Tools/External: retained third-party source references; it is not a Unity runtime asset root.

## Boundaries

- Target Unity 6000.3.27f2; keep the project portrait-first and Windows Editor compatible.
- Use Rigidbody2D and Collider2D for the launch path and playfield physics; do not replace travel with teleportation.
- Preserve the fixed lower-right launcher, independent rising lane, top turn, and center-playfield entry.
- Generate a fresh central bumper layout for every gameplay-screen creation while preserving peg clearance and protected launcher/bin regions.
- Preserve eight bottom settlement slots: two ×1, one ×0, and five random ×2～×5 slots, with their positions shuffled per board.
- Keep generated Unity folders and task Cache out of maintained source.
- This directory currently has no Git repository metadata; do not claim commit or push delivery.

## Navigation and Definition of Done

- Start with .agents/skills/ai-index/SKILL.md and select only the relevant module route.
- Follow .agents/skills/ai-index/verification.md for the smallest convincing Unity check.
- A change is done when source references are valid, relevant Unity checks pass, and disposable task artifacts are cleaned.

