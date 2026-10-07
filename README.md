# PickYourSpirit

[![CI](https://github.com/algo7/PickYourSpirit/actions/workflows/ci.yml/badge.svg)](https://github.com/algo7/PickYourSpirit/actions/workflows/ci.yml)
[![Thunderstore](https://img.shields.io/badge/Thunderstore-PickYourSpirit-blue)](https://thunderstore.io/c/valheim/p/Algo7/PickYourSpirit/)

A client-side [BepInEx](https://github.com/BepInEx/BepInEx) mod for Valheim: choose which spirit the Spirit Caller
summons next. Players without the mod only ever see vanilla things.

What it does for players is in [package/README.md](package/README.md), which is also the mod's Thunderstore page.
Changes: [CHANGELOG.md](CHANGELOG.md).

Made with AI assistance.

## How it works

The staff's attack fires `staff_SpiritCaller_spawn`, an object with only a `SpawnAbility` and no `ZNetView`: it exists
only in the caster's game, and only the caster's game runs the attack (`Humanoid.OnAttackTrigger` checks ownership).
`SpawnAbility.Spawn` rolls `m_spawnPrefab[Random.Range(...)]` over the four spirits and creates the winner, a vanilla
creature everyone receives as usual. So choosing the spirit is a change in the caster's game alone.

Three Harmony patches:

- `SpawnAbility.Setup` prefix: for the local player's Spirit Caller cast with a pick, the cast's own `m_spawnPrefab`
  becomes a one-entry array with the picked spirit before the roll. The staff's list is never written to.
- `Player.SetControls` postfix: reads the secondary-attack press after vanilla's own rules (inventory, chat, radial
  menu, sitting, emoting, steering). With the Spirit Caller in hand it moves to the next pick (Random → the staff's
  spirits in its own order → Random) and shows a centre message. The staff has no secondary attack, so vanilla does
  nothing with the press. The game's own spirit names (`$spiritcaller_*`) are empty in every language, so the message
  names the animal each spirit is made from (`Wolf_spiritcaller` → `Wolf` → `$enemy_wolf`).
- `KeyHints.UpdateHints` postfix: shows the secondary-attack hint while the staff is out and labels it "Next spirit",
  the same way the game relabels its hints; puts the game's label back afterwards.

The pick lives in memory only and goes back to Random on every scene load. The game's cap per kind (staff level, oldest
unsummoned) and the casting cost are untouched.

## Building

Needs the .NET SDK (8+) and the game's DLLs (a local Valheim install, or the dedicated server's).

```sh
make build      # bin/Release/PickYourSpirit.dll
make test       # unit tests: the pick rules, and the game members the patches use
make package    # Algo7-PickYourSpirit-<version>.zip for r2modman's "Import local mod"
```

The version comes from the git tag (MinVer).

## CI / CD

CI builds and tests against the free dedicated server's DLLs (`.github/scripts/valheim-managed.sh`). A `vX.Y.Z` tag
with a matching `## X.Y.Z` section in `CHANGELOG.md` releases to GitHub and Thunderstore after manual approval.

## Layout

| Path | What |
|---|---|
| `src/Plugin.cs` | Entry point, patching, scene-load reset |
| `src/SpiritRules.cs` | The pick rules (Unity-free, unit tested) |
| `src/SpiritCaller.cs` | The staff, its spirit list, the pick, the message |
| `src/CastPatches.cs`, `src/ButtonPatches.cs`, `src/HintPatches.cs` | The three patches |
| `tests/` | Unit tests (`make test`) |
| `package/` | Thunderstore page and icon |

## License

MIT
