# AGENTS.md

Guidance for AI coding agents working in this repository.

## Project

F# library that extends [Rhino.Scripting](https://github.com/goswinr/Rhino.Scripting) with RhinoCommon type extensions (`Vector3d`, `Point3d`, `Line`, `Plane`, …) and curried `RhinoScriptSyntax` functions for pipeline-style scripting of Rhino3D. Published to NuGet as `Rhino.Scripting.FSharp`.

- Targets `net48` (Rhino 7) and `net8.0` (Rhino 8), the same targets as Rhino.Scripting.
- There is no test project and the code only runs inside Rhino. `dotnet build` and the warnings it reports are the verification step.

## Commands

```powershell
dotnet build              # all three targets; also packs the .nupkg (GeneratePackageOnBuild)
dotnet build -f net48     # a single target, for faster iteration
```

API docs (fsdocs), as run by `.github/workflows/docs.yml`:

```powershell
dotnet tool restore
dotnet build --configuration Release
dotnet fsdocs build --clean --strict --properties Configuration=Release --input Docs --output DocsGenerated
```

The build copies `README.md` to `Docs/index.md` (gitignored), so the README is the docs home page. `--strict` makes missing or broken XML docs fail the docs build.

## Versioning and release

- The package version is read from `CHANGELOG.md` by `Ionide.KeepAChangelog.Tasks`; the fsproj has no `<Version>`. Add entries under `## [Unreleased]`.
- The changelog parser crashes on wrapped or nested bullets, so every `- ` item must be a single line. It also rejects mixed line endings (IKC0002). Run `dotnet build` after editing the changelog.
- To release: rename `[Unreleased]` to `## [0.X.Y] - YYYY-MM-DD`, update the compare links at the bottom of the changelog, commit and push to `main`, then `git tag 0.X.Y && git push origin 0.X.Y`. `releaseNuget.yml` fails if the tag differs from the changelog version (written to `bin/ChangelogVersion.txt`), otherwise it publishes to nuget.org.
- `checkLinks.yml` checks every link in `README.md` and `CHANGELOG.md` on each tag and weekly.

## Dependencies

- `FSharp.Core` is pinned to 6.0.7 and `RhinoCommon` to 7.0.x (net48) and 8.19.x (net8.0), matching Rhino.Scripting. Do not bump them; dependabot ignores both.
- `RhinoCommon` is compile-time only (`PrivateAssets`, `ExcludeAssets="runtime"`) because Rhino supplies it at runtime.
- Much of the geometry code is ported from [Euclid](https://github.com/goswinr/Euclid) and marked `// copied from Euclid 0.16`.

## Architecture

Everything is in the namespace `Rhino.Scripting.FSharp`. F# compiles files in the order of the `<Compile>` items in `Rhino.Scripting.Fsharp.fsproj`, and a file can only use what is defined above it. Insert new files at the right position in that list.

1. `Src/UtilRhinoScriptingFSharp.fs`: `RhinoScriptingFSharpException`, the internal `OPT`/`DEF` attribute aliases, tolerances and the `Cosine` unit-of-measure module.
2. `Src/Vector3d.fs`, `Point3d.fs`, `Line.fs`, `Plane.fs`: extensions on RhinoCommon structs (including `Vector3f` and `Point3f`).
3. `Src/Rhino.Scripting/*.fs`: extensions on `RhinoScriptSyntax`. `Curried.fs` defines the `|>!` operator.
4. `Src/RhPoints.fs`, `Src/RhTopology.fs`: algorithm modules marked `[<RequireQualifiedAccess>]`.

Files 1–3 put their extensions (`type X with ...`) in `[<AutoOpen>] module AutoOpen<Name>` blocks, so `open Rhino.Scripting.FSharp` brings them all into scope. Scripts usually alias `type rs = RhinoScriptSyntax` and call wrappers as `rs.setLayer "name" id`.

## Conventions and pitfalls

- **Never shadow an intrinsic member.** F# always picks a RhinoCommon or Rhino.Scripting member over an extension with the same name and signature, even if the extension is opened later. Such an extension is silently never called: for example, `ln.Extend(a, b)` resolves to RhinoCommon's mutating `Line.Extend : bool`. Check the target type first, then use a different name, add a distinguishing required argument (such as a `float<Cosine.cosine>` tolerance), or write a static member such as `Line.extend`.
- **Curried wrappers take the object id last** so they work in pipelines:
  ```fsharp
  static member setLayer (layer:string) (objectId:Guid) : unit =
      RhinoScriptSyntax.ObjectLayer(objectId, layer, createLayerIfMissing=true)
  ```
- **Naming.** Curried `RhinoScriptSyntax` wrappers start with a verb and are camelCase (`setLayer`, `getName`, `tryGetName`, `matchLayer`). Static geometry functions are also camelCase (`Line.divide`, `Point3d.withZ`, `createFromMembersXYZ`). Instance members are PascalCase (`LengthSq`, `IsXAligned`), and modifiers use `With*` (`WithX`, `WithLength`).
- **Errors.** Use `RhinoScriptingFSharpException.Raise` with a printf-style format and include the failing values: `RhinoScriptingFSharpException.Raise "Line.unitTangent: x:%g, y:%g and z:%g are too small" v.X v.Y v.Z`. It adds the `Rhino.Scripting.FSharp.` prefix itself, so don't put it in the message.
- **Inline members keep their error path out of line.** Performance-critical members are `inline`. Their error message is built in a separate public member marked `[<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]`, which keeps the inlined code small (see `Point3d.fs`).
- **Optional parameters** use the internal aliases: `[<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine>`.
- **Tolerances** (in module `UtilRHinoScriptingFSharp`): `zeroLengthTolerance = 1e-12` for divisions and unitizing. `isTooSmall` (1e-6) and `isTooTiny` (1e-12) are written as `not (x > threshold)` so that NaN also counts as too small. Keep that idiom in new checks.
- **XML docs (`///`) are required on every public member**, enforced by `--warnon:3390` and `FsDocsWarnOnMissingDocs`. `RhinoScriptSyntax` wrappers use full `<summary>`/`<param>`/`<returns>` tags and give the type in parentheses: `<param name="objectId">(Guid) The identifier of the object</param>`. Annotate return types on member signatures explicitly.
- `--warnon:1182` reports unused variables.
- **Threading.** Rhino.Scripting marshals UI calls to the Rhino UI thread, so these functions can be called from a background thread. Modifying the document from one background thread at a time is fine.
