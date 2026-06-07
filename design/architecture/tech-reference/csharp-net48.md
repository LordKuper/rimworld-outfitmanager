---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# C# (`LangVersion latest`) @ net48

## Canonical source
- Official docs: https://learn.microsoft.com/dotnet/csharp/
- Last verified: 2026-06-07

## API surface used in project
- `<LangVersion>latest</LangVersion>` (target): every project compiles with the newest C# language version the installed Roslyn/SDK supports, decoupled from the runtime target. To be set in `Source/OutfitManager.csproj` and the future `Source/OutfitManager.Tests/OutfitManager.Tests.csproj`. The current legacy csproj uses `<LangVersion>default</LangVersion>`, yet the source already relies on modern syntax (file-scoped namespaces, collection expressions `[]`, target-typed `new()`), so `latest` is required to match as-built code.
- `<TargetFramework>net48</TargetFramework>` (target SDK-style) / `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` (current legacy): .NET Framework 4.8 — RimWorld's Mono runtime target. Identical in production and test projects.
- C# nullable reference types (`<Nullable>enable</Nullable>`): target state — enabled project-wide. The current legacy build does NOT enable nullable and still uses JetBrains nullability attributes (`[NotNull]` in `ApparelCache.cs`). Migration replaces these with `?` reference-type syntax and compiler flow analysis.
- JetBrains annotations: in the legacy build `[NotNull]` (nullability) and `[UsedImplicitly]` (non-nullability) both appear. Target: remove the nullability attributes, keep `[UsedImplicitly]` (used on `OutfitManagerMod`, `JobGiverPatch`, `Settings`, `ApparelCache` ctor param). `using JetBrains.Annotations;` retained only in files that still reference a non-nullability attribute.
- `InternalsVisibleTo` (target): production will expose internals to `OutfitManager.Tests` once the test project exists (several score/cache members are `internal`/`private`).

## Version-specific notes
- `net48` is the runtime ceiling: RimWorld runs on Unity's Mono, which targets .NET Framework 4.8. The project cannot move to a modern .NET (Core/5+) runtime — the assembly is loaded into the game process.
- `latest` LangVersion gives modern C# *syntax* (records, pattern matching, target-typed `new`, collection expressions) while the *runtime* and *BCL* stay at net48. Compiler-only features work; features that depend on newer BCL types or runtime support do not.
- Features needing newer BCL / runtime support are unavailable on net48 or require polyfills / shim types defined in-project:
  - `init`-only setters and `record` types need `System.Runtime.CompilerServices.IsExternalInit` — absent from the net48 BCL; must be polyfilled if used.
  - C# 11 `required` members need `RequiredMemberAttribute` / `CompilerFeatureRequiredAttribute` — not in net48 BCL.
  - `Index`/`Range` (`^1`, `1..3`) need `System.Index`/`System.Range` — not in net48 BCL.
  - Default-interface-method dispatch, static abstract interface members, `Span<T>`-backed runtime intrinsics, and ref-struct interfaces depend on runtime features net48 lacks.
- Nullable reference types are a compile-time feature only; they do not require runtime support, so they are usable on net48. They are gated by the `<Nullable>` MSBuild property / `#nullable` directives, not by the runtime.

## Deprecations and breaking changes from prior version
- No prior in-project C# version to migrate from; net48 + `latest` is the established baseline once the legacy csproj is modernized.
- **Pending change (legacy migration):** set `<Nullable>enable</Nullable>` on the production project (`OutfitManager.csproj`); resolve all NRT sites with real annotations/guards; remove JetBrains nullability attributes. Build green under `TreatWarningsAsErrors`.

## Project conventions
- **Target: `<Nullable>enable</Nullable>` on all projects** (production and test). Prefer C# nullable reference types over JetBrains annotations; the two MUST NOT contradict. Never disable the nullable context anywhere.
- `[UsedImplicitly]` (and other non-nullability JetBrains attributes) may still appear where needed; `using JetBrains.Annotations;` is retained only in those files.
- Zero-warning policy (target): both projects build with `TreatWarningsAsErrors=true` (Debug and Release). Any warning — including analyzer findings — fails the build, so code MUST compile warning-clean.
- Mod identity: reference `OutfitManagerMod.ModId` rather than a bare `"LordKuper.OutfitManager"` literal (the existing `Logger` wrapper and `OutfitManagerMod` ctor already follow this).

## Known issues and workarounds
- Using a newer-C# feature that depends on a missing BCL type fails to compile on net48 with a missing-type error. Workaround: add the minimal polyfill attribute/type in-project, or avoid the feature. Prefer avoidance unless the polyfill is clearly justified.
- The current legacy non-SDK csproj sets `<LangVersion>default</LangVersion>` but compiles modern syntax only because the installed MSBuild/Roslyn defaults are recent. Pin `latest` explicitly during migration so the language version is not silently downgraded on an older toolchain.
