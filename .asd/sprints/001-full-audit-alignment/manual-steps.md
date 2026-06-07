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
5. Search for the string `"Applying JobGiver patch."` — this is emitted by `JobGiverPatch.Transpiler` when the IL pattern is found and the patch succeeds.
6. Confirm the string is present and there is NO `"Could not apply JobGiver patch."` error in the log.

**Verification:** Log contains `"Applying JobGiver patch."` and does NOT contain `"Could not apply JobGiver patch."` after loading with Harmony 2.4.2 active.
