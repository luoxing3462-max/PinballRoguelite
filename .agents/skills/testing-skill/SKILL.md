---
name: testing-skill
description: Run and maintain PinballRoguelite Unity EditMode and PlayMode checks when gameplay, board physics, scoring, or runtime UI changes.
---

# PinballRoguelite Testing

- Use `Assets/Tests/EditMode/CoreTests.cs` for deterministic state and scoring contracts.
- Use `Assets/Tests/PlayMode/GameplayFlowTests.cs` for generated-board invariants, Rigidbody2D collisions, launcher travel, settlement, and runtime UI flow, including the one-instance GameplayUI prefab hierarchy and result-panel callback wiring.
- Run Unity 6000.3.27f2 in batch mode. Put logs and result XML in the current `Cache/temp-*` task owner, then remove that owner after the result is delivered.
- For bumper-layout changes, sample every template through explicit seeds and assert count, symmetry, protected-route clearance, minimum pairwise spacing, special-bumper allocation, and at least one physical launch/settlement flow.
- Update an existing relevant test before adding another test. Keep each test focused on one durable behavior.
