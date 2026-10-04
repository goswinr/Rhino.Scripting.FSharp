# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Changed
- Target net8.0 instead of net7.0, the same as Rhino.Scripting 0.14 (the net7.0 build was compiled against the net48 build of Rhino.Scripting)
- The tolerance argument of `v.IsParallelTo` and `v.IsPerpendicularTo` on Vector3d is required now, because without it RhinoCommon's own members are called
- Use LangVersion latest instead of preview
### Fixed
- `Plane.WorldTop` was an infinite loop
- `Line.extend` returned a bool instead of the extended Line
- `Line.closestParameter`, `ln.ClosestPoint`, `Line.closestPoint` and the `DistanceToPnt` functions measured to the infinite line instead of the finite line
- `Vector3d.areParallel` returned an int, `Vector3d.arePerpendicular` used a 1 degree tolerance instead of 0.25 degrees
- `Line.divideEvery` and `Line.divideInsideEvery` returned wrongly spaced and duplicate points
- `Line.divideMinLength` and `Line.splitMinLength` failed when the line length equals the minimum segment length
- `Line.intersectFinite` did not return the closest points of the two finite lines
- `rs.OffsetPoints` crashed or returned wrong end points for some open polylines
- `rs.FilletPolyline` failed on corner 0 of closed polylines
- `rs.MeanPoint` returned a NaN point for an empty input
- `RhPoints.cullDuplicatePointsInSeq` could remove the first point
- `RhPoints.findContinuousPoints` modified its input
- `Line.IsTiny` and `Line.isTiny` did not detect NaN
- `Vector3d.orientDown` reversed horizontal vectors
- `rs.MeshAddLoopWelded` created a duplicate face for 3 lines
- Exception messages started with "Rhino.Scripting.FSharp" twice
- Build failed on case-sensitive file systems (Linux, macOS)
- Many doc comments and README examples
### Removed
- Extension members that were never called because RhinoCommon or Rhino.Scripting members with the same name hide them: `ln.Length`, `ln.Direction`, `ln.UnitTangent`, `ln.ClosestParameter`, `ln.Extend(a,b)`, `v.IsZero`, `v.IsTiny`, `pt.DistanceTo`, `pl.ClosestPoint`, `Plane.WorldXY` and `rs.Print`

## [0.14.0] - 2026-04-10
### Changed
- Referencing [Rhino.Scripting 0.14.0](https://github.com/goswinr/Rhino.Scripting/blob/main/CHANGELOG.md#0140)


## [0.13.0] - 2026-01-17
### Changed
- Referencing [Rhino.Scripting 0.13.0](https://github.com/goswinr/Rhino.Scripting/blob/main/CHANGELOG.md#0130)
### Fixed
- Many typos in documentation(thank you Claude!)

## [0.12.1] - 2025-05-28
### Changed
- Referencing [Rhino.Scripting 0.12.0](https://github.com/goswinr/Rhino.Scripting/blob/main/CHANGELOG.md#0120)

## [0.11.0] - 2025-05-25
### Changed
- Referencing Rhino.Scripting 0.11.0
### Added
- build for .NET 7 too
### Fixed
- typos in documentation

## [0.10.2] - 2025-03-25
### Fixed
- nicer Error Messages when remembering objects

## [0.10.1] - 2025-03-15
### Changed
- Referencing Rhino.Scripting 0.10.1

## [0.10.0] - 2025-03-07
### Changed
- Referencing Rhino.Scripting 0.10.0
- removed FsEx dependency
- rename .ToNiceString to .Pretty

## [0.8.2] - 2025-02-24
### Changed
- Referencing Rhino.Scripting 0.8.0
- rename Rhino.Scripting.Fsharp -> Rhino.Scripting.FSharp (capital S)

## [0.8.1] - 2024-10-06
### Changed
- Align Plane API with Euclid library

## [0.8.0] - 2024-10-06
### Changed
- Align Line, Point3d and Vector3d API with Euclid library
- Referencing Rhino.Scripting 0.8.0

## [0.5.0] - 2023-02-20
### Added
- First public release
- Referencing Rhino.Scripting 0.5.0

[Unreleased]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.14.0...HEAD
[0.14.0]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.13.0...0.14.0
[0.13.0]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.12.1...0.13.0
[0.12.1]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.11.0...0.12.1
[0.11.0]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.10.2...0.11.0
[0.10.2]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.10.1...0.10.2
[0.10.1]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.10.0...0.10.1
[0.10.0]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.8.2...0.10.0
[0.8.2]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.8.1...0.8.2
[0.8.1]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.8.0...0.8.1
[0.8.0]: https://github.com/goswinr/Rhino.Scripting.FSharp/compare/0.5.0...0.8.0
[0.5.0]: https://github.com/goswinr/Rhino.Scripting.FSharp/releases/tag/0.5.0

