---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# UnityEngine modules (Core / IMGUI / TextRendering) @ game-bound (RimWorld's bundled Unity)

## Canonical source
- Official docs: https://docs.unity3d.com/ScriptReference/ (Unity Scripting API)
- Last verified: 2026-06-07
- Note: the exact Unity version is whatever RimWorld 1.6 ships; the assemblies are RimWorld's bundled build, not a NuGet/UPM package. Decompile the referenced DLLs in the RimWorld `Managed` directory to confirm the precise surface.

## Reference nature (game-bound, not a package)
- Three modules are referenced by `HintPath` from RimWorld's `Managed` directory (in OM's legacy csproj the path is `..\..\..\Games\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\`), all `Private=False`:
  - `UnityEngine.CoreModule.dll`
  - `UnityEngine.IMGUIModule.dll`
  - `UnityEngine.TextRenderingModule.dll`
- The legacy csproj also references a bare `UnityEngine` assembly (`<Reference Include="UnityEngine">`) in addition to the three modules; during migration this can be reduced to the three modules actually needed.
- **No package version**: these are the Unity runtime DLLs shipped inside RimWorld. They are loaded by the game; the mod compiles against them but never copies them to output.
- This API surface is broadly documented by Unity, but the *specific version* RimWorld bundles is not, so version-sensitive behavior must be checked against the actual DLLs.

## API surface used in project
- `UnityEngine.CoreModule`: core value types used by OM's settings UI — `Rect` (tab/field layout in `Settings`, `Settings_General`, `Settings_WorkTypes`), `Vector2` (`_scrollPosition`, `_workTypesThingBoxScrollPosition`), `Mathf` (`Mathf.Clamp` on `WorkTypeScoreFactor`), `Event` (`Event.current.type == EventType.Layout` to capture content height), `EventType`.
- `UnityEngine.IMGUIModule`: the immediate-mode GUI underpinning the mod settings window. OM draws its General and Work Types tabs through RimWorld's `Verse.Widgets`/`TabRecord` and through `LordKuper.Common.UI` widgets (`Tabs`, `Fields`, `WorkTypeThingRuleWidget`), all of which sit on top of IMGUI. The window is opened via `OutfitManagerMod.DoSettingsWindowContents`.
- `UnityEngine.TextRenderingModule`: font/text glyph rendering that backs label and tooltip drawing in the settings dialog.

OM rarely calls Unity types directly; it consumes them transitively through RimWorld's `Verse.Widgets` / `Verse.GUI` and through `LordKuper.Common.UI` widgets. The references exist because those RimWorld/Common UI types expose Unity types (`Rect`, `Vector2`) in their public signatures, so the compiler needs the modules on the reference path.

## Version-specific notes
- IMGUI is the only supported UI path in RimWorld; do not introduce UI Toolkit / UGUI assumptions.
- Immediate-mode means no retained widget tree: the settings window redraws every frame inside RimWorld's window pump; state lives in the static `Settings` fields, not in Unity objects. OM stores content heights only on `EventType.Layout` frames to keep scroll math stable.

## Deprecations and breaking changes from prior version
- Unity module split: older monolithic `UnityEngine.dll` is replaced by per-module assemblies (Core/IMGUI/TextRendering). The legacy csproj references both the monolith and the three modules; the target keeps only the three modules.

## Project conventions
- Reference only the three modules actually needed (Core, IMGUI, TextRendering); drop the bare `UnityEngine` reference during migration unless a build-time justification appears.
- All UI is drawn via RimWorld `Verse.Widgets`/`TabRecord` and `LordKuper.Common.UI` widgets; reach for raw `GUI`/`GUILayout` only when no helper exists.
- Layout uses `Rect`-based manual positioning (RimWorld idiom), not `GUILayout` auto-flow, for consistency with the rest of the mod.

## Known issues and workarounds
- Bundled Unity version is undocumented for RimWorld → confirm any version-sensitive API against the actual DLLs in the RimWorld `Managed` directory.
- IMGUI calls are only valid during the GUI phase → never invoke `GUI`/`Widgets` from background or save/load code paths.
- The HintPaths in the legacy csproj are machine-specific absolute-relative paths into a Steam install; the migration should parameterize the RimWorld managed dir (e.g. `RimWorldManagedDir` / `RIMWORLD_DIR`) rather than hardcoding the Steam path.
