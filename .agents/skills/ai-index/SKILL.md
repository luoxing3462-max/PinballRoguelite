---
name: ai-index
description: Navigate PinballRoguelite ownership, stable entry points, verification, and Cache rules.
---

# PinballRoguelite AI Index

Use index.json as the maintained route map. Select one module before reading source and expand only through declared dependencies needed by the task.

- core: application bootstrap, run state, and tower data.
- map: map presentation and node selection.
- gameplay: pinball board, launch physics, marbles, scoring, and level flow.
- ui-shared: runtime UI screens and reusable visual factories.
- validation: Editor validation plus EditMode and PlayMode tests.

Stable repository, structure, and verification rules are linked from index.json. Cache ownership and cleanup rules are in cache/README.md; cache/current.json is generated operational state and must not be committed.