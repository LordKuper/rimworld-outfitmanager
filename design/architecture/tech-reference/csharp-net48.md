---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# C# (`LangVersion latest`) @ net48

## Canonical source
- Official docs: https://learn.microsoft.com/dotnet/csharp/
- Last verified: 2026-06-07 (reconciled to as-built post-migration)

## API surface used in project
- `<LangVersion>latest</LangVersion>`: **as-built** — set on both `Source/OutfitManager/OutfitManager.csproj` and `Source/OutfitManager.Tests/OutfitManager.Tests.csproj`. Every project compiles with the newest C# language version the installed Roslyn/SDK supports, decoupled from the runtime target. The source relies on modern syntax (file-scoped namespaces, collection expressions `[]`, target-typed `new()`), so `latest` is required.
- `<TargetFramework>net48</TargetFramework>`: **as-built** — SDK-style `net48` on both projects (the pre-sprint-001 legacy csproj used `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>`). .NET Framework 4.8 — RimWorld's Mono runtime target. Identical in production and test projects.
- C# nullable reference types (`<Nullable>enable</Nullable>`): **as-built — enabled project-wide** (both `Source/OutfitManager/OutfitManager.csproj` and `Source/OutfitManager.Tests/OutfitManager.Tests.csproj`) as of the sprint-001 migration (2026-06-07). The former JetBrains nullability attribute (`[NotNull]` in `ApparelCache.cs`) was removed in favour of `?` reference-type syntax and compiler flow analysis. The nullable context is never disabled anywhere.
- JetBrains annotations: the nullability attribute (`[NotNull]`) has been removed. `[UsedImplicitly]` (non-nullability) is retained where used (`OutfitManagerMod`, `JobGiverPatch`, `Settings`). `using JetBrains.Annotations;` is retained only in files that still reference a non-nullability attribute. (Pre-sprint-001 the legacy build carried a single `[NotNull]` on the `ApparelCache` ctor param; that file no longer imports `JetBrains.Annotations`.)
- `InternalsVisibleTo`: **as-built** — the production project exposes internals to `LordKuper.OutfitManager.Tests` (`<InternalsVisibleTo Include="LordKuper.OutfitManager.Tests" />` in `OutfitManager.csproj`); several score/cache members are `internal`/`private`.

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
- No prior in-project C# version to migrate from; net48 + `latest` is the established baseline now that the csproj is SDK-style.
- **Completed (sprint-001 migration, 2026-06-07):** `<Nullable>enable</Nullable>` set on the production project (and tests); all NRT sites resolved with real annotations/guards; the lone JetBrains `[NotNull]` removed. The build is green under `TreatWarningsAsErrors`.

## Project conventions
- **`<Nullable>enable</Nullable>` on all projects** (production and test) — as-built. Prefer C# nullable reference types over JetBrains annotations; the two MUST NOT contradict. Never disable the nullable context anywhere.
- `[UsedImplicitly]` (and other non-nullability JetBrains attributes) may still appear where needed; `using JetBrains.Annotations;` is retained only in those files.
- Zero-warning policy: both projects build with `TreatWarningsAsErrors=true` and `WarningLevel 9999` (Debug and Release). Any warning — including analyzer findings — fails the build, so code MUST compile warning-clean.
- Mod identity: reference `OutfitManagerMod.ModId` rather than a bare `"LordKuper.OutfitManager"` literal (the existing `Logger` wrapper and `OutfitManagerMod` ctor already follow this).

## Known issues and workarounds
- Using a newer-C# feature that depends on a missing BCL type fails to compile on net48 with a missing-type error. Workaround: add the minimal polyfill attribute/type in-project, or avoid the feature. Prefer avoidance unless the polyfill is clearly justified.
- `<LangVersion>latest</LangVersion>` is pinned explicitly on both projects so the language version is not silently downgraded on an older toolchain. (Pre-sprint-001 the legacy non-SDK csproj used `<LangVersion>default</LangVersion>` and compiled modern syntax only because the installed MSBuild/Roslyn defaults were recent — that reliance has been removed.)
