# Verification Rules

- Use Unity Editor 6000.3.27f2 for project checks.
- Prefer the smallest check proving the change: EditMode for deterministic core logic and PlayMode for scene, UI, Rigidbody2D, Collider2D, randomized layout, score slots, or launch-path behavior.
- The symmetric-template board, boundary containment, eight-slot settlement row, and physical launch acceptance owner is Assets/Tests/PlayMode/GameplayFlowTests.cs; its tests cover count, mirrored pairs, template selection, protected-region bounds, one-way launcher entry, continuous bounded physics for all six marble types, high-charge bounce containment, and minimum spacing.
- `.agents/skills/testing-skill/SKILL.md` owns the stable Unity test-level routing for this project.
- Assets/Editor/ProjectBuilder.cs contains project validation/build entry points when a broader requested contract requires them.
- Run Unity quietly in batch mode and place logs and result XML inside the current Cache/temp-* task owner.
- A compile/import success does not replace the PlayMode rule-count, random-range, x0, label, and physical-shot checks for scoring-row changes.
- Gameplay UI acceptance is owned by Assets/Tests/PlayMode/GameplayFlowTests.cs: it verifies one GameplayUI prefab instance, required stable hierarchy nodes, victory/defeat panel switching, victory event forwarding, and the existing physical launch/combat flow. Inspect the Game view manually at 540×960 and 1080×1920 before accepting visual polish; batch PlayMode does not replace a visual composition review.
- Read the final changed source and Unity result artifact before reporting success; distinguish Editor evidence from untested mobile-device behavior.
