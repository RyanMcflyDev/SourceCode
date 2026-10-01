# Source Code: Claude Code instructions

Final year solo Unity game, BA Games Design and Immersive Technologies,
University of Winchester. Owner: Ryan McFly.

## Read first, every session

The vault is the source of truth, not this file and not conversation memory.

1. `E:\ClaudeVault\_conventions.md` and follow its read order.
2. `E:\ClaudeVault\Projects\source-code\state.md`, `open.md`, `backlog.md`.
3. `decisions.md` when a design question comes up.

If the vault contradicts this file, the vault wins.

## Current design (overrides the GDD where they conflict)

- Angled top-down camera (refs: Tunic, Lara Croft GO, Timelie, CrossCode).
  Orthographic and narrow-FOV perspective, switchable in the Inspector.
- 3D scene, gameplay on the floor plane, free movement, grid-snapped modular
  levels, 2 to 3 height levels per room max.
- Height via stairs, ramps or Swap. Swapping with something higher lifts Byte.
- Fixed-distance hop over gaps up to 2 tiles, never climbs a level. On
  probation: lives in its own removable component.
- Swap, Destabilise, Clock Throttle, Code Vision, one-hit death as in the GDD.

## Architecture rules

- Swap knows only transforms, a target class and a registry. Depends on nothing.
- Cross-system messages through an event bus. Nothing references the player
  directly.
- All tuning in ScriptableObjects.
- Rooms reset themselves; no scene reloads on death.
- Assembly definitions keep core, gameplay and editor code separate.
- Controller: Unity's CharacterController behind a motor interface. Kinematic
  Character Controller (owned) is the fallback.
- CharacterController overwrites teleported positions unless disabled for the
  move or physics transforms are synced. Swap must handle this.
- Do not use the Game Creator 2 suite.
- Plugins in use: Input System, Cinemachine 3 (API differs from 2.x, check docs
  before writing against it), DOTween, NaughtyAttributes, Hot Reload,
  vHierarchy 2, Graphy.
- Unity version is locked. Never upgrade it or suggest upgrading.

## Before handing over code

Check it compiles against this project's Unity version, matches the existing
conventions, and works with the scene as it actually is. Write for extension:
independent components, minimal coupling. If a design is over or under
engineered for a solo project, say so and offer the alternative.

## Ask first

- Any git commit or push. Draft the message, show it, wait.
- Deleting anything.
- Unity editor writes through MCP (creating or modifying GameObjects, scenes,
  prefabs, entering play mode).

## Evidence for the write-up

- One commit per working step. The message says what now works, in plain
  words ("Byte can walk up stairs and hop 2-tile gaps"), not what files changed.
- Record facts and numbers only. Ryan writes all reflection and judgement.
  Never write anything that could be presented as his reflection.
- At the end of a session, run the vault write-back for
  `Projects/source-code/` and tell Ryan what was written.

## Style

No em dashes. Casual and direct. If Ryan does not follow an explanation, walk
through it step by step.
