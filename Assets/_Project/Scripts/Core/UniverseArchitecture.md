# Fine-Tuned Universe - Runtime Architecture

## Deterministic Contract
Every sample in the simulation is a pure function of:

- `seed` (`SeedManager`)
- tuned constants (`ConstantsManager`)
- authoritative simulation time (`SimulationManager`)

No runtime calls to `Random` are used in simulation systems.

## Core Runtime
- `SimulationManager`: owns simulation time, game mode, and deterministic probe spawn.
- `SeedManager`: normalizes seed to 6-char alphanumeric and maps to deterministic integer.
- `UniverseFieldSampler`: procedural field model for density/radiation/entropy/life and planet archetype.

## Modes
- `ObservatoryController`: scale traversal camera for macro-to-micro observation.
- `ProbeController` + `ProbeCamera`: inertial exploration with drift/orbit-assist/anchor.

## Probe Systems
- `ScanSystem`: deterministic spatial scan snapshots.
- `SignalSystem`: deterministic signal emitters and frequency tuning.
- `LocalInfluenceSystem`: limited local pulses (decaying and spatially bounded).
- `FeedbackSystem`: state-driven visual/audio feedback.

## Meta Systems
- `WhisperSystem`: first-person, state-reactive narration lines.
- `CodexManager`: deterministic discovery signatures from scans.
- `PersistenceManager`: save/load seed, constants, and run metadata.
- `UniverseHudController`: minimal constants + probe active UI.
