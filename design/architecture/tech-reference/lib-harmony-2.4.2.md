---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# Lib.Harmony @ 2.4.2

## Canonical source
- Official docs: https://harmony.pardeike.net/
- Last verified: 2026-06-07
- 2.4.2 confirmed latest stable (published 2025-11-13).

## Reference nature (compile-only)
- **Target:** referenced as a NuGet `PackageReference Include="Lib.Harmony" Version="2.4.2"` with `PrivateAssets=all`, `ExcludeAssets=runtime`, `IncludeAssets=compile; build; native; contentfiles; analyzers; buildtransitive`.
- **Current legacy state:** OM references Harmony via a raw HintPath to `packages\Lib.Harmony.2.3.6\lib\net48\0Harmony.dll` (version **2.3.6**, `Private=False`) wired through `packages.config`. The target bumps to 2.4.2 and moves to `PackageReference` as part of the legacy→SDK migration.
- **Compile-time only.** The runtime Harmony assembly is provided by the `brrainz.harmony` mod (declared in `About/About.xml` as a `modDependency` + `loadAfter`). The mod must NOT ship its own Harmony DLL — doing so would conflict with the shared runtime instance every RimWorld mod uses.

## API surface used in project
- `HarmonyLib.Harmony` (ctor + `PatchAll`): `new Harmony(OutfitManagerMod.ModId)` then `harmony.PatchAll(Assembly.GetExecutingAssembly())` in the `Mod` ctor. Single id, single PatchAll call. (`OutfitManagerMod.cs`)
- `[HarmonyPatch]` + `[HarmonyTranspiler]`: OM has exactly **one** patch — `Patches/JobGiverPatch.cs` declares `[HarmonyPatch(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreRaw))]` with a private static `Transpiler`. There are no postfix/prefix patches.
- `HarmonyLib.CodeInstruction` / `System.Reflection.Emit.OpCodes`: the transpiler walks the original IL of `ApparelScoreRaw`, scanning for the anchor sequence `Ldloc_0; Ldarg_1; Callvirt Apparel.GetSpecialApparelScoreOffset; Add; Stloc_0`, and inserts a call to `ApparelScoring.GetPawnApparelWorkScore(pawn, apparel)` whose result is added into the running score local.
- `HarmonyLib.AccessTools`:
  - `AccessTools.Method(typeof(Apparel), nameof(Apparel.GetSpecialApparelScoreOffset))` — resolve the anchor method used to locate the IL insertion point.
  - `CodeInstruction.Call(typeof(ApparelScoring), nameof(ApparelScoring.GetPawnApparelWorkScore))` — build the injected call instruction.
- No reflective optional-mod binding (`TypeByName`, `MethodDelegate`, `FieldRef`) — OM has no optional integrations.

## Version-specific notes
- 2.4.x is the current major line; `Lib.Harmony` package merges dependencies into a self-contained assembly (relevant only at compile; runtime comes from brrainz.harmony).
- Targets net48 / Mono — compatible with RimWorld's runtime.
- Moving from 2.3.6 to 2.4.2: the APIs OM uses (`Harmony`, `PatchAll`, `[HarmonyTranspiler]`, `CodeInstruction`, `AccessTools.Method`, `CodeInstruction.Call`) are stable across the 2.x line; no signature changes affect OM's single transpiler.

## Deprecations and breaking changes from prior version
- No project-affecting breaks observed within the 2.x line for the APIs used here. Keep the compile-time package version aligned with the Harmony runtime shipped by brrainz.harmony to avoid signature drift.

## Project conventions
- One Harmony instance, id = `OutfitManagerMod.ModId`; one `PatchAll(Assembly.GetExecutingAssembly())` at boot.
- The single patch class lives under `Patches/`, is `internal static`, and carries `[UsedImplicitly]` (on the class and the `Transpiler` method) so analyzers don't flag them.
- **Transpiler is fail-soft.** If the IL anchor is not found, the transpiler logs `"Could not apply JobGiver patch."` via the local `Logger` and returns the original instruction list unchanged — vanilla apparel scoring keeps working rather than crashing on an unexpected game build.

## Known issues and workarounds
- Shipping a runtime Harmony copy breaks the shared instance → `ExcludeAssets=runtime` enforces compile-only; runtime stays with brrainz.harmony.
- The transpiler depends on a specific IL shape in `JobGiver_OptimizeApparel.ApparelScoreRaw`; a RimWorld update that re-orders that method can move/remove the anchor. The fail-soft path keeps the game playable, but the OM work-type score will not be applied until the transpiler is re-anchored — verify the patch applies (look for the `"Applying JobGiver patch."` log) after any game update.
