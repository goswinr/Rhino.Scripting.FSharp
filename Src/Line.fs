namespace Rhino.Scripting.FSharp
open Rhino.Scripting
open Rhino
open Rhino.Geometry
open Rhino.Scripting.RhinoScriptingUtils
open UtilRHinoScriptingFSharp


/// When Rhino.Scripting.FSharp is opened this module will be auto-opened.
/// It only contains extension members for type Line.
[<AutoOpen>]
module AutoOpenLine =

  type Line with // copied from Euclid 0.16

    /// Returns the length of the line.
    member inline ln.Length =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        sqrt(x*x + y*y + z*z)

     /// Returns the square length of the line.
    member inline ln.LengthSq =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        x*x + y*y + z*z

    /// Format 3D line into string from X, Y and Z for start and end points.
    /// Using nice floating point number formatting .
    /// But without full type name as in v.ToString()
    member ln.AsString =
        sprintf "%s, %s, %s to %s, %s, %s"
            (PrettyFormat.float ln.FromX)
            (PrettyFormat.float ln.FromY)
            (PrettyFormat.float ln.FromZ)
            (PrettyFormat.float ln.ToX)
            (PrettyFormat.float ln.ToY)
            (PrettyFormat.float ln.ToZ)


    /// Same as ln.Vector or ln.Tangent.
    /// The returned vector has the same length as the Line.
    member inline ln.Direction =
        Vector3d(ln.ToX-ln.FromX, ln.ToY-ln.FromY, ln.ToZ-ln.FromZ)

    /// Same as ln.Tangent or ln.Direction.
    /// The returned vector has the same length as the Line.
    member inline ln.Vector =
        Vector3d(ln.ToX-ln.FromX, ln.ToY-ln.FromY, ln.ToZ-ln.FromZ)

    /// Same as ln.Vector or ln.Direction.
    /// The returned vector has the same length as the Line.
    member inline ln.Tangent =
        Vector3d(ln.ToX-ln.FromX, ln.ToY-ln.FromY, ln.ToZ-ln.FromZ)

    /// Returns a unit-vector of the line Direction.
    member inline ln.UnitTangent =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x * x  + y * y + z * z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.UnitTangent: x:%g, y:%g and z:%g are too small for creating a unit-vector. Tolerance:%g" x y z zeroLengthTolerance
        let s = 1.0 / l
        Vector3d(x*s, y*s, z*s)

    /// Checks if line is parallel to the world X axis. Ignoring orientation.
    /// The absolute deviation tolerance along Y and Z axis is 1e-9.
    /// Fails on lines shorter than 1e-6.
    member inline ln.IsXAligned =
        let x = abs (ln.ToX-ln.FromX)
        let y = abs (ln.ToY-ln.FromY)
        let z = abs (ln.ToZ-ln.FromZ)
        if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Line.IsXAligned cannot not check very short line. (tolerance 1e-6) %O" ln
        else y < 1e-9 && z < 1e-9

    /// Checks if 3D line is parallel to the world Y axis. Ignoring orientation.
    /// The absolute deviation tolerance along X and Z axis is 1e-9.
    /// Fails on lines shorter than 1e-6.
    member inline ln.IsYAligned =
        let x = abs (ln.ToX-ln.FromX)
        let y = abs (ln.ToY-ln.FromY)
        let z = abs (ln.ToZ-ln.FromZ)
        if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Line.IsYAligned cannot not check very short line. (tolerance 1e-6) %O" ln
        else x < 1e-9 && z < 1e-9

    /// Checks if 3D line is parallel to the world Z axis. Ignoring orientation.
    /// The absolute deviation tolerance along X and Y axis is 1e-9.
    /// Fails on lines shorter than 1e-6.
    /// Same as ln.IsVertical
    member inline ln.IsZAligned =
        let x = abs (ln.ToX-ln.FromX)
        let y = abs (ln.ToY-ln.FromY)
        let z = abs (ln.ToZ-ln.FromZ)
        if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Line.IsZAligned cannot not check very short line. (tolerance 1e-6) %O" ln
        else x < 1e-9 && y < 1e-9

    /// Checks if 3D line is parallel to the world Z axis. Ignoring orientation.
    /// The absolute deviation tolerance along X and Y axis is 1e-9.
    /// Fails on lines shorter than 1e-6.
    /// Same as ln.IsZAligned
    member inline ln.IsVertical =
        let x = abs (ln.ToX-ln.FromX)
        let y = abs (ln.ToY-ln.FromY)
        let z = abs (ln.ToZ-ln.FromZ)
        if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Line.IsVertical cannot not check very short line. (tolerance 1e-6) %O" ln
        else x < 1e-9 && y < 1e-9

    /// Checks if 3D line is horizontal.
    /// The absolute deviation tolerance along Z axis is 1e-9.
    /// Fails on lines shorter than 1e-6.
    member inline ln.IsHorizontal =
        let x = abs (ln.ToX-ln.FromX)
        let y = abs (ln.ToY-ln.FromY)
        let z = abs (ln.ToZ-ln.FromZ)
        if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Line.IsHorizontal cannot not check very short line. (tolerance 1e-6) %O" ln
        else z < 1e-9

    /// Check if the 3D line has exactly the same starting and ending point.
    member inline ln.IsZeroLength =
        ln.ToX = ln.FromX &&
        ln.ToY = ln.FromY &&
        ln.ToZ = ln.FromZ

    /// Check if 3D line is shorter than tolerance.
    ///  Or contains a NaN value
    member inline ln.IsTiny tol =
        ln.Length < tol

    /// Check if 3D line is shorter than the squared tolerance.
    ///  Or contains a NaN value
    member inline ln.IsTinySq tol =
        not (ln.LengthSq > tol)

    /// Evaluate 3D line at a given parameter.
    /// Parameters 0.0 to 1.0 are on the line.
    member inline ln.EvaluateAt (p:float) =
        Point3d(ln.FromX + (ln.ToX-ln.FromX)*p,
            ln.FromY + (ln.ToY-ln.FromY)*p,
            ln.FromZ + (ln.ToZ-ln.FromZ)*p)

    /// Evaluate line at a given parameters (parameters 0.0 to 1.0 are on the line),
    /// Return a new line from evaluated points.
    member inline ln.SubLine (start:float, ende:float) =
        let fromX = ln.FromX
        let fromY = ln.FromY
        let fromZ = ln.FromZ
        let x = ln.ToX - fromX
        let y = ln.ToY - fromY
        let z = ln.ToZ - fromZ
        Line( fromX + x * start,
                fromY + y * start,
                fromZ + z * start,
                fromX + x * ende ,
                fromY + y * ende ,
                fromZ + z * ende )

    /// Returns the length of the line segment from the start point to the given parameter.
    /// This length is negative if the parameter is negative.
    member inline ln.LengthTillParam (p:float) =
        let x = (ln.ToX - ln.FromX)*p
        let y = (ln.ToY - ln.FromY)*p
        let z = (ln.ToZ - ln.FromZ)*p
        let l = sqrt(x*x + y*y + z*z)
        if p> 0.0 then l else -l

    /// Returns the length of the line segment from the given parameter till the line End.
    /// This length is negative if the parameter is bigger than 1.0.
    member inline ln.LengthFromParam (t:float) =
        let p = 1.0-t
        let x = (ln.ToX - ln.FromX)*p
        let y = (ln.ToY - ln.FromY)*p
        let z = (ln.ToZ - ln.FromZ)*p
        let l = sqrt(x*x + y*y + z*z)
        if p> 0.0 then l else -l

    /// Returns the midpoint of the 3D line,
    member inline ln.Mid =
        Point3d((ln.ToX + ln.FromX)*0.5,
            (ln.ToY + ln.FromY)*0.5,
            (ln.ToZ + ln.FromZ)*0.5)

    /// Returns the 3D line reversed.
    member inline ln.Reversed =
        Line(ln.ToX, ln.ToY, ln.ToZ, ln.FromX, ln.FromY, ln.FromZ)


    /// Returns a Line from point at Parameter a to point at Parameter b.
    member inline ln.Segment(a, b) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        Line( ln.FromX + x*a,
                ln.FromY + y*a,
                ln.FromZ + z*a,
                ln.FromX + x*b,
                ln.FromY + y*b,
                ln.FromZ + z*b)

    /// Extend 3D line by absolute amount at start and end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.Extend (distAtStart:float, distAtEnd:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.Extend %O to short for finding point at a distance." ln
        Line( ln.FromX - x*distAtStart/l,
                ln.FromY - y*distAtStart/l,
                ln.FromZ - z*distAtStart/l,
                ln.ToX   + x*distAtEnd/l,
                ln.ToY   + y*distAtEnd/l,
                ln.ToZ   + z*distAtEnd/l)

    /// Extend 3D line by absolute amount at start.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.ExtendStart (distAtStart:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.ExtendStart %O to short for finding point at a distance." ln
        Line( ln.FromX - x*distAtStart/l,
                ln.FromY - y*distAtStart/l,
                ln.FromZ - z*distAtStart/l,
                ln.ToX,
                ln.ToY,
                ln.ToZ)

    /// Extend 3D line by absolute amount at end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.ExtendEnd (distAtEnd:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.ExtendEnd %O to short for finding point at a distance." ln
        Line( ln.FromX,
                ln.FromY,
                ln.FromZ,
                ln.ToX   + x*distAtEnd/l,
                ln.ToY   + y*distAtEnd/l,
                ln.ToZ   + z*distAtEnd/l)

    /// Extend 3D line by relative amount at start and end.
    /// A relative amount of 0.5 extends the line by half its length on each side.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.ExtendRel (relAtStart:float, relAtEnd:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.ExtendRel %O to short for finding point at a distance." ln
        Line( ln.FromX - x*relAtStart,
                ln.FromY - y*relAtStart,
                ln.FromZ - z*relAtStart,
                ln.ToX   + x*relAtEnd,
                ln.ToY   + y*relAtEnd,
                ln.ToZ   + z*relAtEnd)

    /// Extend 3D line by relative amount at start.
    /// A relative amount of 0.5 extends the line by half its length.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.ExtendStartRel (relAtStart:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.ExtendStartRel %O to short for finding point at a distance." ln
        Line( ln.FromX - x*relAtStart,
                ln.FromY - y*relAtStart,
                ln.FromZ - z*relAtStart,
                ln.ToX,
                ln.ToY,
                ln.ToZ)

    /// Extend 3D line by relative amount at end.
    /// A relative amount of 0.5 extends the line by half its length.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.ExtendEndRel (relAtEnd:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.ExtendEndRel %O to short for finding point at a distance." ln
        Line( ln.FromX,
                ln.FromY,
                ln.FromZ,
                ln.ToX   + x*relAtEnd,
                ln.ToY   + y*relAtEnd,
                ln.ToZ   + z*relAtEnd)

    /// Shrink 3D line by absolute amount at start and end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.Shrink (distAtStart:float, distAtEnd:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.Shrink %O to short for finding point at a distance." ln
        Line( ln.FromX + x*distAtStart/l,
                ln.FromY + y*distAtStart/l,
                ln.FromZ + z*distAtStart/l,
                ln.ToX   - x*distAtEnd/l,
                ln.ToY   - y*distAtEnd/l,
                ln.ToZ   - z*distAtEnd/l)

    /// Shrink 3D line by absolute amount at start.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.ShrinkStart (distAtStart:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.ShrinkStart %O to short for finding point at a distance." ln
        Line( ln.FromX + x*distAtStart/l,
                ln.FromY + y*distAtStart/l,
                ln.FromZ + z*distAtStart/l,
                ln.ToX,
                ln.ToY,
                ln.ToZ)

    /// Shrink 3D line by absolute amount at end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.ShrinkEnd (distAtEnd:float) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.ShrinkEnd %O to short for finding point at a distance." ln
        Line( ln.FromX,
                ln.FromY,
                ln.FromZ,
                ln.ToX   - x*distAtEnd/l,
                ln.ToY   - y*distAtEnd/l,
                ln.ToZ   - z*distAtEnd/l)

    /// Returns a Line moved by a vector.
    member inline ln.Move (v:Vector3d) =
        Line( ln.FromX + v.X,
                ln.FromY + v.Y,
                ln.FromZ + v.Z,
                ln.ToX   + v.X,
                ln.ToY   + v.Y,
                ln.ToZ   + v.Z)

    /// Returns a Line moved by a given distance in X direction.
    member inline ln.MoveX (distance:float) =
        Line( ln.FromX + distance,
                ln.FromY,
                ln.FromZ,
                ln.ToX  + distance,
                ln.ToY,
                ln.ToZ)
    /// Returns a Line moved by a given distance in Y direction.
    member inline ln.MoveY (distance:float) =
        Line( ln.FromX,
                ln.FromY + distance,
                ln.FromZ,
                ln.ToX,
                ln.ToY + distance,
                ln.ToZ)

    /// Returns a Line moved by a given distance in Z direction.
    member inline ln.MoveZ (distance:float) =
        Line( ln.FromX,
                ln.FromY,
                ln.FromZ + distance,
                ln.ToX,
                ln.ToY,
                ln.ToZ + distance)

    /// Assumes Line to be infinite.
    /// Returns the parameter at which a point is closest to the infinite line.
    /// If it is smaller than 0.0 or bigger than 1.0 it is outside of the finite line.
    /// Fails on curves shorter than 1e-9 units. (ln.ClosestParameter does not)
    member inline ln.ClosestParameterInfinite (p:Point3d) =
        //http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
        let x = ln.FromX - ln.ToX
        let y = ln.FromY - ln.ToY
        let z = ln.FromZ - ln.ToZ
        let lenSq = x*x + y*y + z*z
        if isTooSmallSq(lenSq) then
            RhinoScriptingFSharpException.Raise "Line.ClosestParameterInfinite failed on very short line %O for point %O" ln p
        let u = ln.FromX-p.X
        let v = ln.FromY-p.Y
        let w = ln.FromZ-p.Z
        let dot = x*u + y*v + z*w
        dot / lenSq

    /// Returns the parameter at which a point is closest to the (finite) line.
    /// The result is between 0.0 and 1.0.
    /// Does not fails on very short curves.
    member inline ln.ClosestParameter (p:Point3d) =
        //http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
        let x = ln.FromX - ln.ToX
        let y = ln.FromY - ln.ToY
        let z = ln.FromZ - ln.ToZ
        let u = ln.FromX-p.X
        let v = ln.FromY-p.Y
        let w = ln.FromZ-p.Z
        let dot = x*u + y*v + z*w
        let lenSq = x*x + y*y + z*z
        if isTooSmallSq(lenSq) then
            if dot < 0.0 then 0.0 else 1.0
        else
            dot / lenSq |> clampBetweenZeroAndOne



    /// Assumes Line to be infinite.
    /// Returns closest point on infinite line.
    /// Fails on curves shorter than 1e-9 units. (ln.ClosestPoint does not.)
    member inline ln.ClosestPointInfinite (p:Point3d) =
        //http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
        let x = ln.FromX - ln.ToX
        let y = ln.FromY - ln.ToY
        let z = ln.FromZ - ln.ToZ
        let lenSq = x*x + y*y + z*z
        if isTooSmallSq(lenSq) then
            RhinoScriptingFSharpException.Raise "Line.ClosestPointInfinite failed on very short line %O for point %O" ln p
        let u = ln.FromX-p.X
        let v = ln.FromY-p.Y
        let w = ln.FromZ-p.Z
        let dot = x*u + y*v + z*w
        let t = dot/lenSq
        let x' = ln.FromX - x*t
        let y' = ln.FromY - y*t
        let z' = ln.FromZ - z*t
        Point3d(x', y', z')

    /// Returns closest point on (finite) line.
    /// Does not fails on very short curves.
    member inline ln.ClosestPoint (p:Point3d) =
        ln.EvaluateAt(ln.ClosestParameter(p))

    /// Assumes Line to be infinite.
    /// Returns square distance from point to infinite line.
    /// Fails on curves shorter than 1e-6 units. (ln.DistanceSqToPnt does not.)
    member ln.DistanceSqToPntInfinite(p:Point3d) =
        let lnFromX = ln.FromX
        let lnFromY = ln.FromY
        let lnFromZ = ln.FromZ
        //http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
        let x = lnFromX - ln.ToX
        let y = lnFromY - ln.ToY
        let z = lnFromZ - ln.ToZ
        let lenSq = x*x + y*y + z*z
        if isTooSmallSq lenSq  then // corresponds to a line Length of 1e-6
            RhinoScriptingFSharpException.Raise "Line.DistanceSqToPntInfiniteSq failed on very short line %O for point %O" ln p
        let u = lnFromX - p.X
        let v = lnFromY - p.Y
        let w = lnFromZ - p.Z
        let dot = x*u + y*v + z*w
        let t = dot/lenSq
        let x' = lnFromX - x*t
        let y' = lnFromY - y*t
        let z' = lnFromZ - z*t
        let u' = x' - p.X
        let v' = y' - p.Y
        let w' = z' - p.Z
        u'*u' + v'*v' + w'*w'

    /// Assumes Line to be infinite.
    /// Returns distance from point to infinite line.
    /// Fails on curves shorter than 1e-9 units. (ln.DistanceToPnt does not.)
    member inline ln.DistanceToPntInfinite(p:Point3d) =
        ln.DistanceSqToPntInfinite(p) |> sqrt

    /// Returns square distance from point to finite line.
    member inline ln.DistanceSqToPnt(p:Point3d) =
        p
        |> ln.ClosestParameter
        |> ln.EvaluateAt
        |> Point3d.distanceSq p

    /// Returns distance from point to (finite) line.
    member inline ln.DistanceToPnt(p:Point3d) =
        ln.DistanceSqToPnt(p) |> sqrt

    /// Checks if the angle between the two 3D lines is less than 180 degrees.
    /// Calculates the dot product of two 3D lines.
    /// Then checks if it is bigger than 1e-12.
    member inline ln.MatchesOrientation180 (otherLn:Line) =
        let dot = (otherLn.ToX-otherLn.FromX)*(ln.ToX-ln.FromX) + (otherLn.ToY-otherLn.FromY)*(ln.ToY-ln.FromY) + (otherLn.ToZ-otherLn.FromZ)*(ln.ToZ-ln.FromZ)
        dot > 1e-12

    /// Checks if the angle between the a 3D line and a 3D vector is less than 180 degrees.
    /// Calculates the dot product of both.
    /// Then checks if it is bigger than 1e-12.
    member inline ln.MatchesOrientation180 (v:Vector3d) =
        if isTooTinySq(v.LengthSq) then RhinoScriptingFSharpException.Raise "Line.MatchesOrientation180: Vector3d 'v' is too short: %s. 'ln':%s " v.AsString ln.AsString
        let dot = v.X*(ln.ToX-ln.FromX) + v.Y*(ln.ToY-ln.FromY) + v.Z*(ln.ToZ-ln.FromZ)
        dot > 1e-12


    /// Checks if the angle between the two 3D lines is less than 90 degrees.
    /// Calculates the dot product of the unit-vectors of the two 3D lines.
    /// Then checks if it is bigger than 0.707107 (cosine of 90 degrees).
    member inline ln.MatchesOrientation90 (otherLn:Line) =
        let dot = ln.UnitTangent * otherLn.UnitTangent
        dot > 0.707107

    /// Checks if two 3D lines are parallel.
    /// Ignores the line orientation.
    /// The default angle tolerance is 0.25 degrees.
    /// This tolerance can be customized by an optional minium cosine value.
    /// See Rhino.Scripting.FSharp.Cosine module.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.IsParallelTo( other:Line, [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine> ) =
        let a = ln.Vector
        let b = other.Vector
        let sa = a.LengthSq
        if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Line.IsParallelTo: Line 'ln' is too short: %s. 'other':%s " a.AsString b.AsString
        let sb = b.LengthSq
        if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Line.IsParallelTo: Line 'other' is too short: %s. 'ln':%s " b.AsString a.AsString
        let au = a * (1.0 / sqrt sa)
        let bu = b * (1.0 / sqrt sb)
        abs(bu * au) > float minCosine // 0.999990480720734 = cosine of 0.25 degrees:

    /// Checks if a 3D lines is parallel to a 3D vector.
    /// Ignores the line orientation.
    /// The default angle tolerance is 0.25 degrees.
    /// This tolerance can be customized by an optional minium cosine value.
    /// See Rhino.Scripting.FSharp.Cosine module.
    /// Fails on lines or vectors shorter than zeroLengthTolerance (1e-12).
    member inline ln.IsParallelTo( other:Vector3d, [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine> ) =
        let a = ln.Vector
        let b = other
        let sa = a.LengthSq
        if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Line2D.IsParallelTo: Line2D 'ln' is too short: %s. 'other':%s " a.AsString b.AsString
        let sb = b.LengthSq
        if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Line2D.IsParallelTo: Vector3d 'other' is too short: %s. 'ln':%s " b.AsString a.AsString
        let au = a * (1.0 / sqrt sa)
        let bu = b * (1.0 / sqrt sb)
        abs(bu * au) > float minCosine



    /// Checks if two 3D lines are parallel.
    /// Takes the line orientation into account too.
    /// The default angle tolerance is 0.25 degrees.
    /// This tolerance can be customized by an optional minium cosine value.
    /// See Rhino.Scripting.FSharp.Cosine module.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.IsParallelAndOrientedTo (other:Line, [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine> ) =
        let a = ln.Vector
        let b = other.Vector
        let sa = a.LengthSq
        if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Line.IsParallelAndOrientedTo: Line 'ln' is too short: %s. 'other':%s " a.AsString b.AsString
        let sb = b.LengthSq
        if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Line.IsParallelAndOrientedTo: Line 'other' is too short: %s. 'ln':%s " b.AsString a.AsString
        let au = a * (1.0 / sqrt sa)
        let bu = b * (1.0 / sqrt sb)
        bu * au > float minCosine // 0.999990480720734 = cosine of 0.25 degrees:

    /// Checks if a 3D lines is parallel to a 3D vector.
    /// Takes the line orientation into account too.
    /// The default angle tolerance is 0.25 degrees.
    /// This tolerance can be customized by an optional minium cosine value.
    /// See Rhino.Scripting.FSharp.Cosine module.
    /// Fails on lines or vectors shorter than zeroLengthTolerance (1e-12).
    member inline ln.IsParallelAndOrientedTo (other:Vector3d, [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine> ) =
        let a = ln.Vector
        let b = other
        let sa = a.LengthSq
        if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Line2D.IsParallelAndOrientedTo: Line2D 'ln' is too short: %s. 'other':%s " a.AsString b.AsString
        let sb = b.LengthSq
        if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Line2D.IsParallelAndOrientedTo: Vector3d 'other' is too short: %s. 'ln':%s " b.AsString a.AsString
        let au = a * (1.0 / sqrt sa)
        let bu = b * (1.0 / sqrt sb)
        bu * au > float minCosine



    /// Checks if two 3D lines are perpendicular to each other.
    /// The default angle tolerance is 89.75 to 90.25 degrees.
    /// This tolerance can be customized by an optional minium cosine value.
    /// The default cosine is 0.0043633 ( = 89.75 deg)
    /// See Rhino.Scripting.FSharp.Cosine module.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    member inline ln.IsPerpendicularTo (other:Line, [<OPT;DEF(Cosine.``89.75``)>] maxCosine:float<Cosine.cosine> ) =
        let a = ln.Vector
        let b = other.Vector
        let sa = a.LengthSq
        if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Line.IsPerpendicularTo: Line 'ln' is too short: %s. 'other':%s " a.AsString b.AsString
        let sb = b.LengthSq
        if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Line.IsPerpendicularTo: Line 'other' is too short: %s. 'ln':%s " b.AsString a.AsString
        let au = a * (1.0 / sqrt sa)
        let bu = b * (1.0 / sqrt sb)
        let d = bu * au
        float -maxCosine < d && d  < float maxCosine // = cosine of 98.75 and 90.25 degrees


    /// Checks if a 3D lines is perpendicular to a 3D vector.
    /// The default angle tolerance is 89.75 to 90.25 degrees.
    /// This tolerance can be customized by an optional minium cosine value.
    /// The default cosine is 0.0043633 ( = 89.75 deg)
    /// See Rhino.Scripting.FSharp.Cosine module.
    /// Fails on lines or vectors shorter than zeroLengthTolerance (1e-12).
    member inline ln.IsPerpendicularTo (other:Vector3d, [<OPT;DEF(Cosine.``89.75``)>] maxCosine:float<Cosine.cosine> ) =
        let a = ln.Vector
        let b = other
        let sa = a.LengthSq
        if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Line2D.IsPerpendicularTo: Line2D 'ln' is too short: %s. 'other':%s " a.AsString b.AsString
        let sb = b.LengthSq
        if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Line2D.IsPerpendicularTo: Vector3d 'other' is too short: %s. 'ln':%s " b.AsString a.AsString
        let au = a * (1.0 / sqrt sa)
        let bu = b * (1.0 / sqrt sb)
        let d = bu * au
        float -maxCosine < d && d  < float maxCosine // = cosine of 98.75 and 90.25 degrees


    /// Checks if two 3D lines are coincident within the distance tolerance. 1e-6 by default.
    /// This means that lines are parallel within the angle tolerance.
    /// and the distance of second start to the first line is less than the distance tolerance.
    /// Also returns false on lines shorter than zeroLengthTolerance (1e-12).
    /// The default angle tolerance is 0.25 degrees.
    /// This tolerance can be customized by an optional minium cosine value.
    /// See Rhino.Scripting.FSharp.Cosine module.
    member inline ln.IsCoincidentTo (other:Line,
                                    [<OPT;DEF(1e-6)>] distanceTolerance:float,
                                    [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine>) =
        let a = ln.Vector
        let b = other.Vector
        let sa = a.LengthSq
        if isTooTinySq(sa) then
            false
        else
            let sb = b.LengthSq
            if isTooTinySq(sb) then
                false
            else
                let au = a * (1.0 / sqrt sa)
                let bu = b * (1.0 / sqrt sb)
                abs(bu * au) > float minCosine // 0.999990480720734 = cosine of 0.25 degrees:
                &&
                let pX = other.FromX
                let pY = other.FromY
                let pZ = other.FromZ
                //http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
                let x = a.X
                let y = a.Y
                let z = a.Z
                let u = ln.FromX - pX
                let v = ln.FromY - pY
                let w = ln.FromZ - pZ
                let dot = x*u + y*v + z*w
                let t = dot/sa
                let x' = ln.FromX - x*t
                let y' = ln.FromY - y*t
                let z' = ln.FromZ - z*t
                let u' = x' - pX
                let v' = y' - pY
                let w' = z' - pZ
                u'*u' + v'*v' + w'*w' < distanceTolerance * distanceTolerance

    // /// Applies or multiplies a 4x4 transformation matrix to a 3D line.
    // member inline l.Transform (m:Matrix) = Line(l.From * m, l.To * m)

    // /// Multiplies (or applies) a RigidMatrix to a 3D line .
    // member inline l.TransformRigid (m:RigidMatrix) = Line(l.From * m, l.To * m)

    // /// Multiplies (or applies) a Quaternion to a 3D line .
    // /// The resulting line has the same length as the input.
    // member inline l.Rotate (q:Quaternion) =
    //     // adapted from https://github.com/mrdoob/three.js/blob/dev/src/math/Vector3.js
    //     let u = l.FromX
    //     let v = l.FromY
    //     let w = l.FromZ
    //     let x = l.ToX
    //     let y = l.ToY
    //     let z = l.ToZ
    //     let qx = q.X
    //     let qy = q.Y
    //     let qz = q.Z
    //     let qw = q.W
    //     let tu = 2.0 * ( qy * w - qz * v)
    //     let tv = 2.0 * ( qz * u - qx * w)
    //     let tw = 2.0 * ( qx * v - qy * u)
    //     let tx = 2.0 * ( qy * z - qz * y)
    //     let ty = 2.0 * ( qz * x - qx * z)
    //     let tz = 2.0 * ( qx * y - qy * x)
    //     Line( u + qw * tu + qy * tw - qz * tv ,
    //             v + qw * tv + qz * tu - qx * tw ,
    //             w + qw * tw + qx * tv - qy * tu ,
    //             x + qw * tx + qy * tz - qz * ty ,
    //             y + qw * ty + qz * tx - qx * tz ,
    //             z + qw * tz + qx * ty - qy * tx)

    // /// Multiplies (or applies) a Quaternion to a 3D line around a given center point.
    // /// The resulting line has the same length as the input.
    // member inline l.RotateWithCenter (cen:Point3d, q:Quaternion) =
    //     let u = l.FromX - cen.X
    //     let v = l.FromY - cen.Y
    //     let w = l.FromZ - cen.Z
    //     let x = l.ToX - cen.X
    //     let y = l.ToY - cen.Y
    //     let z = l.ToZ - cen.Z
    //     let qx = q.X
    //     let qy = q.Y
    //     let qz = q.Z
    //     let qw = q.W
    //     let tu = 2.0 * ( qy * w - qz * v)
    //     let tv = 2.0 * ( qz * u - qx * w)
    //     let tw = 2.0 * ( qx * v - qy * u)
    //     let tx = 2.0 * ( qy * z - qz * y)
    //     let ty = 2.0 * ( qz * x - qx * z)
    //     let tz = 2.0 * ( qx * y - qy * x)
    //     Line( u + qw * tu + qy * tw - qz * tv + cen.X ,
    //             v + qw * tv + qz * tu - qx * tw + cen.Y,
    //             w + qw * tw + qx * tv - qy * tu + cen.Z ,
    //             x + qw * tx + qy * tz - qz * ty + cen.X ,
    //             y + qw * ty + qz * tx - qx * tz + cen.Y ,
    //             z + qw * tz + qx * ty - qy * tx + cen.Z)



    //-------------------------------------------------------------------
    //------------------------static members-----------------------------
    //-------------------------------------------------------------------


    /// Checks if two 3D Lines are equal within tolerance.
    /// Identical Lines in opposite directions are not considered equal.
    /// Use a tolerance of 0.0 to check for an exact match.
    static member inline equals (tol:float) (a:Line) (b:Line) =
        abs (a.FromX - b.FromX) <= tol &&
        abs (a.FromY - b.FromY) <= tol &&
        abs (a.FromZ - b.FromZ) <= tol &&
        abs (a.ToX   - b.ToX  ) <= tol &&
        abs (a.ToY   - b.ToY  ) <= tol &&
        abs (a.ToZ   - b.ToZ  ) <= tol

    /// Checks if two 3D lines are coincident within tolerance.
    /// This means that lines are parallel within 0.25 degrees.
    /// and the distance of second start point to the first line is less than 1e-6.
    static member inline areCoincident (a:Line) (b:Line) =
        a.IsCoincidentTo(b)


    /// Creates a line starting at World Origin and going to along the given vector.
    static member inline createFromVec (v:Vector3d) =
        Line(0., 0., 0., v.X, v.Y, v.Z)

    /// Creates a line starting at given point and going to along the given vector.
    static member inline createFromPntAndVec (p:Point3d, v:Vector3d) =
        Line(p.X, p.Y, p.Z, p.X+v.X, p.Y+v.Y, p.Z+v.Z)

    /// Returns the Start point of the line. Same as Line.from.
    static member inline start (l:Line) =
        l.From

    /// Returns the Start point of the line. Same as Line.start.
    static member inline from (l:Line) =
        l.From

    /// Returns the Start point's X coordinate of the line.
    static member inline fromX (l:Line) =
        l.FromX

    /// Returns the Start point's Y coordinate of the line.
    static member inline fromY (l:Line) =
        l.FromY

    /// Returns the Start point's Z coordinate of the line.
    static member inline fromZ (l:Line) =
        l.FromZ

    /// Returns the End point of the line. Same as Line.to'
    static member inline ende (l:Line) =
        l.To

    /// Returns the End point of the line. Same as Line.ende.
    static member inline to' (l:Line) =
        l.To

    /// Returns the End point's X coordinate of the line.
    static member inline toX (l:Line) =
        l.ToX

    /// Returns the End point's Y coordinate of the line.
    static member inline toY (l:Line) =
        l.ToY

    /// Returns the End point's Z coordinate of the line.
    static member inline toZ (l:Line) =
        l.ToZ

    /// Set Line start point, returns a new line.
    static member inline setStart (pt:Point3d) (ln:Line) =
        Line(pt.X, pt.Y, pt.Z, ln.ToX, ln.ToY, ln.ToZ)

    /// Set Line end point, returns a new line.
    static member inline setEnd (pt:Point3d) (ln:Line) =
        Line(ln.FromX, ln.FromY, ln.FromZ, pt.X, pt.Y, pt.Z)

    /// Same as Line.vector or Line.tangent.
    /// The returned vector has the same length as the Line.
    static member inline direction (ln:Line) =
        Vector3d(ln.ToX-ln.FromX, ln.ToY-ln.FromY, ln.ToZ-ln.FromZ)

    /// Same as Line.tangent or Line.direction.
    /// The returned vector has the same length as the Line.
    static member inline vector (ln:Line) =
        Vector3d(ln.ToX-ln.FromX, ln.ToY-ln.FromY, ln.ToZ-ln.FromZ)

    /// Same as Line.vector or Line.direction.
    /// The returned vector has the same length as the Line.
    static member inline tangent (ln:Line) =
        Vector3d(ln.ToX-ln.FromX, ln.ToY-ln.FromY, ln.ToZ-ln.FromZ)

    /// Returns a unit-vector of the line Direction.
    static member inline unitTangent (ln:Line) =
        ln.UnitTangent

    /// Returns the length of the line.
    static member inline length (l:Line) =
        l.Length

    /// Returns the square length of the line.
    static member inline lengthSq (l:Line) =
        l.LengthSq

    /// Check if the line has same starting and ending point.
    static member inline isZeroLength (l:Line) =
        l.IsZeroLength

    /// Check if line is shorter than tolerance.
    /// Also checks if any component is a NaN.
    static member inline isTiny tol (l:Line) =
        l.Length < tol

    /// Check if the lines square length is shorter than squared tolerance.
    /// Also checks if any component is a NaN.
    static member inline isTinySq tol (l:Line) =
        not (l.LengthSq > tol)

    /// Checks if 3D line is parallel to the world X axis. Ignoring orientation.
    /// Tolerance is 1e-6.
    /// Fails on lines shorter than 1e-6.
    static member inline isXAligned (l:Line) =
        l.IsXAligned

    /// Checks if 3D line is parallel to the world Y axis. Ignoring orientation.
    /// Tolerance is 1e-6.
    /// Fails on lines shorter than 1e-6.
    static member inline isYAligned (l:Line) =
        l.IsYAligned

    /// Checks if 3D line is parallel to the world Z axis. Ignoring orientation.
    /// Tolerance is 1e-6.
    /// Fails on lines shorter than 1e-6.
    /// Same as ln.IsVertical
    static member inline isZAligned (l:Line) =
        l.IsZAligned

    /// Checks if 3D line is parallel to the world Z axis. Ignoring orientation.
    /// Tolerance is 1e-6.
    /// Fails on lines shorter than 1e-6.
    /// Same as ln.IsZAligned
    static member inline isVertical (l:Line) =
        l.IsVertical

    /// Checks if 3D line is horizontal (Z component is almost zero).
    /// Tolerance is 1e-6.
    /// Fails on lines shorter than 1e-6.
    static member inline isHorizontal (l:Line) =
        l.IsHorizontal

    /// Evaluate line at a given parameter (parameters 0.0 to 1.0 are on the line)
    static member inline evaluateAt t (ln:Line) =
        ln.EvaluateAt t

    /// Get point at center of line.
    static member inline mid (ln:Line) =
        ln.Mid

    /// Reverse or flip the 3D line (same as Line.flip)
    static member inline reverse (ln:Line) =
        ln.Reversed

    /// Reverse or flip the 3D line (same as Line.reverse)
    static member inline flip (ln:Line) =
        ln.Reversed

    /// Returns new 3D line from point at Parameter a to point at Parameter b.
    static member inline segment a b (ln:Line) =
        ln.Segment (a, b)

    /// Move a 3D line by a vector. (same as Line.move)
    static member inline translate (v:Vector3d) (ln:Line) =
        ln.Move(v)

    /// Returns a 3D line moved by a given distance in X direction.
    static member inline moveX (distance:float) (ln:Line) =
        ln.MoveX(distance)

    /// Returns a 3D line moved by a given distance in Y direction.
    static member inline moveY (distance:double) (ln:Line) =
        ln.MoveY(distance)

    /// Returns a 3D line moved by a given distance in Z direction.
    static member inline moveZ (distance:double) (ln:Line) =
        ln.MoveZ(distance)

    /// Move a 3D line by a vector. (same as Line.translate)
    static member inline move (v:Vector3d) (ln:Line) =
        ln.Move(v)

    // /// Applies or multiplies a 4x4 transformation matrix to a 3D line.
    // static member inline transform (m:Matrix) (ln:Line) =
    //     ln.Transform m

    // /// Multiplies (or applies) a RigidMatrix to a 3D line .
    // static member inline transformRigid (m:RigidMatrix) (ln:Line) =
    //     ln.TransformRigid m

    // /// Multiplies (or applies) a Quaternion to a 3D line.
    // /// The resulting line has the same length as the input.
    // static member inline rotate(q:Quaternion) (ln:Line) =
    //     ln.Rotate q

    // /// Multiplies (or applies) a Quaternion to a 3D line around a given center point.
    // /// The resulting line has the same length as the input.
    // static member inline rotateWithCenter (cen:Point3d) (q:Quaternion) (ln:Line) =
    //     ln.RotateWithCenter (cen, q)


    // /// Rotation a 3D line around Z-Axis.
    // static member inline rotate2D (r:Rotation2D) (ln:Line) =
    //     Line(Point3d.rotateZBy r ln.From, Point3d.rotateZBy r ln.To)

    // /// Rotation a 3D line round given Center point an a local Z-axis.
    // static member inline rotate2dOn (cen:Point3d) (r:Rotation2D) (ln:Line) =
    //     Line(Point3d.rotateZwithCenterBy cen r ln.From, Point3d.rotateZwithCenterBy cen r ln.To)

    /// Ensure 3D line has a positive dot product with given orientation line.
    static member inline matchOrientation (orientationToMatch:Line) (lineToFlip:Line) =
        if orientationToMatch.Vector * lineToFlip.Vector  < 0.0 then lineToFlip.Reversed else lineToFlip

    /// Ensure 3D line has a positive dot product with given 3D vector.
    static member inline matchVecOrientation (orientationToMatch:Vector3d) (lineToFlip:Line) =
        if orientationToMatch * lineToFlip.Vector  < 0.0 then lineToFlip.Reversed else lineToFlip


    /// Checks if the angle between the two 3D lines is less than 180 degrees.
    /// Calculates the dot product of two 3D lines.
    /// Then checks if it is positive.
    static member inline matchesOrientation180 (l:Line) (ln:Line) =
        l.MatchesOrientation180 ln

    /// Checks if the angle between the two 3D lines is less than 90 degrees.
    /// Calculates the dot product of the unit-vectors of the two 3D lines.
    /// Then checks if it is bigger than 0.707107 (cosine of 90 degrees).
    static member inline matchesOrientation90 (l:Line) (ln:Line) =
        l.MatchesOrientation90 ln

    /// Checks if two 3D lines are parallel. Ignoring orientation.
    /// Calculates the cross product of the two line vectors. (= the area of the parallelogram)
    /// And checks if it is smaller than 1e-9
    /// (NOTE: for very long lines a higher tolerance might be needed)
    static member inline areParallel (l:Line) (ln:Line) =
        l.IsParallelTo ln

    /// Checks if two 3D lines are parallel and orientated the same way.
    /// Calculates the cross product of the two line vectors. (= the area of the parallelogram)
    /// And checks if it is smaller than 1e-9
    /// Then calculates the dot product and checks if it is positive.
    /// (NOTE: for very long lines a higher tolerance might be needed)
    static member inline areParallelAndMatchOrientation (l:Line) (ln:Line) =
        l.IsParallelAndOrientedTo ln

    /// Checks if two 3D lines are perpendicular.
    /// Calculates the dot product and checks if it is smaller than 1e-9.
    /// (NOTE: for very long lines a higher tolerance might be needed)
    static member inline arePerpendicular(l:Line) (ln:Line) =
        l.IsPerpendicularTo(ln)

    /// Assumes Line to be infinite.
    /// Returns the parameter at which a point is closest to the infinite line.
    /// If it is smaller than 0.0 or bigger than 1.0 it is outside of the finite line.
    static member inline closestParameterInfinite (p:Point3d) (ln:Line) =
        ln.ClosestParameterInfinite p

    /// Returns the parameter at which a point is closest to the (finite) line.
    /// The result is between 0.0 and 1.0.
    static member inline closestParameter (p:Point3d) (ln:Line) =
        ln.ClosestParameter p

    /// Assumes Line to be infinite.
    /// Returns closest point on infinite line.
    static member inline closestPointInfinite (p:Point3d) (ln:Line) =
        ln.ClosestPointInfinite p

    /// Returns closest point on (finite) line.
    static member inline closestPoint (p:Point3d) (ln:Line) =
        ln.ClosestPoint p

    /// Assumes Line to be infinite.
    /// Returns the square distance from point to infinite line.
    static member inline distanceSqToPntInfinite(p:Point3d) (ln:Line) =
        ln.DistanceSqToPntInfinite p

    /// Assumes Line to be infinite.
    /// Returns distance from point to infinite line.
    static member inline distanceToPntInfinite(p:Point3d) (ln:Line) =
        ln.DistanceToPntInfinite p

    /// Returns the square distance from point to (finite) line.
    static member inline distanceSqToPnt(p:Point3d) (ln:Line) =
        ln.DistanceSqToPnt p

    /// Returns distance from point to (finite) line.
    static member inline distanceToPnt(p:Point3d) (ln:Line) =
        ln.DistanceToPnt p

    /// Get distance from start of line to point projected onto line, may be negative.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline lengthToPtOnLine (ln:Line) pt =
        let t = ln.Vector
        let l = t.Length
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.lengthToPtOnLine %O to short for finding length to point." ln
        (t/l) * (pt-ln.From)

    /// Extend 3D line by absolute amount at start and end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline extend (distAtStart:float) (distAtEnd:float) (ln:Line) =
        ln.Extend(distAtStart, distAtEnd)

    /// Extend 3D line by absolute amount at start.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline extendStart (distAtStart:float) (ln:Line) =
        ln.ExtendStart(distAtStart)

    /// Extend 3D line by absolute amount at end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline extendEnd (distAtEnd:float) (ln:Line) =
        ln.ExtendEnd(distAtEnd)

        /// Extend 3D line by relative amount at start and end.
    /// A relative amount of 0.5 extends the line by half its length on each side.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline extendRel (relAtStart:float) (relAtEnd:float) (ln:Line) =
        ln.ExtendRel(relAtStart, relAtEnd)

    /// Extend 3D line by relative amount at start.
    /// A relative amount of 0.5 extends the line by half its length.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline extendStartRel (relAtStart:float) (ln:Line) =
        ln.ExtendStartRel(relAtStart)

    /// Extend 3D line by relative amount at end.
    /// A relative amount of 0.5 extends the line by half its length.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline extendEndRel (relAtEnd:float) (ln:Line) =
        ln.ExtendEndRel(relAtEnd)

    /// Shrink 3D line by absolute amount at start and end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline shrink (distAtStart:float) (distAtEnd:float) (ln:Line) =
        ln.Shrink(distAtStart, distAtEnd)

    /// Shrink 3D line by absolute amount at start.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline shrinkStart (distAtStart:float) (ln:Line) =
        ln.ShrinkStart(distAtStart)

    /// Shrink 3D line by absolute amount at end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline shrinkEnd (distAtEnd:float) (ln:Line) =
        ln.ShrinkEnd(distAtEnd)

    /// Finds point at given distance from line start.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline pointAtDistance dist (ln:Line) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let len = sqrt(x*x + y*y + z*z)
        if isTooTiny len then RhinoScriptingFSharpException.Raise "Line.pointAtDistance %O to short for finding point at a distance." ln
        let f = dist/len
        Point3d(ln.FromX + x*f,
            ln.FromY + y*f,
            ln.FromZ + z*f)

    /// Returns new Line with given length, going out from start in direction of end.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline withLengthFromStart len (ln:Line) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.withLengthFromStart %O to short for finding point at a distance." ln
        let f = len/l
        Line( ln.FromX,
                ln.FromY,
                ln.FromZ,
                ln.FromX + x*f,
                ln.FromY + y*f,
                ln.FromZ + z*f)

    /// Returns new Line ending at current LineEnd with given length coming from direction of start.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline withLengthToEnd len (ln:Line) =
        let x = ln.FromX-ln.ToX
        let y = ln.FromY-ln.ToY
        let z = ln.FromZ-ln.ToZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.withLengthToEnd %O to short for finding point at a distance." ln
        let f = len/l
        Line( ln.ToX + x*f,
                ln.ToY + y*f,
                ln.ToZ + z*f,
                ln.ToX,
                ln.ToY,
                ln.ToZ)

    /// Returns new Line with given length. Missing length is added to or subtracted from both the end and start of the line.
    /// Fails on lines shorter than zeroLengthTolerance (1e-12).
    static member inline withLengthFromMid len (ln:Line) =
        let x = ln.FromX-ln.ToX
        let y = ln.FromY-ln.ToY
        let z = ln.FromZ-ln.ToZ
        let l = sqrt(x*x + y*y + z*z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Line.withLengthFromMid %O to short for finding point at a distance." ln
        let f = (len/l + 1.0) * 0.5
        Line( ln.ToX   + x*f,
                ln.ToY   + y*f,
                ln.ToZ   + z*f,
                ln.FromX - x*f,
                ln.FromY - y*f,
                ln.FromZ - z*f)

    /// Offset line parallel to XY-Plane to left side in line direction.
    /// Z values are not changed.
    /// Fails on vertical lines or lines shorter than zeroLengthTolerance (1e-12).
    static member offsetXY amount (ln:Line) =
        let x = ln.ToX - ln.FromX
        let y = ln.ToY - ln.FromY
        let lenXY = sqrt (x*x + y*y)
        if isTooTiny (lenXY ) then RhinoScriptingFSharpException.Raise "Line.offset: Cannot offset vertical Line (by %g) %O" amount ln
        let ox = -y*amount/lenXY  // unitized, horizontal, perpendicular  vector
        let oy =  x*amount/lenXY  // unitized, horizontal, perpendicular  vector
        Line( ln.FromX+ox,
                ln.FromY+oy,
                ln.FromZ,
                ln.ToX+ox,
                ln.ToY+oy,
                ln.ToZ)

    /// Divides a 3D line into given amount of segments.
    /// Returns an array of 3D points of length: segment count + 1.
    /// Includes start and endpoint of line.
    static member divide (segments:int) (ln:Line) : Point3d[] =
        match segments with
        | x when x < 1 -> RhinoScriptingFSharpException.Raise "Line.divide failed for %d segments. Minimum one. for %O"  segments ln
        | 1 -> [|ln.From;  ln.To|]
        | k ->
            let x = ln.ToX - ln.FromX
            let y = ln.ToY - ln.FromY
            let z = ln.ToZ - ln.FromZ
            let sx = ln.FromX
            let sy = ln.FromY
            let sz = ln.FromZ
            let kk = float k
            let r = Array.zeroCreate (k+1)
            r.[0] <- ln.From
            for i = 1 to k-1 do
                let t = float i / kk
                r.[i] <- Point3d(sx + x*t, sy + y*t, sz + z*t)
            r.[k] <- ln.To
            r


    /// Divides a 3D line into as many as segments as possible respecting the minimum segment length.
    /// Returned Array includes start and endpoint of line.
    /// The input minSegmentLength is multiplied by factor 1.000001 of to avoid numerical errors.
    /// That means in an edge case there are more segments returned, not fewer.
    static member divideMinLength (minSegmentLength:float) (ln:Line) : Point3d[] =
        let len = ln.Length
        if len < minSegmentLength then
            RhinoScriptingFSharpException.Raise "Line.divideMinLength: minSegmentLength %g is bigger than line length %g for %O"  minSegmentLength len ln
        let k = int (len / (minSegmentLength*1.00000095367431640625)) // 8 float steps above 1.0 https://float.exposed/0x3f800008
        Line.divide k ln


    /// Divides a 3D line into as few as segments as possible respecting the maximum segment length.
    /// Returned Array includes start and endpoint of line.
    /// The input maxSegmentLength is multiplied by factor 0.999999 of to avoid numerical  errors.
    /// That means in an edge case there are fewer segments returned, not more.
    static member divideMaxLength (maxSegmentLength:float) (ln:Line) : Point3d[] =
        let len = ln.Length
        let k = int (len / maxSegmentLength*0.999999523162841796875) + 1 // 8 float steps below 1.0 https://float.exposed/0x3f7ffff8
        Line.divide k ln

    /// Divides a 3D line into given amount of segments.
    /// Includes a gap between the segments. But not at the start or end.
    /// Returns an array of 3D Lines.
    /// Returns an empty array if the length of the line is less than gap-size x segment-count-minus-1.
    static member split (gap:float) (segments:int) (ln:Line) : Line[] =
        if segments <= 0  then
            RhinoScriptingFSharpException.Raise "Line.split failed for %d segments. Minimum one. for %O"  segments ln
        let v = ln.Vector
        let len = v.Length
        let lenMinusGaps = len - gap * float (segments-1)
        let segLen = lenMinusGaps / float segments
        if isTooTiny (segLen) then
            [||]
        else
            let lns = Array.zeroCreate segments
            let vx = v.X
            let vy = v.Y
            let vz = v.Z
            let x = ln.FromX
            let y = ln.FromY
            let z = ln.FromZ
            for i = 0 to segments-1 do
                let g = float i
                let s = float (i+1)
                let sf = (g*segLen + g*gap)/len
                let ef = (s*segLen + g*gap)/len
                let xs = x + vx*sf
                let ys = y + vy*sf
                let zs = z + vz*sf
                let xe = x + vx*ef
                let ye = y + vy*ef
                let ze = z + vz*ef
                lns.[i] <- Line(xs,ys,zs,xe,ye,ze)
            // correct last point to avoid numerical errors
            lns.[segments-1] <- Line.setEnd ln.To lns.[segments-1]
            lns

    /// Divides a 3D line into as many as segments as possible respecting the minimum segment length and the gap.
    /// Includes a gap between the segments. But not at the start or end.
    /// Returns an array ofe3D Lines
    /// The input minSegmentLength is multiplied by factor 1.000001 of to avoid numerical errors.
    /// That means in an edge case there are more segments returned, not fewer.
    static member splitMinLength (gap:float) (minSegmentLength:float) (ln:Line) : Line[] =
        let len = ln.Length
        if len < minSegmentLength then
            RhinoScriptingFSharpException.Raise "Line.splitMinLength: minSegmentLength %g is bigger than line length %g for %O"  minSegmentLength len ln
        let k = int ((len+gap) / ((minSegmentLength+gap)*1.000000953)) // 8 float steps above 1.0 https://float.exposed/0x3f800008
        Line.split gap k ln


    /// Divides a 3D line into as few as segments as possible respecting the maximum segment length and the gap.
    /// Includes a gap between the segments. But not at the start or end.
    /// Returns an array ofe3D Lines
    /// The input maxSegmentLength is multiplied by factor 0.999999 of to avoid numerical errors.
    /// That means in an edge case there are fewer segments returned, not more.
    static member splitMaxLength (gap:float) (maxSegmentLength:float) (ln:Line)  : Line[] =
        let len = ln.Length
        let k = int ((len+gap) / (maxSegmentLength+gap)*0.999999523) + 1 // 8 float steps below 1.0 https://float.exposed/0x3f7ffff8
        Line.split gap k ln


    /// Divides a 2D line into segments of given length.
    /// Includes start and end point
    /// Adds end point only if there is a remainder bigger than 1% of the segment length.
    static member  divideEvery dist (l:Line) =
        let len = l.Length
        let div = len / dist
        let floor = System.Math.Floor div
        let step = 1.0 / floor
        let count = int floor
        let pts = ResizeArray<Point3d>(count + 2)
        pts.Add l.From
        for i = 1 to count do
            pts.Add <| l.EvaluateAt (step * float i)
        if div - floor > 0.01 then
            pts.Add l.To // add end point only if there is a remainder bigger than 1%
        pts

    /// Divides a 2D line into segments of given length.
    /// Excludes start and end point
    /// Adds last div point before end only if there is a remainder bigger than 1% of the segment length.
    static member divideInsideEvery dist (l:Line) =
        let len = l.Length
        let div = len / dist
        let floor = System.Math.Floor div
        let step = 1.0 / floor
        let count = int floor
        let pts = ResizeArray<Point3d>(count)
        for i = 1 to count - 1 do
            pts.Add <| l.EvaluateAt (step * float i)
        if div - floor > 0.01 then
            pts.Add <| l.EvaluateAt (step * floor) // add last div point only if there is a remainder bigger than 1%
        pts


    // ----------------- originally from RhinoScriptSyntax.Line not in Euclid -----------------


    /// Finds intersection of two Infinite Lines.
    /// Fails if lines are parallel or skew by more than 1e-6 units
    /// Considers Lines Infinite
    /// Returns point on lnB (the last parameter)
    static member intersectInOnePoint (lnA:Line) (lnB:Line) : Point3d =
        let ok, ta, tb = Intersect.Intersection.LineLine(lnA,lnB)
        if not ok then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.Line.intersectInOnePoint failed, parallel ?  on %s and %s" lnA.Pretty lnB.Pretty
        let a = lnA.PointAt(ta)
        let b = lnB.PointAt(tb)
        if (a-b).SquareLength > RhinoMath.ZeroTolerance then // = Length > 1e-6
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.Line.intersect intersectInOnePoint, they are skew. distance: %g  on %s and %s" (a-b).Length lnA.Pretty lnB.Pretty
        b

    /// Finds intersection of two Infinite Lines.
    /// Returns a point for each line where they are the closest to each other.
    /// (in same order as input)
    /// Fails if lines are parallel.
    /// Considers Lines infinite
    static member intersectSkew (lnA:Line) (lnB:Line) :Point3d*Point3d=
        let ok, ta, tb = Intersect.Intersection.LineLine(lnA,lnB)
        if not ok then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.Line.intersectSkew failed, parallel ?  on %s and %s" lnA.Pretty lnB.Pretty
        let a = lnA.PointAt(ta)
        let b = lnB.PointAt(tb)
        a,b


    /// Checks if two Finite lines intersect.
    /// Also returns true for skew lines if the virtual Intersection Point's Domain is between 0.0 and 1.0 for both Lines
    static member doIntersectFinite (lnA:Line) (lnB:Line) : bool=
        let ok, ta, tb = Intersect.Intersection.LineLine(lnA,lnB)
        if ok then
            0.0 <= ta && ta <= 1.0 && 0.0 <= tb && tb <= 1.0
        else
            false


    /// Finds intersection of two Finite Lines.
    /// Returns:
    ///    an empty array if they are parallel,
    ///    an array with one point if they intersect by RhinoScriptSyntax.Doc.ModelAbsoluteTolerance (Point will be the average of the two points within the tolerance)
    ///    an array with two points where they are the closest to each other. In same order as input. They might be skew or they might intersect only when infinite.
    /// Fails if lines are parallel.
    /// Considers Lines finite
    static member intersectFinite (lnA:Line) (lnB:Line) : Point3d[]=
        let ok, ta, tb = Intersect.Intersection.LineLine(lnA,lnB)
        if not ok then [||] //RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.Line.intersectFinite failed, parallel ?  on %s and %s" lnA.Pretty lnB.Pretty
        else
            let inline clamp01 x =
                if x < 0.0 then 0.0
                elif x > 1.0 then 1.0
                else x


            let ca = clamp01 ta
            let cb = clamp01 tb
            let a = lnA.PointAt(ca)
            let b = lnB.PointAt(cb)
            let d = Point3d.distance a b
            if  d < RhinoScriptSyntax.Doc.ModelAbsoluteTolerance * 0.5 then
                if d < RhinoMath.ZeroTolerance then [|a|]
                else [| Point3d.divPt (a, b, 0.5)|]
            else [|a ; b|]

    /// Returns the distance between two Infinite Lines.
    /// At the point where they are closest to each other.
    /// works even if lines are parallel.
    static member distanceToLine (lnA:Line) (lnB:Line) :float=
        let ok, ta, tb = Intersect.Intersection.LineLine(lnA,lnB)
        if ok then
            let a = lnA.PointAt(ta)
            let b = lnB.PointAt(tb)
            (a-b).Length
        else// parallel
            let pt = lnA.ClosestPoint(lnB.From, limitToFiniteSegment=false)
            (pt-lnB.From).Length

    /// Returns a new transformed Line
    static member transform(xForm:Transform) (line:Line) =
        let ln = Line(line.From,line.To)
        if ln.Transform(xForm) then
            ln
        else
            RhinoScriptingFSharpException.Raise "Line.transform failed on line %A with  %A" line xForm

