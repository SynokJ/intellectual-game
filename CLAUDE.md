# Scripting Rules (Unity C#)

Apply to every new or modified script under `Assets/_Project`.

## MVC layout

Each mini-game lives in `Assets/_Project/MiniGames/<Game>/` with three folders:

- **Models/** – `ScriptableObject`s holding data and settings (`<Game>LevelData`, config, state assets). No scene/GameObject references, no UI, no effects.
- **Views/** – scripts for presentation only: effects, animations, particle systems, visual feedback (`<Game>View`, element views like `SheepView`). No game rules, no win/lose decisions. Expose methods like `Play...`/`Show...`/`Set...` for output and events for input.
- **Controllers/** – scripts that manage objects, connect components (Model + View + scene references) and drive the game flow (`<Game>Game`): receive View input, update the Model, tell the View what to play/render, handle start, win, lose, level change.

Rules:
- Model never references View or Controller. Views don't change Model data; they read what the Controller passes in.
- View ↔ Controller communication via events/interfaces, not by View calling Controller concrete classes.
- Shared infrastructure (game lifecycle, tweens) stays in `Core/`; mini-game code must not depend on another mini-game.
- Editor scene builders stay in `Editor/` and must not be referenced by runtime code.

## SOLID

- **S** – One reason to change per class. Split rules / presentation / flow / data loading into separate classes. Avoid "God" MonoBehaviours.
- **O** – Add behaviour by new classes/implementations (new level data, new mini-game via `Game` base) rather than editing working code with more `if/switch` branches.
- **L** – Subclasses and interface implementations must be usable wherever the base is expected, with no surprise exceptions or ignored contract.
- **I** – Prefer small, focused interfaces (e.g. `IInputSource`, `IView`) over one large one. No empty/throwing method stubs.
- **D** – Depend on abstractions at boundaries. Pass dependencies in (constructor, `Init(...)`, or `[SerializeField]`) instead of `FindObjectOfType`, static singletons, or `GameObject.Find`.

## Practical conventions

- Prefer `[SerializeField] private` over `public` fields.
- Prefer composition over inheritance; inherit only for a real is-a (e.g. `Game` base).
- Keep `Update()` thin; delegate to Controller/Model methods.
- Put tunable values in `LevelData`, not magic numbers in code.
- If a request conflicts with these rules, flag it and propose a compliant alternative before implementing.
- Animations use DOTween (`using DG.Tweening;`), only inside Views. Always `SetLink(gameObject)`, keep a reference to long-running Sequences and `Kill()` them in `ResetView()`. Views still expose `IEnumerator Play...()` (via `WaitForCompletion()`) so Controllers can `yield return` them.
- UI must stay adaptive (WebGL on PC and mobile): anchors instead of fixed pixel positions, `AdaptiveGrid` for lists/grids, `SafeAreaFitter` for edge-anchored content, text with best-fit; test in both landscape and portrait.
