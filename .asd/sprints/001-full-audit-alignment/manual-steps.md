# Manual Steps

Manual actions required by sprint `001-full-audit-alignment` that cannot be executed autonomously.

---

## MS-1: Verify `jb` ReSharper CLI cleanup/inspect flow on the migrated `.slnx`

**Blocking subtask:** T3 — "Verify the `jb` (ReSharper CLI) cleanup/inspect flow runs against the migrated project"

**Why human-only:** The `jb` CLI tool (JetBrains ReSharper CLI) must be run interactively on this machine; the agent cannot invoke GUI/licensed tooling, and the output `.sarif` file must be inspected for zero error/warning severity entries. This is a verification step, not a code change.

**Steps:**

1. Open a terminal at the repo root `D:\Storage\Projects\RimWorld\rimworld-outfitmanager`.
2. Run the build first so binaries exist:
   ```
   dotnet build Source/OutfitManager.slnx -c Release
   ```
3. Run JetBrains cleanup:
   ```
   jb cleanupcode Source\OutfitManager.slnx --toolset-path="C:\Program Files\dotnet\sdk\10.0.300\MSBuild.dll"
   ```
4. Run JetBrains inspect:
   ```
   jb inspectcode Source\OutfitManager.slnx -o=".\TestResults\jb-inspect.sarif" --no-build --toolset-path="C:\Program Files\dotnet\sdk\10.0.300\MSBuild.dll"
   ```
5. Open `TestResults\jb-inspect.sarif` and confirm there are no entries with `"level": "error"` or `"level": "warning"`.

**Verification:** Zero `error`/`warning` severity entries in `TestResults\jb-inspect.sarif`. If any findings appear, fix the code and re-run until clean.

---

## MS-2: Confirm transpiler patch on `ApparelScoreRaw` applies in-game after Harmony 2.4.2 bump

**Blocking subtask:** T3 — "Confirm the transpiler patch on `ApparelScoreRaw` still applies and emits its visible patch-applied startup signal after the dependency bump"

**Why human-only:** In-game verification requires loading RimWorld with the mod active and observing the startup log. This cannot be done by the agent (requires a live game process).

**Steps:**

1. Build the mod in Release:
   ```
   dotnet build Source/OutfitManager.slnx -c Release
   ```
2. Copy (or confirm symlink) `1.6/Assemblies/LordKuper.OutfitManager.dll` is in place for your mod load path.
3. Launch RimWorld with OutfitManager enabled.
4. Open the RimWorld log (Dev mode → Open Log File, or `Player.log`).
5. Search for the string `"Work-type apparel scoring patch APPLIED"` — this is emitted by `JobGiverPatch.Transpiler` when the IL pattern is found and the patch succeeds.
6. Confirm the string is present. If absent, check for `"Work-type apparel scoring patch FAILED"` which indicates fail-soft triggered (pattern not found).

**Verification:** Log contains `"Work-type apparel scoring patch APPLIED"` (success case) or `"Work-type apparel scoring patch FAILED"` (fail-soft case, still acceptable per AC-27). Game must not crash; vanilla apparel selection will be used if patch fails.

---

## MS-3: Unit test scenarios verification in-game context

**Blocking subtask:** T8 — "Initial test coverage including transpiler fail-soft characterization"

**Why human-only:** The unit test classes document expected behaviour but require live RimWorld game context to construct game objects (Pawn, Apparel, Pawn_WorkSettings). The following 16 tests are marked `[Ignore("... RimWorld context")]` in:
- `WorkTypeHelperTests.cs` (6 tests) — work-type weight normalization
- `ApparelCacheTests.cs` (4 tests) — weighted-sum scoring
- `ApparelScoringTests.cs` (3 tests + 1 MS-2) — factor multiplication and characterization (C-3, C-4)

**Steps:**

1. **Manual code review (Pre-T6):**
   - Open `Source/OutfitManager.Tests/WorkTypeHelperTests.cs`.
   - For each `[Ignore]` test, read the documentation comment.
   - Verify the production code (`WorkTypeHelper.GetNormalizedWorkTypeWeights`) matches the documented expected behaviour.
   - Repeat for `ApparelCacheTests.cs` and `ApparelScoringTests.cs`.

2. **Characterization test verification (Pre-T6 and Post-T6):**
   - Review tests marked "CHARACTERIZATION" (C-3, C-4):
     - `ApparelCache_FirstOrDefaultLookupPaths_AreFunctionallyEquivalent` — documents C-3 duplication
     - `ApparelScoring_InitializeFlagIsSetImmediately` — documents C-4 flag ordering
   - Verify the current code follows these patterns.
   - After T6 (simplification), re-run `dotnet test Source/OutfitManager.slnx` and confirm all tests still [Ignore] (no failures).

3. **In-game endurance test (Post-T6):**
   - Create a pawn with varied work priorities (Hauling, Crafting, Cooking, etc.).
   - Equip different apparel pieces on the same pawn.
   - Verify apparel selection behaves the same before and after T6 (same outfit chosen for given priorities/settings).
   - This manual verification is part of T10 end-to-end save/behaviour stability check.

**Verification:** 
- All 16 ignored tests pass (no failures) in both Pre-T6 and Post-T6 test runs.
- Manual code review documents expected behaviour per test.
- In-game apparel-pick outcomes are unchanged after T6 simplifications (for AC-20 behaviour-neutrality proof).
